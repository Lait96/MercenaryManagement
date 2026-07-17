using System;
using MercenaryManagement.Contracts;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;
using TaleWorlds.CampaignSystem.Actions;

namespace MercenaryManagement.Services;

internal static class MercenaryEvaluationService
{
    public static void EvaluateDaily(MercenaryContract contract)
    {
        Clan? clan = contract.Clan;
        Kingdom? kingdom = contract.EmployerKingdom;
        if (clan is null || kingdom is null || clan.Leader is null)
        {
            return;
        }
        contract.DecayContribution();

        int baseRate = MercenaryPaymentService.GetBaseInfluencePrice(clan);
        float rateRatio = contract.InfluencePrice / (float)baseRate;
        DailyEvaluationResult result = MercenaryEvaluationModel.Evaluate(
            new DailyEvaluationInputs(contract.Satisfaction, contract.Reliability,
                contract.EmployerSatisfaction, rateRatio,
                clan.Leader.GetRelation(kingdom.Leader), GetAveragePartySizeRatio(clan),
                clan.DebtToKingdom, contract.ContributionPoints));
        contract.SetEvaluation(result.Satisfaction, result.Reliability, CampaignTime.Now);
        contract.SetEmployerSatisfaction(result.EmployerSatisfaction);
        if (contract.TryConsumeContributionMilestone())
        {
            contract.AdjustEvaluation(4f, 2f);
            if (kingdom.Leader is not null)
                ChangeRelationAction.ApplyRelationChangeBetweenHeroes(
                    clan.Leader, kingdom.Leader, 1, true);
        }
    }

    public static float CalculateDepartureRisk(MercenaryContract contract)
        => GetDepartureRiskBreakdown(contract).Total;

    public static RiskBreakdown GetDepartureRiskBreakdown(MercenaryContract contract)
    {
        Clan? clan = contract.Clan;
        Kingdom? kingdom = contract.EmployerKingdom;
        if (clan is null || kingdom is null)
        {
            return new RiskBreakdown { Total = 100f };
        }

        int baseRate = MercenaryPaymentService.GetBaseInfluencePrice(clan);
        float rateRatio = contract.InfluencePrice / (float)baseRate;
        int relation = clan.Leader is not null && kingdom.Leader is not null
            ? clan.Leader.GetRelation(kingdom.Leader)
            : 0;
        bool finite = contract.EndTime != CampaignTime.Zero
                      && contract.EndTime != CampaignTime.Never;
        return MercenaryRiskModel.CalculateDepartureRisk(new DepartureRiskInputs(
            contract.Satisfaction, contract.Reliability, rateRatio, relation,
            GetAveragePartySizeRatio(clan), MercenaryHiringService.CalculateMilitaryNeed(kingdom),
            GetPriorityMismatchPenalty(contract, kingdom),
            finite ? contract.EndTime.RemainingDaysFromNow : 0f, finite));
    }

    public static string GetPerformanceSummary(MercenaryContract contract)
    {
        switch (MercenaryEvaluationModel.GetPerformanceBand(
                    contract.ContributionPoints,
                    contract.ServiceStartTime.ElapsedDaysUntilNow))
        {
            case 4: return Localize("MercenaryManagement_PerformanceExcellent", "Excellent");
            case 3: return Localize("MercenaryManagement_PerformanceGood", "Good");
            case 2: return Localize("MercenaryManagement_PerformanceDeveloping", "Developing");
            case 1: return Localize("MercenaryManagement_PerformanceInitial", "Initial assessment");
            default: return Localize("MercenaryManagement_PerformanceLimited", "Limited results");
        }
    }

    public static float CalculateEmployerTerminationRisk(MercenaryContract contract)
        => GetEmployerTerminationRiskBreakdown(contract).Total;

    public static RiskBreakdown GetEmployerTerminationRiskBreakdown(MercenaryContract contract)
    {
        Clan? clan = contract.Clan;
        Kingdom? kingdom = contract.EmployerKingdom;
        if (clan is null || kingdom is null)
        {
            return new RiskBreakdown { Total = 100f };
        }

        int baseRate = MercenaryPaymentService.GetBaseInfluencePrice(clan);
        float rateRatio = contract.InfluencePrice / (float)baseRate;
        return MercenaryRiskModel.CalculateEmployerTerminationRisk(
            new EmployerTerminationRiskInputs(contract.EmployerSatisfaction, rateRatio,
                MercenaryHiringService.CalculateMilitaryNeed(kingdom), contract.Reliability));
    }

    private static float GetPriorityMismatchPenalty(
        MercenaryContract contract,
        Kingdom kingdom)
    {
        switch (contract.StrategicPriority)
        {
            case MercenaryStrategicPriority.JoinArmies:
                return kingdom.Armies.Count == 0 ? 8f : 0f;
            case MercenaryStrategicPriority.EconomicWarfare:
            case MercenaryStrategicPriority.TerritorialDefense:
                return MercenaryHiringService.CalculateMilitaryNeed(kingdom) <= 0f ? 6f : 0f;
            default:
                return 0f;
        }
    }

    public static string GetActivitySummary(MercenaryContract contract)
    {
        if (contract.LastActivityTime == CampaignTime.Zero
            || contract.LastActivityTime.ElapsedDaysUntilNow > 7f)
            return Localize("MercenaryManagement_ActivityUnknown", "No recent activity reported");
        switch (contract.LastActivity)
        {
            case "JoinedArmy": return Localize("MercenaryManagement_ActivityJoinedArmy", "Recently joined an army");
            case "CompletedRaid": return Localize("MercenaryManagement_ActivityCompletedRaid", "Recently completed a raid");
            case "ServingInArmy": return Localize("MercenaryManagement_ActivityServingArmy", "Serving in an army");
            case "EconomicOperation": return Localize("MercenaryManagement_ActivityEconomic", "Conducting economic warfare");
            case "DefendingTerritory": return Localize("MercenaryManagement_ActivityDefense", "Operating in friendly territory");
            case "ClearedBandits": return Localize("MercenaryManagement_ActivityClearedBandits", "Recently defeated bandits");
            case "ClearedHideout": return Localize("MercenaryManagement_ActivityClearedHideout", "Recently cleared a bandit hideout");
            case "IndependentOperation": return Localize("MercenaryManagement_ActivityIndependent", "Conducting independent operations");
            default: return Localize("MercenaryManagement_ActivityUnknown", "No recent activity reported");
        }
    }

    private static float GetAveragePartySizeRatio(Clan clan)
    {
        float total = 0f;
        int count = 0;
        foreach (var component in clan.WarPartyComponents)
        {
            if (!component.MobileParty.IsActive) continue;
            total += component.MobileParty.PartySizeRatio;
            count++;
        }
        return count == 0 ? 1f : total / count;
    }

    private static float Clamp(float value) => Math.Max(0f, Math.Min(100f, value));
    private static string Localize(string id, string fallback) => new TextObject($"{{={id}}}{fallback}").ToString();
}
