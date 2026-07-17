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
        float target = 50f + (rateRatio - 1f) * 60f;
        target += Math.Max(-10, Math.Min(10, clan.Leader.GetRelation(kingdom.Leader))) * 0.5f;
        target -= Math.Max(0f, 0.65f - GetAveragePartySizeRatio(clan)) * 30f;
        target -= Math.Min(15f, Math.Max(0, clan.DebtToKingdom) / 200f);
        target += Math.Min(10f, contract.ContributionPoints / 20f);

        float satisfaction = MoveTowards(contract.Satisfaction, target, 3f);
        float reliabilityTarget = 50f + Math.Min(25f, contract.ContributionPoints / 10f);
        float reliability = MoveTowards(contract.Reliability, reliabilityTarget, 0.5f);
        contract.SetEvaluation(satisfaction, reliability, CampaignTime.Now);
        float employerTarget = 40f + System.Math.Min(35f, contract.ContributionPoints / 8f)
                               + (contract.Reliability - 50f) * 0.25f
                               - System.Math.Max(0f, rateRatio - 1f) * 20f;
        contract.SetEmployerSatisfaction(MoveTowards(
            contract.EmployerSatisfaction, employerTarget, 2f));
        if (contract.TryConsumeContributionMilestone())
        {
            contract.AdjustEvaluation(4f, 2f);
            if (kingdom.Leader is not null)
                ChangeRelationAction.ApplyRelationChangeBetweenHeroes(
                    clan.Leader, kingdom.Leader, 1, true);
        }
    }

    public static float CalculateDepartureRisk(MercenaryContract contract)
    {
        float risk = 65f - contract.Satisfaction * 0.45f - contract.Reliability * 0.2f;
        if (contract.EndTime != CampaignTime.Zero && contract.EndTime != CampaignTime.Never)
        {
            risk += Math.Max(0f, 10f - contract.EndTime.RemainingDaysFromNow);
        }
        return Clamp(risk);
    }

    public static string GetPerformanceSummary(MercenaryContract contract)
    {
        if (contract.ContributionPoints >= 30) return Localize("MercenaryManagement_PerformanceExcellent", "Excellent");
        if (contract.ContributionPoints >= 15) return Localize("MercenaryManagement_PerformanceGood", "Good");
        if (contract.ContributionPoints >= 3) return Localize("MercenaryManagement_PerformanceDeveloping", "Developing");
        return contract.ServiceStartTime.ElapsedDaysUntilNow < 2f
            ? Localize("MercenaryManagement_PerformanceInitial", "Initial assessment")
            : Localize("MercenaryManagement_PerformanceLimited", "Limited results");
    }

    public static float CalculateEmployerTerminationRisk(MercenaryContract contract)
    {
        float risk = 75f - contract.EmployerSatisfaction * 0.65f
                     - contract.ContributionPoints * 0.5f;
        return Clamp(risk);
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

    private static float MoveTowards(float value, float target, float maximumDelta)
    {
        if (value < target) return Math.Min(value + maximumDelta, target);
        return Math.Max(value - maximumDelta, target);
    }

    private static float Clamp(float value) => Math.Max(0f, Math.Min(100f, value));
    private static string Localize(string id, string fallback) => new TextObject($"{{={id}}}{fallback}").ToString();
}
