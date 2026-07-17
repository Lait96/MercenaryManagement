using System.Collections.Generic;
using System.Linq;
using MercenaryManagement.Contracts;
using MercenaryManagement.Services;
using TaleWorlds.CampaignSystem;

namespace MercenaryManagement.Behaviors;

internal sealed class MercenaryOfferBehavior : CampaignBehaviorBase
{
    private const float OfferLifetimeDays = 7f;
    private Dictionary<string, MercenaryOffer> _offers = new Dictionary<string, MercenaryOffer>();

    public override void RegisterEvents()
    {
        CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, OnSessionLaunched);
        CampaignEvents.DailyTickEvent.AddNonSerializedListener(this, OnDailyTick);
    }

    public override void SyncData(IDataStore dataStore)
    {
        dataStore.SyncData("MercenaryManagement_Offers", ref _offers);
        _offers ??= new Dictionary<string, MercenaryOffer>();
    }

    public IEnumerable<MercenaryOffer> GetOffersFor(Clan clan)
    {
        return _offers.Values.Where(offer => offer.ClanId == clan.StringId && IsValid(offer));
    }

    public void RefreshPlayerOffers()
    {
        Reconcile();
        GeneratePlayerOffers();
    }

    public MercenaryOffer? RequestOffer(Clan clan, Kingdom kingdom)
    {
        if (MercenaryHiringService.TryGetPlayerOfferFailureReason(clan, kingdom, out _)) return null;
        MercenaryOffer offer = CreateOffer(clan, kingdom, true);
        _offers[GetKey(clan, kingdom)] = offer;
        return offer;
    }

    public bool TryAccept(MercenaryOffer offer)
    {
        Clan? clan = offer.Clan;
        Kingdom? kingdom = offer.Kingdom;
        if (clan is null || kingdom is null || !IsValid(offer)
            || MercenaryHiringService.TryGetPlayerOfferFailureReason(clan, kingdom, out _))
        {
            return false;
        }

        MercenaryHiringService.Hire(clan, kingdom, offer);
        RemoveOffersFor(clan);
        return clan.IsUnderMercenaryService && clan.Kingdom == kingdom;
    }

    private void OnSessionLaunched(CampaignGameStarter starter)
    {
        Reconcile();
        GeneratePlayerOffers();
    }

    private void OnDailyTick()
    {
        Reconcile();
        GeneratePlayerOffers();
    }

    private void GeneratePlayerOffers()
    {
        Clan clan = Clan.PlayerClan;
        if (clan.Kingdom is not null || clan.Fiefs.Count > 0 || clan.Leader != Hero.MainHero) return;

        foreach (Kingdom kingdom in Kingdom.All.Where(kingdom =>
                     !MercenaryHiringService.TryGetPlayerOfferFailureReason(clan, kingdom, out _)))
        {
            string key = GetKey(clan, kingdom);
            if (_offers.TryGetValue(key, out MercenaryOffer existing) && IsValid(existing)) continue;

            float need = MercenaryHiringService.CalculateMilitaryNeed(kingdom);
            int relation = clan.Leader?.GetRelation(kingdom.Leader) ?? 0;
            if (need >= 0.3f || relation >= 10)
            {
                _offers[key] = CreateOffer(clan, kingdom, false);
            }
        }
    }

    private static MercenaryOffer CreateOffer(Clan clan, Kingdom kingdom, bool requested)
    {
        int basePrice = MercenaryHiringService.CalculateHirePrice(clan, kingdom);
        float need = MercenaryHiringService.CalculateMilitaryNeed(kingdom);
        MercenaryContractBehavior? contracts = Campaign.Current?
            .GetCampaignBehavior<MercenaryContractBehavior>();
        float reliability = contracts?.GetReliability(clan) ?? 50f;
        float multiplier = 0.9f + need * 0.25f + (reliability - 50f) / 500f;
        if (requested) multiplier -= 0.05f;
        int price = System.Math.Max(
            MercenaryPaymentService.CalculateMinimumInfluencePrice(clan),
            System.Math.Min(MercenaryPaymentService.CalculateMaximumInfluencePrice(clan),
                (int)System.Math.Round(basePrice * multiplier)));
        MercenaryContractDuration duration = need >= 0.75f
            ? MercenaryContractDuration.Days120
            : need >= 0.4f ? MercenaryContractDuration.Days60 : MercenaryContractDuration.Days30;
        return new MercenaryOffer(clan, kingdom, price,
            MercenaryHiringService.GetSuggestedPriority(kingdom), duration,
            CampaignTime.DaysFromNow(OfferLifetimeDays));
    }

    private void Reconcile()
    {
        foreach (MercenaryOffer offer in _offers.Values)
        {
            Clan? clan = Clan.All.FirstOrDefault(item => item.StringId == offer.ClanId);
            Kingdom? kingdom = Kingdom.All.FirstOrDefault(item => item.StringId == offer.KingdomId);
            if (clan is not null && kingdom is not null) offer.AttachParticipants(clan, kingdom);
        }
        var invalidKeys = _offers.Where(pair => !IsValid(pair.Value)).Select(pair => pair.Key).ToArray();
        foreach (string key in invalidKeys) _offers.Remove(key);
    }

    private static bool IsValid(MercenaryOffer offer)
    {
        return !offer.IsExpired && offer.InfluencePrice > 0
               && offer.Clan is { IsEliminated: false, Kingdom: null }
               && offer.Kingdom is { IsEliminated: false };
    }

    private void RemoveOffersFor(Clan clan)
    {
        foreach (string key in _offers.Where(pair => pair.Value.ClanId == clan.StringId)
                     .Select(pair => pair.Key).ToArray()) _offers.Remove(key);
    }

    private static string GetKey(Clan clan, Kingdom kingdom) => clan.StringId + "|" + kingdom.StringId;
}
