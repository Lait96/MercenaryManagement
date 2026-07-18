using System;
using System.Collections.Generic;
using MercenaryManagement.Contracts;

namespace MercenaryManagement.Services;

internal enum RecoveryDecision
{
    VisitSettlement,
    RecruitTroops,
    TrainTroops,
    JoinArmy,
    OffensiveCombat,
    DefendSelf
}

internal static class MercenaryRecoveryPolicy
{
    public const float DefaultDurationDays = 14f;
    public const float DefaultTargetPartySizeRatio = 0.9f;
    public const float MinimumDurationDays = 1f;
    public const float MaximumDurationDays = 30f;
    public const float MinimumTargetPartySizeRatio = 0.5f;

    public static float NormalizeDurationDays(float value)
    {
        if (!IsFinite(value)) return DefaultDurationDays;
        return Math.Max(MinimumDurationDays, Math.Min(MaximumDurationDays, value));
    }

    public static float NormalizeTargetPartySizeRatio(float value)
    {
        if (!IsFinite(value)) return DefaultTargetPartySizeRatio;
        return Math.Max(MinimumTargetPartySizeRatio, Math.Min(1f, value));
    }

    public static bool IsGoalComplete(
        IReadOnlyList<float> activePartySizeRatios,
        float targetPartySizeRatio)
    {
        if (activePartySizeRatios is null || activePartySizeRatios.Count == 0)
            return false;

        float target = NormalizeTargetPartySizeRatio(targetPartySizeRatio);
        for (int index = 0; index < activePartySizeRatios.Count; index++)
        {
            float ratio = activePartySizeRatios[index];
            if (!IsFinite(ratio) || ratio < target)
                return false;
        }
        return true;
    }

    public static bool ShouldComplete(
        double currentDay,
        double endDay,
        IReadOnlyList<float> activePartySizeRatios,
        float targetPartySizeRatio)
    {
        bool validTimes = !double.IsNaN(currentDay) && !double.IsInfinity(currentDay)
                          && !double.IsNaN(endDay) && !double.IsInfinity(endDay);
        return (validTimes && currentDay >= endDay)
               || IsGoalComplete(activePartySizeRatios, targetPartySizeRatio);
    }

    public static MercenaryStrategicPriority GetRestoredPriority(
        MercenaryStrategicPriority previousPriority)
    {
        switch (previousPriority)
        {
            case MercenaryStrategicPriority.JoinArmies:
            case MercenaryStrategicPriority.EconomicWarfare:
            case MercenaryStrategicPriority.TerritorialDefense:
            case MercenaryStrategicPriority.IndependentOperations:
                return previousPriority;
            default:
                return MercenaryStrategicPriority.IndependentOperations;
        }
    }

    public static float GetMultiplier(RecoveryDecision decision)
    {
        switch (decision)
        {
            case RecoveryDecision.VisitSettlement: return 2.25f;
            case RecoveryDecision.RecruitTroops: return 2f;
            case RecoveryDecision.TrainTroops: return 1.75f;
            case RecoveryDecision.JoinArmy: return 0.35f;
            case RecoveryDecision.OffensiveCombat: return 0.45f;
            case RecoveryDecision.DefendSelf: return 1f;
            default: return 1f;
        }
    }

    private static bool IsFinite(float value)
    {
        return !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
