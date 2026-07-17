using System;
using MercenaryManagement.Contracts;
using MercenaryManagement.Services;

namespace MercenaryManagement.StaticTests;

internal static class Program
{
    private static int _passed;

    private static void Main()
    {
        TestContributionPolicy();
        TestEvaluationProgression();
        TestRiskBoundariesAndBusinessScenarios();
        TestOfferTerms();
        Console.WriteLine($"PASS: {_passed} deterministic business assertions.");
    }

    private static void TestContributionPolicy()
    {
        Assert(!MercenaryContributionPolicy.IsEligible(
            MercenaryStrategicPriority.IndependentOperations,
            PassiveContributionEvidence.ServingInArmy),
            "Independent Operations never receives passive contribution");
        Assert(MercenaryContributionPolicy.IsEligible(
            MercenaryStrategicPriority.JoinArmies,
            PassiveContributionEvidence.ServingInArmy),
            "matching army evidence is eligible");
        Assert(!MercenaryContributionPolicy.CanRecordDaily(0.99f),
            "second party cannot multiply the same clan-day contribution");
        Assert(MercenaryContributionPolicy.CanRecordDaily(1f),
            "next clan-day can record new evidence");
        int smallClan = SimulateParties(1);
        int largeClan = SimulateParties(8);
        Assert(smallClan == 1 && largeClan == 1,
            "party count creates no automatic contribution advantage");
    }

    private static void TestEvaluationProgression()
    {
        DailyEvaluationResult neutral = MercenaryEvaluationModel.Evaluate(
            new DailyEvaluationInputs(50, 50, 50, 1f, 0, 1f, 0, 0));
        Assert(neutral.Satisfaction == 50f && neutral.Reliability == 50f,
            "neutral contract remains neutral for mercenary evaluation");
        Assert(neutral.EmployerSatisfaction == 48f,
            "no contribution lowers employer satisfaction gradually");

        DailyEvaluationResult successful = MercenaryEvaluationModel.Evaluate(
            new DailyEvaluationInputs(50, 50, 50, 1.1f, 10, 1f, 0, 30));
        Assert(successful.Satisfaction > 50f && successful.Reliability > 50f
               && successful.EmployerSatisfaction > 50f,
            "confirmed contribution improves all relevant evaluations");
        Assert(MercenaryEvaluationModel.GetPerformanceBand(0, 1) == 1
               && MercenaryEvaluationModel.GetPerformanceBand(0, 3) == 0
               && MercenaryEvaluationModel.GetPerformanceBand(30, 3) == 4,
            "performance bands distinguish initial, limited, and excellent results");
    }

    private static void TestRiskBoundariesAndBusinessScenarios()
    {
        RiskBreakdown goodDeparture = MercenaryRiskModel.CalculateDepartureRisk(
            new DepartureRiskInputs(90, 90, 1.5f, 20, 1f, 0.2f, 0, 60, true));
        RiskBreakdown badDeparture = MercenaryRiskModel.CalculateDepartureRisk(
            new DepartureRiskInputs(5, 10, 0.85f, -20, 0.25f, 1f, 8, 2, true));
        Assert(goodDeparture.Total >= 0 && goodDeparture.Total <= 100
               && badDeparture.Total >= 0 && badDeparture.Total <= 100,
            "departure risk is clamped to percentages");
        Assert(badDeparture.Total > 70 && badDeparture.Total > goodDeparture.Total,
            "bad contract creates actionable departure risk");

        RiskBreakdown goodEmployer = MercenaryRiskModel.CalculateEmployerTerminationRisk(
            new EmployerTerminationRiskInputs(90, 0.9f, 1f, 90));
        RiskBreakdown badEmployer = MercenaryRiskModel.CalculateEmployerTerminationRisk(
            new EmployerTerminationRiskInputs(5, 1.5f, 0f, 10));
        Assert(badEmployer.Total > 70 && badEmployer.Total > goodEmployer.Total,
            "expensive ineffective contract creates actionable termination risk");
        RiskBreakdown sameInputs = MercenaryRiskModel.CalculateEmployerTerminationRisk(
            new EmployerTerminationRiskInputs(5, 1.5f, 0f, 10));
        Assert(sameInputs.Total == badEmployer.Total,
            "termination formula has no hidden or duplicated contribution input");
    }

    private static void TestOfferTerms()
    {
        OfferTermsResult lowNeed = MercenaryOfferModel.CalculateTerms(100, 85, 150, 0f, 50, true);
        OfferTermsResult highNeed = MercenaryOfferModel.CalculateTerms(100, 85, 150, 1f, 80, false);
        Assert(lowNeed.InfluencePrice == 85 && lowNeed.DurationDays == 30,
            "requested low-need offer respects minimum and short duration");
        Assert(highNeed.InfluencePrice > lowNeed.InfluencePrice && highNeed.DurationDays == 120,
            "military need and reliability improve calculated offer terms");
        Assert(highNeed.InfluencePrice <= 150,
            "offer terms respect maximum price");
    }

    private static int SimulateParties(int partyCount)
    {
        int contribution = 0;
        float elapsed = -1f;
        for (int i = 0; i < partyCount; i++)
        {
            if (MercenaryContributionPolicy.CanRecordDaily(elapsed))
            {
                contribution++;
                elapsed = 0f;
            }
        }
        return contribution;
    }

    private static void Assert(bool condition, string scenario)
    {
        if (!condition) throw new InvalidOperationException("FAIL: " + scenario);
        _passed++;
        Console.WriteLine("PASS: " + scenario);
    }
}
