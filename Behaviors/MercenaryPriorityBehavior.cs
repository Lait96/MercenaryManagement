using System.Collections.Generic;
using MercenaryManagement.Contracts;
using MercenaryManagement.Services;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;

namespace MercenaryManagement.Behaviors;

internal sealed class MercenaryPriorityBehavior : CampaignBehaviorBase
{
    private const float EconomicWarfareDailyLeaveChance = 0.15f;
    private const float TerritorialDefenseDailyLeaveChance = 0.12f;
    private const float MatchingArmyTypeChanceMultiplier = 0.5f;
    private const float ArmyRejoinCooldownDays = 3f;
    private const float MaximumArmyRankPenalty = 1.3f;
    private const float MinimumSafePartySizeRatio = 0.5f;
    private const int MinimumFoodDays = 3;

    private Dictionary<string, CampaignTime> _armyRejoinCooldowns =
        new Dictionary<string, CampaignTime>();

    public override void RegisterEvents()
    {
        CampaignEvents.DailyTickPartyEvent.AddNonSerializedListener(
            this,
            OnDailyTickParty);
    }

    public override void SyncData(IDataStore dataStore)
    {
        dataStore.SyncData(
            "MercenaryManagement_ArmyRejoinCooldowns",
            ref _armyRejoinCooldowns);
        _armyRejoinCooldowns ??= new Dictionary<string, CampaignTime>();
    }

    public float GetArmyCandidateRankMultiplier(MobileParty mobileParty)
    {
        if (!_armyRejoinCooldowns.TryGetValue(
                mobileParty.StringId,
                out CampaignTime cooldownEnd))
        {
            return 1f;
        }

        if (cooldownEnd.IsPast)
        {
            _armyRejoinCooldowns.Remove(mobileParty.StringId);
            return 1f;
        }

        float remainingDays = System.Math.Min(
            ArmyRejoinCooldownDays,
            cooldownEnd.RemainingDaysFromNow);
        return System.Math.Min(
            MaximumArmyRankPenalty,
            1f + remainingDays * 0.1f);
    }

    private void OnDailyTickParty(MobileParty mobileParty)
    {
        Army? army = mobileParty.Army;
        if (army is null
            || mobileParty.IsMainParty
            || !mobileParty.IsActive
            || mobileParty.IsDisbanding
            || army.LeaderParty == mobileParty
            || !MercenaryPriorityService.TryGetStrategicPriority(
                mobileParty.ActualClan,
                out MercenaryStrategicPriority priority))
        {
            return;
        }

        float leaveChance = GetDailyLeaveChance(priority, army.ArmyType);
        if (leaveChance <= 0f)
        {
            return;
        }

        RecordEvent(mobileParty, priority, "ArmyLeaveCheck");
        if (mobileParty.MapEvent is not null || army.LeaderParty.MapEvent is not null)
        {
            RecordEvent(mobileParty, priority, "ArmyLeaveBlockedBattle");
            return;
        }

        if (mobileParty.SiegeEvent is not null || army.LeaderParty.SiegeEvent is not null)
        {
            RecordEvent(mobileParty, priority, "ArmyLeaveBlockedSiege");
            return;
        }

        Hero? leader = mobileParty.LeaderHero;
        if (leader is null || !leader.IsAlive || leader.IsPrisoner)
        {
            RecordEvent(mobileParty, priority, "ArmyLeaveBlockedLeader");
            return;
        }

        if (mobileParty.PartySizeRatio < MinimumSafePartySizeRatio)
        {
            RecordEvent(mobileParty, priority, "ArmyLeaveBlockedStrength");
            return;
        }

        if (mobileParty.GetNumDaysForFoodToLast() < MinimumFoodDays)
        {
            RecordEvent(mobileParty, priority, "ArmyLeaveBlockedFood");
            return;
        }

        if (MBRandom.RandomFloat >= leaveChance)
        {
            RecordEvent(mobileParty, priority, "ArmyLeaveRollFailed");
            return;
        }

        // MobileParty.Army owns the native removal lifecycle, including detaching
        // the party, resetting its initiative, and dispatching army leave events.
        mobileParty.Army = null;
        _armyRejoinCooldowns[mobileParty.StringId] = CampaignTime.DaysFromNow(
            ArmyRejoinCooldownDays);
        RecordEvent(mobileParty, priority, "ArmyLeaveCompleted");
    }

    private static float GetDailyLeaveChance(
        MercenaryStrategicPriority priority,
        Army.ArmyTypes armyType)
    {
        switch (priority)
        {
            case MercenaryStrategicPriority.EconomicWarfare:
                return armyType == Army.ArmyTypes.Raider
                    ? EconomicWarfareDailyLeaveChance * MatchingArmyTypeChanceMultiplier
                    : EconomicWarfareDailyLeaveChance;
            case MercenaryStrategicPriority.TerritorialDefense:
                return (armyType == Army.ArmyTypes.Defender
                        || armyType == Army.ArmyTypes.Patrolling)
                    ? TerritorialDefenseDailyLeaveChance * MatchingArmyTypeChanceMultiplier
                    : TerritorialDefenseDailyLeaveChance;
            default:
                return 0f;
        }
    }

    private static void RecordEvent(
        MobileParty mobileParty,
        MercenaryStrategicPriority priority,
        string eventName)
    {
        if (MercenaryAiDiagnostics.IsEnabled)
        {
            MercenaryAiDiagnostics.RecordEvent(
                mobileParty.ActualClan,
                priority,
                eventName);
        }
    }
}
