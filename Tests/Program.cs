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
        TestArmyLifecycleScenarios();
        TestAiControlBoundaries();
        TestPlayerRecruitmentAdvantage();
        TestArmyPartyLifecycleSafety();
        TestUnexpectedContributionInputs();
        TestEvaluationInputRobustness();
        TestRiskInputRobustnessAndMonotonicity();
        TestOfferInputRobustness();
        TestPersistentValueRobustness();
        TestContractReputation();
        TestRecoveryPriority();
        TestHiringLimit();
        Console.WriteLine($"PASS: {_passed} deterministic business assertions.");
    }

    private static void TestHiringLimit()
    {
        Assert(MercenaryHiringLimitPolicy.MaximumHiredMercenaryClans == 3
               && MercenaryHiringLimitPolicy.CanHireAnother(2, 3)
               && !MercenaryHiringLimitPolicy.CanHireAnother(3, 3)
               && MercenaryHiringLimitPolicy.CanHireAnother(3, 4)
               && !MercenaryHiringLimitPolicy.CanHireAnother(4, 4),
            "the hiring limit defaults to three and blocks only at the configured maximum");
        Assert(MercenaryHiringLimitPolicy.ParseMaximumHiredMercenaryClans(
                   "<MercenaryManagementSettings><MaximumHiredMercenaryClans value=\"5\" /></MercenaryManagementSettings>") == 5
               && MercenaryHiringLimitPolicy.ParseMaximumHiredMercenaryClans(
                   "<MercenaryManagementSettings><MaximumHiredMercenaryClans>4</MaximumHiredMercenaryClans></MercenaryManagementSettings>") == 4
               && MercenaryHiringLimitPolicy.ParseMaximumHiredMercenaryClans(
                   "<MercenaryManagementSettings><MaximumHiredMercenaryClans value=\"0\" /></MercenaryManagementSettings>") == 1
               && MercenaryHiringLimitPolicy.ParseMaximumHiredMercenaryClans(
                   "<MercenaryManagementSettings><MaximumHiredMercenaryClans value=\"99\" /></MercenaryManagementSettings>") == 20,
            "the hiring limit is read from the settings file and clamped to 1..20");
        Assert(MercenaryHiringLimitPolicy.ParseMaximumHiredMercenaryClans(null) == 3
               && MercenaryHiringLimitPolicy.ParseMaximumHiredMercenaryClans("") == 3
               && MercenaryHiringLimitPolicy.ParseMaximumHiredMercenaryClans("not xml") == 3
               && MercenaryHiringLimitPolicy.ParseMaximumHiredMercenaryClans(
                   "<MercenaryManagementSettings><MaximumHiredMercenaryClans value=\"many\" /></MercenaryManagementSettings>") == 3
               && MercenaryHiringLimitPolicy.ParseMaximumHiredMercenaryClans("<MercenaryManagementSettings />") == 3,
            "a missing or unreadable hiring limit keeps the default of three");
        MercenaryHiringLimitPolicy.Load("/no/such/module/directory");
        Assert(MercenaryHiringLimitPolicy.MaximumHiredMercenaryClans == 3,
            "without a settings file the hiring limit stays at three");
    }

    private static void TestPlayerRecruitmentAdvantage()
    {
        Assert(MercenaryPlayerRecruitmentPolicy.ApplyPlayerRulerAcceptanceAdvantage(5) == 75
               && MercenaryPlayerRecruitmentPolicy.ApplyPlayerRulerAcceptanceAdvantage(60) == 80
               && MercenaryPlayerRecruitmentPolicy.ApplyPlayerRulerAcceptanceAdvantage(90) == 95,
            "player ruler recruitment has a meaningful floor, bonus, and bounded maximum");
        Assert(!MercenaryPlayerRecruitmentPolicy.HasCompletedProtectedService(6.99f)
               && MercenaryPlayerRecruitmentPolicy.HasCompletedProtectedService(7f)
               && MercenaryPlayerRecruitmentPolicy.HasCompletedProtectedService(120f),
            "player poaching replaces the full contract lock with a seven-day loyalty window");
        Assert(!MercenaryPlayerRecruitmentPolicy.HasCompletedProtectedService(float.NaN)
               && !MercenaryPlayerRecruitmentPolicy.HasCompletedProtectedService(float.PositiveInfinity),
            "invalid service time cannot bypass the poaching loyalty window");
    }

    private static void TestAiControlBoundaries()
    {
        Assert(!MercenaryAiControlPolicy.CanMakeAutonomousContractDecision(isPlayerClan: true),
            "AI cannot make autonomous hiring or departure decisions for the player clan");
        Assert(MercenaryAiControlPolicy.CanMakeAutonomousContractDecision(isPlayerClan: false),
            "AI can still make autonomous contract decisions for non-player clans");
    }

    private static void TestRecoveryPriority()
    {
        Assert(MercenaryRecoveryPolicy.GetMultiplier(RecoveryDecision.VisitSettlement) >= 2f
               && MercenaryRecoveryPolicy.GetMultiplier(RecoveryDecision.RecruitTroops) >= 2f
               && MercenaryRecoveryPolicy.GetMultiplier(RecoveryDecision.TrainTroops) > 1.5f,
            "recovery significantly favors recruitment, settlement visits, and training");
        Assert(MercenaryRecoveryPolicy.GetMultiplier(RecoveryDecision.JoinArmy) < 0.5f
               && MercenaryRecoveryPolicy.GetMultiplier(RecoveryDecision.OffensiveCombat) < 0.5f
               && MercenaryRecoveryPolicy.GetMultiplier(RecoveryDecision.DefendSelf) == 1f,
            "recovery avoids optional combat without weakening self-defense");
        Assert(!MercenaryRecoveryPolicy.ShouldComplete(10d, 20d,
                new[] { 0.95f, 0.89f }, 0.9f)
               && MercenaryRecoveryPolicy.ShouldComplete(10d, 20d,
                   new[] { 0.95f, 0.9f }, 0.9f),
            "recovery completes only when every active party reaches its target");
        Assert(MercenaryRecoveryPolicy.ShouldComplete(20d, 20d,
                Array.Empty<float>(), 0.9f)
               && !MercenaryRecoveryPolicy.ShouldComplete(19.99d, 20d,
                   Array.Empty<float>(), 0.9f),
            "recovery deadline completes independently and an empty clan does not fake its goal");
        Assert(MercenaryRecoveryPolicy.NormalizeDurationDays(float.NaN)
                   == MercenaryRecoveryPolicy.DefaultDurationDays
               && MercenaryRecoveryPolicy.NormalizeDurationDays(-500f)
                   == MercenaryRecoveryPolicy.MinimumDurationDays
               && MercenaryRecoveryPolicy.NormalizeDurationDays(float.PositiveInfinity)
                   == MercenaryRecoveryPolicy.DefaultDurationDays,
            "nonstandard recovery durations are normalized safely");
        Assert(MercenaryRecoveryPolicy.NormalizeTargetPartySizeRatio(float.NaN)
                   == MercenaryRecoveryPolicy.DefaultTargetPartySizeRatio
               && MercenaryRecoveryPolicy.NormalizeTargetPartySizeRatio(-4f)
                   == MercenaryRecoveryPolicy.MinimumTargetPartySizeRatio
               && MercenaryRecoveryPolicy.NormalizeTargetPartySizeRatio(4f) == 1f,
            "nonstandard recovery targets are normalized safely");
        Assert(MercenaryRecoveryPolicy.GetRestoredPriority(
                   MercenaryStrategicPriority.TerritorialDefense)
                   == MercenaryStrategicPriority.TerritorialDefense
               && MercenaryRecoveryPolicy.GetRestoredPriority(
                   MercenaryStrategicPriority.Recovery)
                   == MercenaryStrategicPriority.IndependentOperations,
            "temporary recovery restores a valid previous priority and rejects recursive recovery");
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

    private static void TestArmyLifecycleScenarios()
    {
        Assert(CreatedArmy(hasArmy: false, hasKingdom: false)
               == ArmyLifecycleDecision.Ignore,
            "missing army event is ignored without failure");
        Assert(CreatedArmy(hasArmy: true, hasKingdom: false)
               == ArmyLifecycleDecision.Ignore,
            "created army with cleared kingdom is ignored without failure");
        Assert(CreatedArmy(kingdomIsEliminated: true)
               == ArmyLifecycleDecision.Ignore,
            "eliminated kingdom cannot trigger army priority updates");
        Assert(CreatedArmy(playerRulesKingdom: true)
               == ArmyLifecycleDecision.Ignore,
            "AI army lifecycle does not manage the player ruler kingdom");
        Assert(CreatedArmy(playerRulesKingdom: false)
               == ArmyLifecycleDecision.ProcessCreatedArmy,
            "player-aligned kingdom remains AI-managed when the player is not its ruler");
        Assert(CreatedArmy(hasArmy: false, hasKingdom: true)
               == ArmyLifecycleDecision.Ignore,
            "contradictory creation snapshot cannot reach kingdom processing");
        Assert(CreatedArmy()
               == ArmyLifecycleDecision.ProcessCreatedArmy,
            "valid AI army creation is processed");

        Assert(DispersedArmy(otherActiveArmyCount: 1)
               == ArmyLifecycleDecision.Ignore,
            "dispersing one of two armies keeps army priorities unchanged");
        Assert(DispersedArmy(otherActiveArmyCount: 0)
               == ArmyLifecycleDecision.ReevaluateAfterLastArmyDispersed,
            "dispersing the last army triggers priority reevaluation");
        Assert(DispersedArmy(hasKingdom: false)
               == ArmyLifecycleDecision.Ignore,
            "leader destruction with cleared kingdom is ignored without failure");
        Assert(DispersedArmy(hasArmy: false, hasKingdom: false)
               == ArmyLifecycleDecision.Ignore,
            "repeated dispersal after full cleanup is idempotently ignored");
        Assert(DispersedArmy(kingdomIsEliminated: true)
               == ArmyLifecycleDecision.Ignore,
            "last army of an eliminated kingdom cannot update contracts");
        Assert(DispersedArmy(otherActiveArmyCount: 3)
               == ArmyLifecycleDecision.Ignore,
            "multiple remaining armies never cause false last-army handling");
        Assert(DispersedArmy(otherActiveArmyCount: -1)
               == ArmyLifecycleDecision.Ignore,
            "invalid negative army count fails closed without a priority update");
        Assert(DispersedArmy(hasArmy: false, hasKingdom: true)
               == ArmyLifecycleDecision.Ignore,
            "contradictory dispersal snapshot is ignored without failure");
    }

    private static void TestArmyPartyLifecycleSafety()
    {
        Assert(!CanEvaluatePartyExit(hasArmy: false, hasArmyLeader: false),
            "party tick without an army exits safely");
        Assert(!CanEvaluatePartyExit(hasArmy: false, hasArmyLeader: true),
            "stale leader reference without an army exits safely");
        Assert(!CanEvaluatePartyExit(hasArmyLeader: false),
            "partially dispersed army without a leader exits safely");
        Assert(!CanEvaluatePartyExit(isMainParty: true),
            "main party is never detached by daily AI policy");
        Assert(!CanEvaluatePartyExit(isActive: false),
            "inactive party is ignored during army cleanup");
        Assert(!CanEvaluatePartyExit(isDisbanding: true),
            "disbanding party is ignored during army cleanup");
        Assert(!CanEvaluatePartyExit(isArmyLeader: true),
            "army leader is never detached by member leave policy");
        Assert(!CanEvaluatePartyExit(
                hasArmy: false,
                hasArmyLeader: false,
                isMainParty: true,
                isActive: false,
                isDisbanding: true,
                isArmyLeader: true),
            "fully inconsistent party cleanup state exits safely");
        Assert(CanEvaluatePartyExit(),
            "healthy non-leader army member can reach normal leave evaluation");
    }

    private static void TestUnexpectedContributionInputs()
    {
        MercenaryStrategicPriority[] priorities =
        {
            MercenaryStrategicPriority.IndependentOperations,
            MercenaryStrategicPriority.JoinArmies,
            MercenaryStrategicPriority.EconomicWarfare,
            MercenaryStrategicPriority.TerritorialDefense,
            (MercenaryStrategicPriority)(-1),
            (MercenaryStrategicPriority)int.MaxValue
        };
        PassiveContributionEvidence[] evidenceValues =
        {
            PassiveContributionEvidence.None,
            PassiveContributionEvidence.ServingInArmy,
            PassiveContributionEvidence.EconomicOperation,
            PassiveContributionEvidence.ActiveTerritorialDefense,
            (PassiveContributionEvidence)(-1),
            (PassiveContributionEvidence)int.MaxValue
        };
        int eligiblePairs = 0;
        foreach (MercenaryStrategicPriority priority in priorities)
        foreach (PassiveContributionEvidence evidence in evidenceValues)
        {
            if (MercenaryContributionPolicy.IsEligible(priority, evidence))
                eligiblePairs++;
        }

        Assert(eligiblePairs == 3,
            "only the three explicit priority/evidence pairs can earn passive contribution");
        Assert(!MercenaryContributionPolicy.CanRecordDaily(float.NaN)
               && !MercenaryContributionPolicy.CanRecordDaily(float.PositiveInfinity)
               && !MercenaryContributionPolicy.CanRecordDaily(float.NegativeInfinity),
            "non-finite elapsed time cannot create contribution");
        Assert(MercenaryContributionPolicy.CanRecordDaily(-1f)
               && !MercenaryContributionPolicy.CanRecordDaily(0f)
               && !MercenaryContributionPolicy.CanRecordDaily(0.999999f)
               && MercenaryContributionPolicy.CanRecordDaily(1f),
            "daily contribution boundary is deterministic");
    }

    private static void TestEvaluationInputRobustness()
    {
        float[] hostileFloats =
        {
            float.NaN,
            float.PositiveInfinity,
            float.NegativeInfinity,
            float.MinValue,
            float.MaxValue,
            -10000f,
            10000f
        };
        bool allResultsSafe = true;
        foreach (float value in hostileFloats)
        {
            DailyEvaluationResult result = MercenaryEvaluationModel.Evaluate(
                new DailyEvaluationInputs(
                    value, value, value, value, int.MaxValue, value,
                    int.MaxValue, int.MinValue));
            allResultsSafe &= IsPercentage(result.Satisfaction)
                              && IsPercentage(result.Reliability)
                              && IsPercentage(result.EmployerSatisfaction);
        }
        Assert(allResultsSafe,
            "evaluation returns finite percentages for malformed numeric inputs");

        DailyEvaluationResult lowRate = MercenaryEvaluationModel.Evaluate(
            new DailyEvaluationInputs(50, 50, 50, 0.8f, 0, 1f, 0, 0));
        DailyEvaluationResult highRate = MercenaryEvaluationModel.Evaluate(
            new DailyEvaluationInputs(50, 50, 50, 1.2f, 0, 1f, 0, 0));
        Assert(highRate.Satisfaction >= lowRate.Satisfaction,
            "raising valid pay cannot reduce mercenary satisfaction");

        DailyEvaluationResult negativeContribution = MercenaryEvaluationModel.Evaluate(
            new DailyEvaluationInputs(50, 50, 50, 1f, 0, 1f, 0, int.MinValue));
        DailyEvaluationResult zeroContribution = MercenaryEvaluationModel.Evaluate(
            new DailyEvaluationInputs(50, 50, 50, 1f, 0, 1f, 0, 0));
        Assert(negativeContribution.Reliability == zeroContribution.Reliability
               && negativeContribution.EmployerSatisfaction
               == zeroContribution.EmployerSatisfaction,
            "negative contribution is normalized to no contribution");

        int[] bands =
        {
            MercenaryEvaluationModel.GetPerformanceBand(int.MinValue, float.NaN),
            MercenaryEvaluationModel.GetPerformanceBand(-1, -100f),
            MercenaryEvaluationModel.GetPerformanceBand(int.MaxValue, float.PositiveInfinity)
        };
        Assert(Array.TrueForAll(bands, band => band >= 0 && band <= 4),
            "performance band remains valid for extreme service data");
    }

    private static void TestRiskInputRobustnessAndMonotonicity()
    {
        float[] hostileFloats =
        {
            float.NaN,
            float.PositiveInfinity,
            float.NegativeInfinity,
            float.MinValue,
            float.MaxValue,
            -10000f,
            10000f
        };
        bool allBreakdownsSafe = true;
        foreach (float value in hostileFloats)
        {
            RiskBreakdown departure = MercenaryRiskModel.CalculateDepartureRisk(
                new DepartureRiskInputs(value, value, value, int.MinValue,
                    value, value, value, value, true));
            RiskBreakdown termination = MercenaryRiskModel.CalculateEmployerTerminationRisk(
                new EmployerTerminationRiskInputs(value, value, value, value));
            allBreakdownsSafe &= IsSafeBreakdown(departure)
                                 && IsSafeBreakdown(termination);
        }
        Assert(allBreakdownsSafe,
            "risk totals and displayed factors stay finite for malformed inputs");

        RiskBreakdown satisfied = MercenaryRiskModel.CalculateDepartureRisk(
            new DepartureRiskInputs(90, 50, 1f, 0, 1f, 0.5f, 0, 60, true));
        RiskBreakdown dissatisfied = MercenaryRiskModel.CalculateDepartureRisk(
            new DepartureRiskInputs(10, 50, 1f, 0, 1f, 0.5f, 0, 60, true));
        Assert(dissatisfied.Total > satisfied.Total,
            "lower satisfaction strictly increases departure risk");

        RiskBreakdown longTerm = MercenaryRiskModel.CalculateDepartureRisk(
            new DepartureRiskInputs(50, 50, 1f, 0, 1f, 0.5f, 0, 60, true));
        RiskBreakdown expiring = MercenaryRiskModel.CalculateDepartureRisk(
            new DepartureRiskInputs(50, 50, 1f, 0, 1f, 0.5f, 0, 0, true));
        Assert(expiring.Total > longTerm.Total,
            "an expiring finite contract raises departure risk");

        RiskBreakdown lowNeed = MercenaryRiskModel.CalculateEmployerTerminationRisk(
            new EmployerTerminationRiskInputs(50, 1f, 0f, 50));
        RiskBreakdown highNeed = MercenaryRiskModel.CalculateEmployerTerminationRisk(
            new EmployerTerminationRiskInputs(50, 1f, 1f, 50));
        Assert(lowNeed.Total > highNeed.Total,
            "lower military need strictly increases employer termination risk");
    }

    private static void TestOfferInputRobustness()
    {
        OfferTermsResult reversedBounds = MercenaryOfferModel.CalculateTerms(
            100, 150, 85, 0.5f, 50f, false);
        Assert(reversedBounds.InfluencePrice >= 85
               && reversedBounds.InfluencePrice <= 150,
            "reversed offer bounds are normalized safely");

        OfferTermsResult negativePrices = MercenaryOfferModel.CalculateTerms(
            -100, -50, -10, 0.5f, 50f, false);
        Assert(negativePrices.InfluencePrice == 0,
            "negative offer prices cannot escape into a contract");

        float[] hostileFloats =
        {
            float.NaN,
            float.PositiveInfinity,
            float.NegativeInfinity,
            float.MinValue,
            float.MaxValue
        };
        bool allOffersSafe = true;
        foreach (float value in hostileFloats)
        {
            OfferTermsResult result = MercenaryOfferModel.CalculateTerms(
                int.MaxValue, 85, 150, value, value, false);
            allOffersSafe &= result.InfluencePrice >= 85
                             && result.InfluencePrice <= 150
                             && (result.DurationDays == 30
                                 || result.DurationDays == 60
                                 || result.DurationDays == 120);
        }
        Assert(allOffersSafe,
            "offer calculation contains extreme and non-finite campaign inputs");

        OfferTermsResult requested = MercenaryOfferModel.CalculateTerms(
            100, 0, 200, 0.5f, 50f, true);
        OfferTermsResult unsolicited = MercenaryOfferModel.CalculateTerms(
            100, 0, 200, 0.5f, 50f, false);
        Assert(requested.InfluencePrice <= unsolicited.InfluencePrice,
            "requesting services cannot improve the calculated price over an unsolicited offer");
    }

    private static void TestPersistentValueRobustness()
    {
        Assert(MercenaryValuePolicy.NormalizePercentage(-1f) == 0f
               && MercenaryValuePolicy.NormalizePercentage(101f) == 100f
               && MercenaryValuePolicy.NormalizePercentage(42f) == 42f,
            "saveable percentages retain valid values and clamp normal overflow");
        Assert(MercenaryValuePolicy.NormalizePercentage(float.NaN) == 50f
               && MercenaryValuePolicy.NormalizePercentage(float.PositiveInfinity) == 50f
               && MercenaryValuePolicy.NormalizePercentage(25f, float.NaN) == 25f,
            "non-finite saveable percentages recover to a finite neutral value");
        Assert(MercenaryValuePolicy.AdjustPercentage(70f, float.NaN) == 70f
               && MercenaryValuePolicy.AdjustPercentage(float.NaN, 5f) == 55f
               && MercenaryValuePolicy.AdjustPercentage(99f, 5f) == 100f,
            "malformed evaluation deltas cannot poison existing contract state");
        Assert(MercenaryValuePolicy.AccumulateNonNegative(10, 5) == 15
               && MercenaryValuePolicy.AccumulateNonNegative(10, -20) == 0,
            "normal contribution accumulation preserves its non-negative invariant");
        Assert(MercenaryValuePolicy.AccumulateNonNegative(int.MaxValue, 1) == int.MaxValue
               && MercenaryValuePolicy.AccumulateNonNegative(int.MaxValue, int.MaxValue)
               == int.MaxValue
               && MercenaryValuePolicy.AccumulateNonNegative(int.MinValue, int.MinValue) == 0,
            "contribution and earnings accumulation cannot wrap around integer limits");
    }

    private static void TestContractReputation()
    {
        float successful = MercenaryReputationModel.Calculate(
            new ContractReputationInputs(90f, 85f, 100, 20, 60f));
        float failed = MercenaryReputationModel.Calculate(
            new ContractReputationInputs(20f, 15f, 0, -20, 60f));
        float shortService = MercenaryReputationModel.Calculate(
            new ContractReputationInputs(90f, 85f, 100, 20, 2f));
        Assert(successful > 75f && failed < 30f,
            "completed contract results produce meaningful reputation");
        Assert(shortService > 50f && shortService < successful,
            "very short service has limited reputation weight");
        Assert(MercenaryReputationModel.Combine(40f, 90f, true)
               > MercenaryReputationModel.Combine(40f, 90f, false),
            "previous service with the same employer affects later negotiations more strongly");

        float malformed = MercenaryReputationModel.Calculate(
            new ContractReputationInputs(float.NaN, float.PositiveInfinity,
                int.MinValue, int.MaxValue, float.NaN));
        Assert(IsPercentage(malformed),
            "malformed saved history cannot poison reputation");
    }

    private static bool IsPercentage(float value)
    {
        return IsFinite(value) && value >= 0f && value <= 100f;
    }

    private static bool IsSafeBreakdown(RiskBreakdown breakdown)
    {
        return IsPercentage(breakdown.Total)
               && IsFinite(breakdown.Base)
               && IsFinite(breakdown.Satisfaction)
               && IsFinite(breakdown.Reliability)
               && IsFinite(breakdown.Rate)
               && IsFinite(breakdown.Relation)
               && IsFinite(breakdown.Losses)
               && IsFinite(breakdown.MilitaryNeed)
               && IsFinite(breakdown.PriorityMismatch)
               && IsFinite(breakdown.RemainingTerm);
    }

    private static bool IsFinite(float value)
    {
        return !float.IsNaN(value) && !float.IsInfinity(value);
    }

    private static ArmyLifecycleDecision CreatedArmy(
        bool hasArmy = true,
        bool hasKingdom = true,
        bool kingdomIsEliminated = false,
        bool playerRulesKingdom = false)
    {
        return MercenaryArmyLifecyclePolicy.OnArmyCreated(
            new ArmyLifecycleSnapshot(
                hasArmy,
                hasKingdom,
                kingdomIsEliminated,
                playerRulesKingdom,
                0));
    }

    private static ArmyLifecycleDecision DispersedArmy(
        bool hasArmy = true,
        bool hasKingdom = true,
        bool kingdomIsEliminated = false,
        bool playerRulesKingdom = false,
        int otherActiveArmyCount = 0)
    {
        return MercenaryArmyLifecyclePolicy.OnArmyDispersed(
            new ArmyLifecycleSnapshot(
                hasArmy,
                hasKingdom,
                kingdomIsEliminated,
                playerRulesKingdom,
                otherActiveArmyCount));
    }

    private static bool CanEvaluatePartyExit(
        bool hasArmy = true,
        bool hasArmyLeader = true,
        bool isMainParty = false,
        bool isActive = true,
        bool isDisbanding = false,
        bool isArmyLeader = false)
    {
        return MercenaryArmyLifecyclePolicy.CanEvaluatePartyExit(
            new ArmyPartyLifecycleSnapshot(
                hasArmy,
                hasArmyLeader,
                isMainParty,
                isActive,
                isDisbanding,
                isArmyLeader));
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
