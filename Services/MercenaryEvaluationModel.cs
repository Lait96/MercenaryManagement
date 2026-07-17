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
        float target = 50f + (input.RateRatio - 1f) * 60f;
        target += Math.Max(-10, Math.Min(10, input.RulerRelation)) * 0.5f;
        target -= Math.Max(0f, 0.65f - input.AveragePartySizeRatio) * 30f;
        target -= Math.Min(15f, Math.Max(0, input.DebtToKingdom) / 200f);
        target += Math.Min(10f, input.ContributionPoints / 20f);

        float satisfaction = MoveTowards(input.Satisfaction, target, 3f);
        float reliabilityTarget = 50f + Math.Min(25f, input.ContributionPoints / 10f);
        float reliability = MoveTowards(input.Reliability, reliabilityTarget, 0.5f);
        float employerTarget = 40f + Math.Min(35f, input.ContributionPoints / 2f)
                               + (reliability - 50f) * 0.25f
                               - Math.Max(0f, input.RateRatio - 1f) * 20f;
        float employerSatisfaction = MoveTowards(
            input.EmployerSatisfaction, employerTarget, 2f);
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
}
