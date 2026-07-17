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
        CampaignEvents.ArmyCreated.AddNonSerializedListener(this, OnArmyCreated);
        CampaignEvents.ArmyDispersed.AddNonSerializedListener(this, OnArmyDispersed);
    }

    public override void SyncData(IDataStore dataStore)
    {
    }

    private static void OnDailyTick()
    {
        foreach (Kingdom kingdom in Kingdom.All)
        {
            if (!CanManageKingdom(kingdom))
            {
                continue;
            }

            ManageExistingContracts(kingdom);
            TryHireForKingdom(kingdom);
        }
    }

    private static void OnArmyCreated(Army army)
    {
        Kingdom kingdom = army.Kingdom;
        if (!CanManageKingdom(kingdom)) return;
        MercenaryStrategicPriority desiredPriority =
            MercenaryHiringService.GetSuggestedPriority(kingdom, true);
        if (desiredPriority == MercenaryStrategicPriority.JoinArmies)
            SetPriorityForMercenaries(kingdom, desiredPriority);
    }

    private static void OnArmyDispersed(
        Army army,
        Army.ArmyDispersionReason reason,
        bool isPlayersArmy)
    {
        Kingdom kingdom = army.Kingdom;
        if (!CanManageKingdom(kingdom)
            || kingdom.Armies.Any(candidate => candidate != army))
        {
            return;
        }

        SetPriorityForMercenaries(
            kingdom,
            MercenaryHiringService.GetSuggestedPriority(kingdom, false),
            MercenaryStrategicPriority.JoinArmies);
    }

    private static bool CanManageKingdom(Kingdom kingdom)
    {
        return !kingdom.IsEliminated
               && (kingdom != Clan.PlayerClan.Kingdom
                   || !MercenaryHiringService.IsPlayerRuler(kingdom));
    }

    private static void SetPriorityForMercenaries(
        Kingdom kingdom,
        MercenaryStrategicPriority priority,
        MercenaryStrategicPriority? requiredCurrentPriority = null)
    {
        MercenaryContractBehavior? behavior = Campaign.Current?
            .GetCampaignBehavior<MercenaryContractBehavior>();
        if (behavior is null) return;
        foreach (Clan clan in kingdom.Clans.Where(clan => clan.IsUnderMercenaryService))
        {
            if (behavior.GetOrCreateContract(clan, out MercenaryContract contract)
                && (requiredCurrentPriority is null
                    || contract.StrategicPriority == requiredCurrentPriority.Value))
            {
                behavior.SetStrategicPriority(clan, priority);
            }
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

            bool hasActiveArmy = kingdom.Armies.Count > 0;
            MercenaryStrategicPriority desiredPriority =
                MercenaryHiringService.GetSuggestedPriority(kingdom, true);
            MercenaryStrategicPriority availablePriority =
                MercenaryHiringService.GetSuggestedPriority(kingdom, hasActiveArmy);
            bool armyAvailabilityException =
                (contract.StrategicPriority == MercenaryStrategicPriority.JoinArmies
                 && !hasActiveArmy)
                || (desiredPriority == MercenaryStrategicPriority.JoinArmies
                    && hasActiveArmy
                    && contract.StrategicPriority != MercenaryStrategicPriority.JoinArmies);
            if (armyAvailabilityException
                || contract.LastPriorityChangeTime == CampaignTime.Zero
                || contract.LastPriorityChangeTime.ElapsedDaysUntilNow >= MinimumPriorityDurationDays)
            {
                behavior.SetStrategicPriority(
                    clan,
                    availablePriority);
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
