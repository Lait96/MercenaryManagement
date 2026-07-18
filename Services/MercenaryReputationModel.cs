using System;

namespace MercenaryManagement.Services;

internal readonly struct ContractReputationInputs
{
    public ContractReputationInputs(float reliability, float employerSatisfaction,
        int contributionPoints, int relation, float serviceDays)
    {
        Reliability = reliability;
        EmployerSatisfaction = employerSatisfaction;
        ContributionPoints = contributionPoints;
        Relation = relation;
        ServiceDays = serviceDays;
    }

    public float Reliability { get; }
    public float EmployerSatisfaction { get; }
    public int ContributionPoints { get; }
    public int Relation { get; }
    public float ServiceDays { get; }
}

internal static class MercenaryReputationModel
{
    public static float Calculate(ContractReputationInputs input)
    {
        float reliability = Bounded(input.Reliability, 0f, 100f, 50f);
        float satisfaction = Bounded(input.EmployerSatisfaction, 0f, 100f, 50f);
        float contribution = Math.Min(100f, Math.Max(0, input.ContributionPoints));
        float relation = Bounded(50f + input.Relation, 0f, 100f, 50f);
        float score = reliability * 0.4f + satisfaction * 0.3f
                      + contribution * 0.2f + relation * 0.1f;
        float weight = Bounded(input.ServiceDays / 30f, 0.25f, 1f, 0.25f);
        return 50f + (score - 50f) * weight;
    }

    public static float Combine(float generalReputation, float employerReputation,
        bool hasEmployerHistory)
    {
        float general = Bounded(generalReputation, 0f, 100f, 50f);
        if (!hasEmployerHistory) return general;
        float employer = Bounded(employerReputation, 0f, 100f, 50f);
        return general * 0.4f + employer * 0.6f;
    }

    private static float Bounded(float value, float minimum, float maximum, float fallback)
    {
        float finite = float.IsNaN(value) || float.IsInfinity(value) ? fallback : value;
        return Math.Max(minimum, Math.Min(maximum, finite));
    }
}
