using MercenaryManagement.Behaviors;
using MercenaryManagement.Contracts;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;

namespace MercenaryManagement.Services;

internal static class MercenaryPriorityService
{
    private const float JoinArmiesMaximumFollowingWeight = 25f;
    private const float FriendlyTerritoryRadiusMultiplier = 1.25f;
    private const float DeepRaidDistanceMultiplier = 2f;

    public static bool TrySetStrategicPriority(
        Clan clan,
        Kingdom kingdom,
        MercenaryStrategicPriority strategicPriority)
    {
        if (!MercenaryHiringService.IsPlayerRuler(kingdom)
            || !clan.IsUnderMercenaryService
            || clan.Kingdom != kingdom
            || !IsDefined(strategicPriority))
        {
            return false;
        }

        MercenaryContractBehavior? contractBehavior = Campaign.Current?
            .GetCampaignBehavior<MercenaryContractBehavior>();
        return contractBehavior?.SetStrategicPriority(clan, strategicPriority) == true;
    }

    public static bool TryGetStrategicPriority(
        Clan? clan,
        out MercenaryStrategicPriority priority)
    {
        if (clan is null
            || !clan.IsUnderMercenaryService
            || clan.Kingdom is null)
        {
            priority = MercenaryStrategicPriority.IndependentOperations;
            return false;
        }

        MercenaryContractBehavior? contractBehavior = Campaign.Current?
            .GetCampaignBehavior<MercenaryContractBehavior>();
        if (contractBehavior?.TryGetContract(clan, out MercenaryContract contract) != true)
        {
            priority = MercenaryStrategicPriority.IndependentOperations;
            return false;
        }

        priority = contract.StrategicPriority;
        return true;
    }

    public static float GetArmyCandidateWeight(
        MobileParty mobileParty,
        float nativeWeight)
    {
        if (!TryGetStrategicPriority(mobileParty, out MercenaryStrategicPriority priority))
        {
            return nativeWeight;
        }

        float finalWeight = GetDecisionWeight(
            priority,
            MercenaryAiDecision.ArmyCandidateRank,
            nativeWeight);
        if (priority == MercenaryStrategicPriority.EconomicWarfare
            || priority == MercenaryStrategicPriority.TerritorialDefense)
        {
            MercenaryPriorityBehavior? priorityBehavior = Campaign.Current?
                .GetCampaignBehavior<MercenaryPriorityBehavior>();
            finalWeight *= priorityBehavior?
                .GetArmyCandidateRankMultiplier(mobileParty) ?? 1f;
        }
        if (MercenaryAiDiagnostics.IsEnabled)
        {
            MercenaryAiDiagnostics.Record(
                mobileParty.ActualClan,
                priority,
                MercenaryAiDecision.ArmyCandidateRank.ToString(),
                nativeWeight,
                finalWeight);
        }
        return finalWeight;
    }

    public static float GetMaximumDistanceToCallToArmy(
        ArmyManagementCalculationModel armyManagementModel,
        MobileParty mobileParty)
    {
        float nativeMaximumDistance = armyManagementModel.MaximumDistanceToCallToArmy;
        if (!TryGetStrategicPriority(mobileParty, out MercenaryStrategicPriority priority))
        {
            return nativeMaximumDistance;
        }

        float finalDistance = GetDecisionWeight(
            priority,
            MercenaryAiDecision.ArmyCallDistance,
            nativeMaximumDistance);
        if (MercenaryAiDiagnostics.IsEnabled)
        {
            MercenaryAiDiagnostics.Record(
                mobileParty.ActualClan,
                priority,
                MercenaryAiDecision.ArmyCallDistance.ToString(),
                nativeMaximumDistance,
                finalDistance);
        }
        return finalDistance;
    }

    public static void AdjustBehaviorScores(
        MobileParty mobileParty,
        PartyThinkParams p,
        int firstScoreIndex,
        MercenaryAiBehaviorSource behaviorSource)
    {
        if (!TryGetStrategicPriority(
                mobileParty,
                out MercenaryStrategicPriority priority)
            || priority == MercenaryStrategicPriority.IndependentOperations)
        {
            return;
        }

        int scoreCount = p.AIBehaviorScores.Count;
        for (int i = firstScoreIndex; i < scoreCount; i++)
        {
            (AIBehaviorData behaviorData, float nativeWeight) = p.AIBehaviorScores[i];
            float adjustedWeight = GetBehaviorWeight(
                mobileParty,
                priority,
                behaviorSource,
                in behaviorData,
                nativeWeight);
            if (MercenaryAiDiagnostics.IsEnabled)
            {
                MercenaryAiDiagnostics.Record(
                    mobileParty.ActualClan,
                    priority,
                    behaviorSource + ":" + behaviorData.AiBehavior,
                    nativeWeight,
                    adjustedWeight);
            }
            if (adjustedWeight == nativeWeight)
            {
                continue;
            }

            p.SetBehaviorScore(in behaviorData, adjustedWeight);
        }
    }

    private static float GetBehaviorWeight(
        MobileParty mobileParty,
        MercenaryStrategicPriority priority,
        MercenaryAiBehaviorSource behaviorSource,
        in AIBehaviorData behaviorData,
        float nativeWeight)
    {
        float adjustedWeight = nativeWeight;
        bool isFollowingCurrentArmy = IsFollowingCurrentArmy(
            mobileParty,
            in behaviorData);

        if (isFollowingCurrentArmy)
        {
            adjustedWeight = GetDecisionWeight(
                priority,
                MercenaryAiDecision.FollowArmy,
                adjustedWeight);
        }

        switch (behaviorData.AiBehavior)
        {
            case AiBehavior.RaidSettlement:
                if (behaviorData.Party is Settlement raidTarget && raidTarget.IsVillage)
                {
                    adjustedWeight = GetDecisionWeight(
                        priority,
                        MercenaryAiDecision.RaidVillage,
                        adjustedWeight);

                    if (priority == MercenaryStrategicPriority.TerritorialDefense
                        && IsDeepEnemyRaid(mobileParty, raidTarget))
                    {
                        adjustedWeight = GetDecisionWeight(
                            priority,
                            MercenaryAiDecision.DeepEnemyRaid,
                            adjustedWeight);
                    }
                }

                break;
            case AiBehavior.EngageParty
                when behaviorSource == MercenaryAiBehaviorSource.EngageParty:
                if (behaviorData.Party is MobileParty engageTarget)
                {
                    adjustedWeight = GetEngagePartyWeight(
                        mobileParty,
                        priority,
                        engageTarget,
                        adjustedWeight);
                }

                break;
            case AiBehavior.DefendSettlement:
                adjustedWeight = GetDecisionWeight(
                    priority,
                    MercenaryAiDecision.PassiveDefense,
                    adjustedWeight);
                if (behaviorData.Party is Settlement defendedSettlement)
                {
                    adjustedWeight = GetSettlementDefenseWeight(
                        mobileParty,
                        priority,
                        defendedSettlement,
                        adjustedWeight);
                }

                break;
            case AiBehavior.PatrolAroundPoint:
                adjustedWeight = GetDecisionWeight(
                    priority,
                    MercenaryAiDecision.PassiveDefense,
                    adjustedWeight);
                if (priority == MercenaryStrategicPriority.TerritorialDefense
                    && IsInsideFriendlyTerritory(
                        mobileParty,
                        behaviorData.Position))
                {
                    adjustedWeight = GetDecisionWeight(
                        priority,
                        MercenaryAiDecision.FriendlyTerritoryAction,
                        adjustedWeight);
                }

                break;
        }

        if (isFollowingCurrentArmy
            && priority == MercenaryStrategicPriority.JoinArmies)
        {
            adjustedWeight = System.Math.Max(
                nativeWeight,
                System.Math.Min(
                    adjustedWeight,
                    JoinArmiesMaximumFollowingWeight));
        }

        return adjustedWeight;
    }

    private static float GetEngagePartyWeight(
        MobileParty mobileParty,
        MercenaryStrategicPriority priority,
        MobileParty target,
        float nativeWeight)
    {
        float adjustedWeight = nativeWeight;
        bool isAtWar = FactionManager.IsAtWarAgainstFaction(
            mobileParty.MapFaction,
            target.MapFaction);
        if (target.IsCaravan && isAtWar)
        {
            adjustedWeight = GetDecisionWeight(
                priority,
                MercenaryAiDecision.AttackCaravan,
                adjustedWeight);
        }
        else if (target.IsVillager && isAtWar)
        {
            adjustedWeight = GetDecisionWeight(
                priority,
                MercenaryAiDecision.AttackVillagers,
                adjustedWeight);
        }
        else if (target.IsBandit)
        {
            adjustedWeight = GetDecisionWeight(
                priority,
                MercenaryAiDecision.PursueBandits,
                adjustedWeight);
        }
        else if (isAtWar
                 && target.Party.EstimatedStrength
                 <= mobileParty.Party.EstimatedStrength * 0.75f)
        {
            adjustedWeight = GetDecisionWeight(
                priority,
                MercenaryAiDecision.PursueSmallEnemyParty,
                adjustedWeight);
        }

        if (priority == MercenaryStrategicPriority.TerritorialDefense
            && IsThreateningFriendlyCaravan(mobileParty, target))
        {
            adjustedWeight = GetDecisionWeight(
                priority,
                MercenaryAiDecision.DefendCaravan,
                adjustedWeight);
        }

        if (priority == MercenaryStrategicPriority.TerritorialDefense
            && IsInsideFriendlyTerritory(mobileParty, target.Position))
        {
            adjustedWeight = System.Math.Max(
                adjustedWeight,
                GetDecisionWeight(
                    priority,
                    MercenaryAiDecision.FriendlyTerritoryAction,
                    nativeWeight));
        }

        return adjustedWeight;
    }

    private static float GetSettlementDefenseWeight(
        MobileParty mobileParty,
        MercenaryStrategicPriority priority,
        Settlement settlement,
        float nativeWeight)
    {
        MercenaryAiDecision decision = settlement.IsVillage
            ? MercenaryAiDecision.DefendVillage
            : MercenaryAiDecision.DefendSettlement;
        float adjustedWeight = GetDecisionWeight(priority, decision, nativeWeight);
        if (priority == MercenaryStrategicPriority.TerritorialDefense
            && settlement.MapFaction == mobileParty.MapFaction)
        {
            adjustedWeight = System.Math.Max(
                adjustedWeight,
                GetDecisionWeight(
                    priority,
                    MercenaryAiDecision.FriendlyTerritoryAction,
                    nativeWeight));
        }

        return adjustedWeight;
    }

    private static float GetDecisionWeight(
        MercenaryStrategicPriority priority,
        MercenaryAiDecision decision,
        float nativeWeight)
    {
        return nativeWeight * GetDecisionMultiplier(priority, decision);
    }

    private static float GetDecisionMultiplier(
        MercenaryStrategicPriority priority,
        MercenaryAiDecision decision)
    {
        switch (priority)
        {
            case MercenaryStrategicPriority.JoinArmies:
                switch (decision)
                {
                    // Bannerlord sorts this rank in ascending order.
                    case MercenaryAiDecision.ArmyCandidateRank:
                        return 0.7f;
                    case MercenaryAiDecision.ArmyCallDistance:
                        return 1.5f;
                    case MercenaryAiDecision.FollowArmy:
                        return 1.25f;
                    default:
                        return 1f;
                }
            case MercenaryStrategicPriority.EconomicWarfare:
                switch (decision)
                {
                    case MercenaryAiDecision.RaidVillage:
                        return 1.5f;
                    case MercenaryAiDecision.AttackCaravan:
                        return 1.4f;
                    case MercenaryAiDecision.AttackVillagers:
                        return 1.35f;
                    case MercenaryAiDecision.PassiveDefense:
                    case MercenaryAiDecision.FollowArmy:
                        return 0.75f;
                    default:
                        return 1f;
                }
            case MercenaryStrategicPriority.TerritorialDefense:
                switch (decision)
                {
                    case MercenaryAiDecision.FollowArmy:
                        return 0.85f;
                    case MercenaryAiDecision.FriendlyTerritoryAction:
                        return 1.2f;
                    case MercenaryAiDecision.DefendVillage:
                        return 1.5f;
                    case MercenaryAiDecision.DefendCaravan:
                        return 1.4f;
                    case MercenaryAiDecision.DefendSettlement:
                        return 1.45f;
                    case MercenaryAiDecision.PursueBandits:
                        return 1.5f;
                    case MercenaryAiDecision.PursueSmallEnemyParty:
                        return 1.3f;
                    case MercenaryAiDecision.DeepEnemyRaid:
                        return 0.65f;
                    default:
                        return 1f;
                }
            default:
                return 1f;
        }
    }

    private static bool IsFollowingCurrentArmy(
        MobileParty mobileParty,
        in AIBehaviorData behaviorData)
    {
        return behaviorData.AiBehavior == AiBehavior.EscortParty
               && mobileParty.Army is not null
               && mobileParty.Army.LeaderParty != mobileParty
               && behaviorData.Party == mobileParty.Army.LeaderParty;
    }

    private static bool IsDeepEnemyRaid(
        MobileParty mobileParty,
        Settlement raidTarget)
    {
        if (raidTarget.MapFaction == mobileParty.MapFaction)
        {
            return false;
        }

        float maximumDistance = GetAverageTownDistance(mobileParty)
                                * DeepRaidDistanceMultiplier;
        return maximumDistance > 0f
               && TryGetDistanceToClosestFriendlySettlementSquared(
                   mobileParty,
                   raidTarget.Position,
                   out float distanceSquared)
               && distanceSquared > maximumDistance * maximumDistance;
    }

    private static bool IsInsideFriendlyTerritory(
        MobileParty mobileParty,
        CampaignVec2 position)
    {
        float maximumDistance = GetAverageTownDistance(mobileParty)
                                * FriendlyTerritoryRadiusMultiplier;
        return maximumDistance > 0f
               && TryGetDistanceToClosestFriendlySettlementSquared(
                   mobileParty,
                   position,
                   out float distanceSquared)
               && distanceSquared <= maximumDistance * maximumDistance;
    }

    private static bool IsThreateningFriendlyCaravan(
        MobileParty mobileParty,
        MobileParty target)
    {
        MobileParty? threatenedParty = target.ShortTermTargetParty
                                       ?? target.TargetParty;
        return threatenedParty is not null
               && FactionManager.IsAtWarAgainstFaction(
                   mobileParty.MapFaction,
                   target.MapFaction)
               && threatenedParty.IsCaravan
               && threatenedParty.MapFaction == mobileParty.MapFaction;
    }

    private static bool TryGetDistanceToClosestFriendlySettlementSquared(
        MobileParty mobileParty,
        CampaignVec2 position,
        out float distanceSquared)
    {
        Kingdom? kingdom = mobileParty.ActualClan?.Kingdom;
        distanceSquared = float.MaxValue;
        if (kingdom is null || kingdom.Settlements.Count == 0)
        {
            return false;
        }

        foreach (Settlement settlement in kingdom.Settlements)
        {
            float currentDistanceSquared = position.DistanceSquared(
                settlement.Position);
            if (currentDistanceSquared < distanceSquared)
            {
                distanceSquared = currentDistanceSquared;
            }
        }

        return distanceSquared < float.MaxValue;
    }

    private static float GetAverageTownDistance(MobileParty mobileParty)
    {
        return Campaign.Current
            .GetAverageDistanceBetweenClosestTwoTownsWithNavigationType(
                mobileParty.NavigationCapability);
    }

    private static bool TryGetStrategicPriority(
        MobileParty mobileParty,
        out MercenaryStrategicPriority priority)
    {
        return TryGetStrategicPriority(mobileParty.ActualClan, out priority);
    }

    private static bool IsDefined(MercenaryStrategicPriority strategicPriority)
    {
        switch (strategicPriority)
        {
            case MercenaryStrategicPriority.IndependentOperations:
            case MercenaryStrategicPriority.JoinArmies:
            case MercenaryStrategicPriority.EconomicWarfare:
            case MercenaryStrategicPriority.TerritorialDefense:
                return true;
            default:
                return false;
        }
    }
}
