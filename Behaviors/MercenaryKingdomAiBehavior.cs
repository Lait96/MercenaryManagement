using System.Linq;
using MercenaryManagement.Contracts;
using MercenaryManagement.Services;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;

namespace MercenaryManagement.Behaviors;

internal sealed class MercenaryKingdomAiBehavior : CampaignBehaviorBase
{
    private const float MinimumPriorityDurationDays = 3f;

    public override void RegisterEvents()
    {
        CampaignEvents.DailyTickEvent.AddNonSerializedListener(this, OnDailyTick);
    }

    public override void SyncData(IDataStore dataStore)
    {
    }

    private static void OnDailyTick()
    {
        foreach (Kingdom kingdom in Kingdom.All)
        {
            if (kingdom.IsEliminated || kingdom == Clan.PlayerClan.Kingdom && MercenaryHiringService.IsPlayerRuler(kingdom))
            {
                continue;
            }

            ManageExistingContracts(kingdom);
            TryHireForKingdom(kingdom);
        }
    }

    private static void ManageExistingContracts(Kingdom kingdom)
    {
        MercenaryContractBehavior? behavior = Campaign.Current?
            .GetCampaignBehavior<MercenaryContractBehavior>();
        if (behavior is null) return;

        foreach (Clan clan in kingdom.Clans.Where(clan => clan.IsUnderMercenaryService).ToArray())
        {
            if (!behavior.GetOrCreateContract(clan, out MercenaryContract contract)) continue;
            float need = MercenaryHiringService.CalculateMilitaryNeed(kingdom);
            if (clan != Clan.PlayerClan
                && contract.EndTime != CampaignTime.Zero
                && contract.EndTime.RemainingDaysFromNow <= 7f
                && contract.Satisfaction >= 40f
                && contract.Reliability >= 35f)
            {
                float renewalChance = 0.25f + need * 0.5f + contract.Satisfaction / 400f;
                if (MBRandom.RandomFloat < renewalChance)
                    behavior.SetContractEndTime(clan, CampaignTime.DaysFromNow(60f));
            }
            if (contract.Satisfaction < 15f
                || (need < 0.15f && contract.Satisfaction < 40f && MBRandom.RandomFloat < 0.08f))
            {
                MercenaryDismissalService.EndService(clan);
                continue;
            }

            if (contract.LastPriorityChangeTime == CampaignTime.Zero
                || contract.LastPriorityChangeTime.ElapsedDaysUntilNow >= MinimumPriorityDurationDays)
            {
                behavior.SetStrategicPriority(
                    clan,
                    MercenaryHiringService.GetSuggestedPriority(kingdom));
            }
            int targetRate = MercenaryHiringService.CalculateHirePrice(clan, kingdom);
            int currentRate = contract.InfluencePrice;
            int adjustedRate = currentRate < targetRate ? currentRate + 1 : currentRate > targetRate ? currentRate - 1 : currentRate;
            MercenaryPaymentService.TrySetInfluencePrice(clan, adjustedRate, out _);
        }
    }

    private static void TryHireForKingdom(Kingdom kingdom)
    {
        float need = MercenaryHiringService.CalculateMilitaryNeed(kingdom);
        if (need < 0.35f || MBRandom.RandomFloat > 0.12f * need) return;

        Clan? candidate = Clan.All
            .Where(clan => MercenaryHiringService.CanAiKingdomHire(clan, kingdom))
            .OrderByDescending(clan => clan.CurrentTotalStrength
                + (clan.Leader?.GetRelation(kingdom.Leader) ?? 0) * 5f)
            .FirstOrDefault();
        if (candidate is null) return;

        MercenaryHiringService.Hire(candidate, kingdom);
        Campaign.Current?.GetCampaignBehavior<MercenaryContractBehavior>()?
            .SetStrategicPriority(candidate, MercenaryHiringService.GetSuggestedPriority(kingdom));
    }
}
