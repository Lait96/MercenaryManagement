using System;

namespace MercenaryManagement.Services;

internal readonly struct DepartureRiskInputs
{
    public DepartureRiskInputs(float satisfaction, float reliability, float rateRatio,
        int rulerRelation, float averagePartySizeRatio, float militaryNeed,
        float priorityMismatchPenalty, float remainingDays, bool hasFiniteTerm)
    {
        Satisfaction = satisfaction;
        Reliability = reliability;
        RateRatio = rateRatio;
        RulerRelation = rulerRelation;
        AveragePartySizeRatio = averagePartySizeRatio;
        MilitaryNeed = militaryNeed;
        PriorityMismatchPenalty = priorityMismatchPenalty;
        RemainingDays = remainingDays;
        HasFiniteTerm = hasFiniteTerm;
    }

    public float Satisfaction { get; }
    public float Reliability { get; }
    public float RateRatio { get; }
    public int RulerRelation { get; }
    public float AveragePartySizeRatio { get; }
    public float MilitaryNeed { get; }
    public float PriorityMismatchPenalty { get; }
    public float RemainingDays { get; }
    public bool HasFiniteTerm { get; }
}

internal readonly struct EmployerTerminationRiskInputs
{
    public EmployerTerminationRiskInputs(float employerSatisfaction, float rateRatio,
        float militaryNeed, float reliability)
    {
        EmployerSatisfaction = employerSatisfaction;
        RateRatio = rateRatio;
        MilitaryNeed = militaryNeed;
        Reliability = reliability;
    }

    public float EmployerSatisfaction { get; }
    public float RateRatio { get; }
    public float MilitaryNeed { get; }
    public float Reliability { get; }
}

internal sealed class RiskBreakdown
{
    public float Base { get; set; }
    public float Satisfaction { get; set; }
    public float Reliability { get; set; }
    public float Rate { get; set; }
    public float Relation { get; set; }
    public float Losses { get; set; }
    public float MilitaryNeed { get; set; }
    public float PriorityMismatch { get; set; }
    public float RemainingTerm { get; set; }
    public float Total { get; set; }
}

internal static class MercenaryRiskModel
{
    public static RiskBreakdown CalculateDepartureRisk(DepartureRiskInputs input)
    {
        var result = new RiskBreakdown
        {
            Base = 50f,
            Satisfaction = (50f - input.Satisfaction) * 0.6f,
            Reliability = (50f - input.Reliability) * 0.2f,
            Rate = input.RateRatio < 1f
                ? Math.Min(12f, (1f - input.RateRatio) * 40f)
                : -Math.Min(8f, (input.RateRatio - 1f) * 16f),
            Relation = -Math.Max(-20, Math.Min(20, input.RulerRelation)) * 0.35f,
            Losses = Math.Max(0f, 0.65f - input.AveragePartySizeRatio) * 30f,
            MilitaryNeed = input.MilitaryNeed * 8f,
            PriorityMismatch = input.PriorityMismatchPenalty,
            RemainingTerm = input.HasFiniteTerm
                ? Math.Max(0f, 10f - Math.Max(0f, input.RemainingDays))
                : 0f
        };
        result.Total = Clamp(result.Base + result.Satisfaction + result.Reliability
            + result.Rate + result.Relation + result.Losses + result.MilitaryNeed
            + result.PriorityMismatch + result.RemainingTerm);
        return result;
    }

    public static RiskBreakdown CalculateEmployerTerminationRisk(
        EmployerTerminationRiskInputs input)
    {
        var result = new RiskBreakdown
        {
            Base = 50f,
            Satisfaction = (50f - input.EmployerSatisfaction) * 0.7f,
            Rate = Math.Max(0f, input.RateRatio - 1f) * 20f,
            MilitaryNeed = (0.5f - input.MilitaryNeed) * 16f,
            Reliability = (50f - input.Reliability) * 0.1f
        };
        result.Total = Clamp(result.Base + result.Satisfaction + result.Rate
            + result.MilitaryNeed + result.Reliability);
        return result;
    }

    private static float Clamp(float value) => Math.Max(0f, Math.Min(100f, value));
}
