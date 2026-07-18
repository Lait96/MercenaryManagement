using System;

namespace MercenaryManagement.Services;

internal readonly struct DailyEvaluationInputs
{
    public DailyEvaluationInputs(float satisfaction, float reliability,
        float employerSatisfaction, float rateRatio, int rulerRelation,
        float averagePartySizeRatio, int debtToKingdom, int contributionPoints)
    {
        Satisfaction = satisfaction;
        Reliability = reliability;
        EmployerSatisfaction = employerSatisfaction;
        RateRatio = rateRatio;
        RulerRelation = rulerRelation;
        AveragePartySizeRatio = averagePartySizeRatio;
        DebtToKingdom = debtToKingdom;
        ContributionPoints = contributionPoints;
    }

    public float Satisfaction { get; }
    public float Reliability { get; }
    public float EmployerSatisfaction { get; }
    public float RateRatio { get; }
    public int RulerRelation { get; }
    public float AveragePartySizeRatio { get; }
    public int DebtToKingdom { get; }
    public int ContributionPoints { get; }
}

internal readonly struct DailyEvaluationResult
{
    public DailyEvaluationResult(float satisfaction, float reliability,
        float employerSatisfaction)
    {
        Satisfaction = satisfaction;
        Reliability = reliability;
        EmployerSatisfaction = employerSatisfaction;
    }

    public float Satisfaction { get; }
    public float Reliability { get; }
    public float EmployerSatisfaction { get; }
}

internal static class MercenaryEvaluationModel
{
    public static DailyEvaluationResult Evaluate(DailyEvaluationInputs input)
    {
        float currentSatisfaction = Percentage(input.Satisfaction, 50f);
        float currentReliability = Percentage(input.Reliability, 50f);
        float currentEmployerSatisfaction = Percentage(input.EmployerSatisfaction, 50f);
        float rateRatio = Finite(input.RateRatio, 1f);
        float averagePartySizeRatio = Finite(input.AveragePartySizeRatio, 1f);
        int contributionPoints = Math.Max(0, input.ContributionPoints);

        float target = 50f + (rateRatio - 1f) * 60f;
        target += Math.Max(-10, Math.Min(10, input.RulerRelation)) * 0.5f;
        target -= Math.Max(0f, 0.65f - averagePartySizeRatio) * 30f;
        target -= Math.Min(15f, Math.Max(0, input.DebtToKingdom) / 200f);
        target += Math.Min(10f, contributionPoints / 20f);

        float satisfaction = Percentage(MoveTowards(currentSatisfaction, target, 3f), 50f);
        float reliabilityTarget = 50f + Math.Min(25f, contributionPoints / 10f);
        float reliability = Percentage(
            MoveTowards(currentReliability, reliabilityTarget, 0.5f), 50f);
        float employerTarget = 40f + Math.Min(35f, contributionPoints / 2f)
                               + (reliability - 50f) * 0.25f
                               - Math.Max(0f, rateRatio - 1f) * 20f;
        float employerSatisfaction = Percentage(MoveTowards(
            currentEmployerSatisfaction, employerTarget, 2f), 50f);
        return new DailyEvaluationResult(satisfaction, reliability, employerSatisfaction);
    }

    public static int GetPerformanceBand(int contributionPoints, float serviceDays)
    {
        if (contributionPoints >= 30) return 4;
        if (contributionPoints >= 15) return 3;
        if (contributionPoints >= 3) return 2;
        return serviceDays < 2f ? 1 : 0;
    }

    private static float MoveTowards(float value, float target, float maximumDelta)
    {
        if (value < target) return Math.Min(value + maximumDelta, target);
        return Math.Max(value - maximumDelta, target);
    }

    private static float Percentage(float value, float fallback)
    {
        return Math.Max(0f, Math.Min(100f, Finite(value, fallback)));
    }

    private static float Finite(float value, float fallback)
    {
        return float.IsNaN(value) || float.IsInfinity(value) ? fallback : value;
    }
}
