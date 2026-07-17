using System.Collections.Generic;
using MercenaryManagement.Contracts;
using MercenaryManagement.Services;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.CampaignSystem.MapEvents;

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
        CampaignEvents.OnPartyJoinedArmyEvent.AddNonSerializedListener(this, OnPartyJoinedArmy);
        CampaignEvents.RaidCompletedEvent.AddNonSerializedListener(this, OnRaidCompleted);
        CampaignEvents.MapEventEnded.AddNonSerializedListener(this, OnMapEventEnded);
        CampaignEvents.OnHideoutBattleCompletedEvent.AddNonSerializedListener(
            this,
            OnHideoutBattleCompleted);
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
        RecordDailyContribution(mobileParty);
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

    private static void RecordDailyContribution(MobileParty mobileParty)
    {
        Clan? clan = mobileParty.ActualClan;
        if (!mobileParty.IsActive || clan is null || clan.Kingdom is null)
        {
            return;
        }

        MercenaryContractBehavior? behavior = Campaign.Current?
            .GetCampaignBehavior<MercenaryContractBehavior>();
        if (behavior?.TryGetContract(clan, out MercenaryContract contract) != true)
        {
            return;
        }

        bool matches = contract.StrategicPriority == MercenaryStrategicPriority.JoinArmies
            ? mobileParty.Army is not null
            : contract.StrategicPriority == MercenaryStrategicPriority.EconomicWarfare
                ? mobileParty.DefaultBehavior == AiBehavior.RaidSettlement
                : contract.StrategicPriority == MercenaryStrategicPriority.IndependentOperations
                  || contract.StrategicPriority == MercenaryStrategicPriority.TerritorialDefense
                     && MercenaryPriorityService.IsInFriendlyTerritory(mobileParty);
        if (matches)
        {
            contract.RecordContribution(1, GetDailyActivity(contract.StrategicPriority));
        }
    }

    private static void OnPartyJoinedArmy(MobileParty party)
    {
        RecordContribution(party, MercenaryStrategicPriority.JoinArmies, 5, "JoinedArmy");
    }

    private static void OnRaidCompleted(BattleSideEnum winnerSide, RaidEventComponent raid)
    {
        if (winnerSide != BattleSideEnum.Attacker) return;
        MobileParty? party = raid.AttackerSide.LeaderParty?.MobileParty;
        if (party is not null)
            RecordContribution(party, MercenaryStrategicPriority.EconomicWarfare, 15, "CompletedRaid");
    }

    private static void OnMapEventEnded(MapEvent mapEvent)
    {
        if (!mapEvent.HasWinner
            || mapEvent.IsHideoutBattle
            || mapEvent.Winner.OtherSide.MapFaction?.IsBanditFaction != true)
        {
            return;
        }

        RecordTerritorialDefenseContribution(
            mapEvent,
            5,
            "ClearedBandits");
    }

    private static void OnHideoutBattleCompleted(
        BattleSideEnum winnerSide,
        HideoutEventComponent hideout,
        HideoutEventComponent.HideoutBattleEndState endState)
    {
        if (winnerSide != BattleSideEnum.Attacker) return;
        RecordTerritorialDefenseContribution(
            hideout.MapEvent,
            15,
            "ClearedHideout");
    }

    private static void RecordTerritorialDefenseContribution(
        MapEvent mapEvent,
        int points,
        string activity)
    {
        if (!mapEvent.HasWinner) return;
        var creditedClans = new HashSet<string>();
        foreach (MapEventParty eventParty in mapEvent.Winner.Parties)
        {
            Clan? clan = eventParty.Party.MobileParty?.ActualClan;
            if (clan is null || !creditedClans.Add(clan.StringId)) continue;
            MercenaryContractBehavior? behavior = Campaign.Current?
                .GetCampaignBehavior<MercenaryContractBehavior>();
            if (behavior?.TryGetContract(clan, out MercenaryContract contract) == true
                && contract.StrategicPriority == MercenaryStrategicPriority.TerritorialDefense)
            {
                contract.RecordContribution(points, activity);
            }
        }
    }

    private static void RecordContribution(MobileParty party,
        MercenaryStrategicPriority expectedPriority, int points, string activity)
    {
        MercenaryContractBehavior? behavior = Campaign.Current?
            .GetCampaignBehavior<MercenaryContractBehavior>();
        if (party.ActualClan is not null
            && behavior?.TryGetContract(party.ActualClan, out MercenaryContract contract) == true
            && contract.StrategicPriority == expectedPriority)
        {
            contract.RecordContribution(points, activity);
        }
    }

    private static string GetDailyActivity(MercenaryStrategicPriority priority)
    {
        switch (priority)
        {
            case MercenaryStrategicPriority.JoinArmies: return "ServingInArmy";
            case MercenaryStrategicPriority.EconomicWarfare: return "EconomicOperation";
            case MercenaryStrategicPriority.TerritorialDefense: return "DefendingTerritory";
            default: return "IndependentOperation";
        }
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
