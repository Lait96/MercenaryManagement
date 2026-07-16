using System.Collections.Generic;
using MercenaryManagement.Contracts;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;

namespace MercenaryManagement.Behaviors;

internal sealed class MercenaryContractBehavior : CampaignBehaviorBase
{
    private Dictionary<string, MercenaryContract> _contracts =
        new Dictionary<string, MercenaryContract>();
    private Dictionary<string, int> _legacyInfluencePrices = new Dictionary<string, int>();

    public override void RegisterEvents()
    {
        CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(
            this,
            OnSessionLaunched);
        CampaignEvents.OnClanChangedKingdomEvent.AddNonSerializedListener(
            this,
            OnClanChangedKingdom);
    }

    public override void SyncData(IDataStore dataStore)
    {
        dataStore.SyncData("MercenaryManagement_Contracts", ref _contracts);

        // Keep reading the old key so campaigns saved before contracts were introduced
        // retain their individually configured prices.
        dataStore.SyncData(
            "MercenaryManagement_InfluencePrices",
            ref _legacyInfluencePrices);

        _contracts ??= new Dictionary<string, MercenaryContract>();
        _legacyInfluencePrices ??= new Dictionary<string, int>();
    }

    public bool TryGetContract(Clan clan, out MercenaryContract contract)
    {
        if (clan.IsUnderMercenaryService
            && clan.Kingdom is not null
            && _contracts.TryGetValue(clan.StringId, out contract)
            && contract.IsActive
            && contract.Matches(clan.StringId, clan.Kingdom.StringId))
        {
            bool isLegacyContract = contract.Clan is null
                                    || contract.EmployerKingdom is null;
            contract.AttachParticipants(clan, clan.Kingdom);
            if (isLegacyContract && contract.EndTime == CampaignTime.Zero)
            {
                contract.SetEndTime(clan.ShouldStayInKingdomUntil);
            }

            contract.EnsureServiceStartTime();
            return true;
        }

        contract = null!;
        return false;
    }

    public bool GetOrCreateContract(Clan clan, out MercenaryContract contract)
    {
        if (TryGetContract(clan, out contract))
        {
            return true;
        }

        if (!clan.IsUnderMercenaryService || clan.Kingdom is null)
        {
            contract = null!;
            return false;
        }

        int influencePrice = clan.MercenaryAwardMultiplier;
        if (_legacyInfluencePrices.TryGetValue(clan.StringId, out int legacyInfluencePrice))
        {
            influencePrice = legacyInfluencePrice;
        }

        return StartContract(
            clan,
            clan.Kingdom,
            influencePrice,
            clan.ShouldStayInKingdomUntil,
            out contract);
    }

    public bool StartContract(
        Clan clan,
        Kingdom kingdom,
        int influencePrice,
        CampaignTime endTime,
        out MercenaryContract contract)
    {
        if (!clan.IsUnderMercenaryService || clan.Kingdom != kingdom)
        {
            contract = null!;
            return false;
        }

        if (TryGetContract(clan, out contract))
        {
            contract.SetInfluencePrice(influencePrice);
            contract.SetEndTime(endTime);
            return true;
        }

        CompleteContract(clan);
        contract = new MercenaryContract(clan, kingdom, influencePrice, endTime);
        _contracts[clan.StringId] = contract;
        _legacyInfluencePrices.Remove(clan.StringId);
        return true;
    }

    public void SetInfluencePrice(Clan clan, int influencePrice)
    {
        if (GetOrCreateContract(clan, out MercenaryContract contract))
        {
            contract.SetInfluencePrice(influencePrice);
        }
    }

    public bool SetServiceStartTime(Clan clan, CampaignTime serviceStartTime)
    {
        if (!GetOrCreateContract(clan, out MercenaryContract contract))
        {
            return false;
        }

        contract.SetServiceStartTime(serviceStartTime);
        return true;
    }

    public bool SetContractEndTime(Clan clan, CampaignTime endTime)
    {
        if (!GetOrCreateContract(clan, out MercenaryContract contract))
        {
            return false;
        }

        contract.SetEndTime(endTime);
        return true;
    }

    public bool CompleteContract(Clan clan)
    {
        _legacyInfluencePrices.Remove(clan.StringId);

        if (!_contracts.TryGetValue(clan.StringId, out MercenaryContract contract)
            || !contract.IsActive)
        {
            return false;
        }

        if (contract.Clan is null || contract.EmployerKingdom is null)
        {
            foreach (Kingdom kingdom in Kingdom.All)
            {
                if (kingdom.StringId == contract.KingdomId)
                {
                    contract.AttachParticipants(clan, kingdom);
                    break;
                }
            }
        }

        contract.Complete(CampaignTime.Now);
        return true;
    }

    private void OnSessionLaunched(CampaignGameStarter campaignGameStarter)
    {
        foreach (Clan clan in Clan.All)
        {
            if (clan.IsUnderMercenaryService && clan.Kingdom is not null)
            {
                GetOrCreateContract(clan, out _);
                continue;
            }

            CompleteContract(clan);
        }
    }

    private void OnClanChangedKingdom(
        Clan clan,
        Kingdom oldKingdom,
        Kingdom newKingdom,
        ChangeKingdomAction.ChangeKingdomActionDetail detail,
        bool showNotification)
    {
        if (oldKingdom == newKingdom)
        {
            if (!clan.IsUnderMercenaryService)
            {
                CompleteContract(clan);
            }

            return;
        }

        CompleteContract(clan);

        if (newKingdom is not null && clan.IsUnderMercenaryService)
        {
            StartContract(
                clan,
                newKingdom,
                clan.MercenaryAwardMultiplier,
                clan.ShouldStayInKingdomUntil,
                out _);
        }
    }
}
