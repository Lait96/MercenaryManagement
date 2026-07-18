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

    private static void OnArmyCreated(Army? army)
    {
        ArmyLifecycleDecision decision = EvaluateArmyLifecycle(
            army,
            countOtherArmies: false);
        Kingdom? kingdom = army?.Kingdom;
        if (decision != ArmyLifecycleDecision.ProcessCreatedArmy
            || kingdom is null)
        {
            return;
        }

        MercenaryStrategicPriority desiredPriority =
            MercenaryHiringService.GetSuggestedPriority(kingdom, true);
        if (desiredPriority == MercenaryStrategicPriority.JoinArmies)
            SetPriorityForMercenaries(kingdom, desiredPriority);
    }

    private static void OnArmyDispersed(
        Army? army,
        Army.ArmyDispersionReason reason,
        bool isPlayersArmy)
    {
        // Bannerlord may clear Army.Kingdom before dispatching ArmyDispersed
        // when the leader party is destroyed. There is no safe kingdom context
        // left in that state, so the priority update must be skipped.
        ArmyLifecycleDecision decision = EvaluateArmyLifecycle(
            army,
            countOtherArmies: true);
        Kingdom? kingdom = army?.Kingdom;
        if (decision != ArmyLifecycleDecision.ReevaluateAfterLastArmyDispersed
            || kingdom is null)
        {
            return;
        }

        SetPriorityForMercenaries(
            kingdom,
            MercenaryHiringService.GetSuggestedPriority(kingdom, false),
            MercenaryStrategicPriority.JoinArmies);
    }

    private static ArmyLifecycleDecision EvaluateArmyLifecycle(
        Army? army,
        bool countOtherArmies)
    {
        Kingdom? kingdom = army?.Kingdom;
        Clan? playerClan = Clan.PlayerClan;
        int otherActiveArmyCount = countOtherArmies && kingdom is not null
            ? kingdom.Armies.Count(candidate => candidate != army)
            : 0;
        var snapshot = new ArmyLifecycleSnapshot(
            army is not null,
            kingdom is not null,
            kingdom?.IsEliminated == true,
            kingdom is not null
            && playerClan?.Kingdom == kingdom
            && MercenaryHiringService.IsPlayerRuler(kingdom),
            otherActiveArmyCount);
        return countOtherArmies
            ? MercenaryArmyLifecyclePolicy.OnArmyDispersed(snapshot)
            : MercenaryArmyLifecyclePolicy.OnArmyCreated(snapshot);
    }

    internal static ArmyLifecycleDecision RunDiagnosticArmyProbe(
        Army? army,
        bool dispersed)
    {
        return EvaluateArmyLifecycle(army, dispersed);
    }

    internal static void RunDiagnosticNullArmyCallbacks()
    {
        OnArmyCreated(null);
        OnArmyDispersed(null, default, false);
    }

    private static bool CanManageKingdom(Kingdom? kingdom)
    {
        Clan? playerClan = Clan.PlayerClan;
        return kingdom is not null
               && !kingdom.IsEliminated
               && (playerClan is null
                   || kingdom != playerClan.Kingdom
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
                && contract.StrategicPriority != MercenaryStrategicPriority.Recovery
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
            float departureRisk = MercenaryEvaluationService.CalculateDepartureRisk(contract);
            float terminationRisk = MercenaryEvaluationService.CalculateEmployerTerminationRisk(contract);
            if (clan != Clan.PlayerClan
                && contract.EndTime != CampaignTime.Zero
                && contract.EndTime.RemainingDaysFromNow <= 7f
                && contract.Satisfaction >= 40f
                && contract.Reliability >= 35f)
            {
                float renewalChance = 0.2f + need * 0.45f
                    + (100f - departureRisk) / 500f
                    + (100f - terminationRisk) / 500f;
                if (MBRandom.RandomFloat < System.Math.Min(0.95f, renewalChance))
                    behavior.SetContractEndTime(clan, CampaignTime.DaysFromNow(60f));
            }
            float dailyTerminationChance = terminationRisk > 70f
                ? (terminationRisk - 70f) / 500f
                : 0f;
            if (contract.EmployerSatisfaction < 10f
                || MBRandom.RandomFloat < dailyTerminationChance)
            {
                MercenaryDismissalService.EndService(clan,
                    MercenaryContractEndReason.EmployerDismissed);
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
            if (contract.StrategicPriority != MercenaryStrategicPriority.Recovery
                && (armyAvailabilityException
                || contract.LastPriorityChangeTime == CampaignTime.Zero
                || contract.LastPriorityChangeTime.ElapsedDaysUntilNow >= MinimumPriorityDurationDays))
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
