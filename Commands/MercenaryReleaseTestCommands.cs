using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using HarmonyLib;
using MercenaryManagement.Behaviors;
using MercenaryManagement.Contracts;
using MercenaryManagement.Services;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Library;

namespace MercenaryManagement.Commands;

public static class MercenaryReleaseTestCommands
{
    [CommandLineFunctionality.CommandLineArgumentFunction("mm_help", "campaign")]
    public static string Help(List<string> arguments)
    {
        if (arguments.Count != 0) return "Usage: campaign.mm_help";
        return "Mercenary Management release-test commands:\n"
            + "mm_contract_state <clan>\nmm_contribution_add <points> <reason> | <clan>\n"
            + "mm_contract_tick <count> <clan>\nmm_set_evaluation <mercenary> <employer> <reliability> | <clan>\n"
            + "mm_risks <clan>\nmm_set_priority <independent|armies|economic|defense> <clan>\n"
            + "mm_contract_term <30|60|120|indefinite|war> <clan>\n"
            + "mm_contract_renew <days> <clan>\nmm_contract_finish <clan>\n"
            + "mm_contract_check <normal|force_war_ended> <clan>\n"
            + "mm_offer_services <kingdom>\nmm_offers [clan]\n"
            + "mm_offer_accept <kingdom>\nmm_offer_reject <kingdom>\n"
            + "mm_ai_evaluate <departure|dismissal|renewal> <preview|apply> <clan>\n"
            + "mm_ai_hire <preview|apply> <kingdom>\nmm_party_state <clan>\n"
            + "mm_contribution_probe <clan>\nmm_harmony_status\n"
            + "mm_runtime_test <free|vassal|mercenary|ruler> confirm";
    }

    [CommandLineFunctionality.CommandLineArgumentFunction("mm_contract_state", "campaign")]
    public static string ContractState(List<string> arguments)
    {
        if (!TryContract(arguments, out Clan clan, out MercenaryContract contract, out string error)) return error;
        return FormatContract(clan, contract);
    }

    [CommandLineFunctionality.CommandLineArgumentFunction("mm_contribution_add", "campaign")]
    public static string AddContribution(List<string> arguments)
    {
        if (!MercenaryDebugCommands.TryValidateCampaign(out string error)) return error;
        int separator = arguments.IndexOf("|");
        if (separator < 2 || !int.TryParse(arguments[0], out int points) || points <= 0)
            return "Usage: campaign.mm_contribution_add <positive points> <reason> | <clan>";
        string reason = string.Join(" ", arguments.Skip(1).Take(separator - 1));
        if (!MercenaryDebugCommands.TryFindClan(
                MercenaryDebugCommands.JoinArguments(arguments.Skip(separator + 1)), out Clan clan, out error)) return error;
        if (!TryGetContract(clan, out MercenaryContract contract, out error)) return error;
        contract.RecordContribution(points, "Diagnostic:" + reason);
        return $"Added {points} confirmed contribution points to {clan.Name}. total={contract.ContributionPoints}; reason={reason}";
    }

    [CommandLineFunctionality.CommandLineArgumentFunction("mm_contract_tick", "campaign")]
    public static string ContractTick(List<string> arguments)
    {
        if (!MercenaryDebugCommands.TryValidateCampaign(out string error)) return error;
        if (arguments.Count < 2 || !int.TryParse(arguments[0], out int count) || count < 1 || count > 365)
            return "Usage: campaign.mm_contract_tick <1..365> <clan>";
        if (!MercenaryDebugCommands.TryFindClan(MercenaryDebugCommands.JoinArguments(arguments.Skip(1)), out Clan clan, out error)) return error;
        MercenaryContractBehavior? behavior = Campaign.Current!.GetCampaignBehavior<MercenaryContractBehavior>();
        if (behavior?.RunDiagnosticDailyEvaluation(clan, count) != true) return $"{clan.Name} has no active contract.";
        behavior.TryGetContract(clan, out MercenaryContract contract);
        return $"Executed {count} contract-only daily evaluations without changing campaign time.\n{FormatContract(clan, contract)}";
    }

    [CommandLineFunctionality.CommandLineArgumentFunction("mm_set_evaluation", "campaign")]
    public static string SetEvaluation(List<string> arguments)
    {
        if (!MercenaryDebugCommands.TryValidateCampaign(out string error)) return error;
        int separator = arguments.IndexOf("|");
        if (separator != 3
            || !TryPercent(arguments[0], out float mercenary)
            || !TryPercent(arguments[1], out float employer)
            || !TryPercent(arguments[2], out float reliability))
            return "Usage: campaign.mm_set_evaluation <mercenary 0..100> <employer 0..100> <reliability 0..100> | <clan>";
        if (!MercenaryDebugCommands.TryFindClan(MercenaryDebugCommands.JoinArguments(arguments.Skip(4)), out Clan clan, out error)) return error;
        if (!TryGetContract(clan, out MercenaryContract contract, out error)) return error;
        contract.SetEvaluation(mercenary, reliability, CampaignTime.Now);
        contract.SetEmployerSatisfaction(employer);
        return FormatContract(clan, contract);
    }

    [CommandLineFunctionality.CommandLineArgumentFunction("mm_risks", "campaign")]
    public static string Risks(List<string> arguments)
    {
        if (!TryContract(arguments, out Clan clan, out MercenaryContract contract, out string error)) return error;
        return FormatRisks(clan, contract);
    }

    [CommandLineFunctionality.CommandLineArgumentFunction("mm_set_priority", "campaign")]
    public static string SetPriority(List<string> arguments)
    {
        if (!MercenaryDebugCommands.TryValidateCampaign(out string error)) return error;
        if (arguments.Count < 2 || !TryPriority(arguments[0], out MercenaryStrategicPriority priority))
            return "Usage: campaign.mm_set_priority <independent|armies|economic|defense> <clan>";
        if (!MercenaryDebugCommands.TryFindClan(MercenaryDebugCommands.JoinArguments(arguments.Skip(1)), out Clan clan, out error)) return error;
        MercenaryContractBehavior? behavior = Campaign.Current!.GetCampaignBehavior<MercenaryContractBehavior>();
        return behavior?.SetStrategicPriority(clan, priority) == true
            ? $"{clan.Name} priority={priority}." : $"{clan.Name} has no active contract.";
    }

    [CommandLineFunctionality.CommandLineArgumentFunction("mm_contract_term", "campaign")]
    public static string SetContractTerm(List<string> arguments)
    {
        if (!MercenaryDebugCommands.TryValidateCampaign(out string error)) return error;
        if (arguments.Count < 2) return "Usage: campaign.mm_contract_term <30|60|120|indefinite|war> <clan>";
        if (!MercenaryDebugCommands.TryFindClan(MercenaryDebugCommands.JoinArguments(arguments.Skip(1)), out Clan clan, out error)) return error;
        if (!TryGetContract(clan, out MercenaryContract contract, out error)) return error;
        string term = arguments[0].ToLowerInvariant();
        if (term == "indefinite") contract.SetDuration(MercenaryContractDuration.Indefinite, CampaignTime.Zero);
        else if (term == "war")
        {
            Kingdom? enemy = Kingdom.All.FirstOrDefault(candidate => candidate != clan.Kingdom
                && !candidate.IsEliminated && FactionManager.IsAtWarAgainstFaction(clan.Kingdom, candidate));
            if (enemy is null) return $"{clan.Kingdom?.Name} has no active war to bind the contract to.";
            contract.SetDuration(MercenaryContractDuration.UntilCurrentWarEnds, CampaignTime.Zero, enemy.StringId);
        }
        else if (int.TryParse(term, out int days) && (days == 30 || days == 60 || days == 120))
        {
            MercenaryContractDuration duration = days == 30 ? MercenaryContractDuration.Days30
                : days == 120 ? MercenaryContractDuration.Days120 : MercenaryContractDuration.Days60;
            contract.SetDuration(duration, CampaignTime.DaysFromNow(days));
        }
        else return "Usage: campaign.mm_contract_term <30|60|120|indefinite|war> <clan>";
        return FormatContract(clan, contract);
    }

    [CommandLineFunctionality.CommandLineArgumentFunction("mm_contract_renew", "campaign")]
    public static string Renew(List<string> arguments)
    {
        if (!MercenaryDebugCommands.TryValidateCampaign(out string error)) return error;
        if (arguments.Count < 2 || !float.TryParse(arguments[0], NumberStyles.Float, CultureInfo.InvariantCulture, out float days) || days <= 0)
            return "Usage: campaign.mm_contract_renew <positive days> <clan>";
        if (!MercenaryDebugCommands.TryFindClan(MercenaryDebugCommands.JoinArguments(arguments.Skip(1)), out Clan clan, out error)) return error;
        MercenaryContractBehavior? behavior = Campaign.Current!.GetCampaignBehavior<MercenaryContractBehavior>();
        return behavior?.SetContractEndTime(clan, CampaignTime.DaysFromNow(days)) == true
            ? $"{clan.Name} renewed for {days.ToString(CultureInfo.InvariantCulture)} days." : $"{clan.Name} has no active contract.";
    }

    [CommandLineFunctionality.CommandLineArgumentFunction("mm_contract_finish", "campaign")]
    public static string Finish(List<string> arguments)
    {
        if (!MercenaryDebugCommands.TryValidateCampaign(out string error)) return error;
        if (!MercenaryDebugCommands.TryFindClan(MercenaryDebugCommands.JoinArguments(arguments), out Clan clan, out error)) return error;
        MercenaryDismissalService.EndService(clan);
        return clan.IsUnderMercenaryService ? $"Native leave action did not complete for {clan.Name}." : $"{clan.Name} contract and service finished.";
    }

    [CommandLineFunctionality.CommandLineArgumentFunction("mm_contract_check", "campaign")]
    public static string ContractCheck(List<string> arguments)
    {
        if (!MercenaryDebugCommands.TryValidateCampaign(out string error)) return error;
        if (arguments.Count < 2 || (arguments[0] != "normal" && arguments[0] != "force_war_ended"))
            return "Usage: campaign.mm_contract_check <normal|force_war_ended> <clan>";
        if (!MercenaryDebugCommands.TryFindClan(MercenaryDebugCommands.JoinArguments(arguments.Skip(1)), out Clan clan, out error)) return error;
        MercenaryContractBehavior? behavior = Campaign.Current!.GetCampaignBehavior<MercenaryContractBehavior>();
        bool ended = behavior?.RunDiagnosticLifecycleCheck(clan, arguments[0] == "force_war_ended") == true;
        return ended ? $"Lifecycle check ended {clan.Name}'s contract." : "Lifecycle check completed; contract remains active.";
    }

    [CommandLineFunctionality.CommandLineArgumentFunction("mm_offer_services", "campaign")]
    public static string OfferServices(List<string> arguments)
    {
        if (!MercenaryDebugCommands.TryValidateCampaign(out string error)) return error;
        if (!MercenaryDebugCommands.TryFindKingdom(MercenaryDebugCommands.JoinArguments(arguments), out Kingdom kingdom, out error)) return error;
        MercenaryOfferBehavior? behavior = Campaign.Current!.GetCampaignBehavior<MercenaryOfferBehavior>();
        MercenaryOffer? offer = behavior?.RequestOffer(Clan.PlayerClan, kingdom);
        return offer is null ? $"{kingdom.Name} refused or is not eligible for a service request."
            : $"Saved offer: kingdom={kingdom.StringId}; rate={offer.InfluencePrice}; duration={offer.Duration}; priority={offer.StrategicPriority}; expires_in={offer.ExpirationTime.RemainingDaysFromNow:0.##} days.";
    }

    [CommandLineFunctionality.CommandLineArgumentFunction("mm_offers", "campaign")]
    public static string Offers(List<string> arguments)
    {
        if (!MercenaryDebugCommands.TryValidateCampaign(out string error)) return error;
        Clan clan = Clan.PlayerClan;
        if (arguments.Count > 0 && !MercenaryDebugCommands.TryFindClan(MercenaryDebugCommands.JoinArguments(arguments), out clan, out error)) return error;
        MercenaryOfferBehavior? behavior = Campaign.Current!.GetCampaignBehavior<MercenaryOfferBehavior>();
        MercenaryOffer[] offers = behavior?.GetSavedOffers().Where(item => item.ClanId == clan.StringId).ToArray() ?? Array.Empty<MercenaryOffer>();
        if (offers.Length == 0) return $"No saved offers for {clan.Name}.";
        return string.Join("\n", offers.Select(item => $"kingdom={item.KingdomId}; valid={!item.IsExpired}; rate={item.InfluencePrice}; duration={item.Duration}; priority={item.StrategicPriority}; expires={item.ExpirationTime.RemainingDaysFromNow:0.##}d"));
    }

    [CommandLineFunctionality.CommandLineArgumentFunction("mm_offer_accept", "campaign")]
    public static string AcceptOffer(List<string> arguments) => ChangeOffer(arguments, true);

    [CommandLineFunctionality.CommandLineArgumentFunction("mm_offer_reject", "campaign")]
    public static string RejectOffer(List<string> arguments) => ChangeOffer(arguments, false);

    [CommandLineFunctionality.CommandLineArgumentFunction("mm_ai_evaluate", "campaign")]
    public static string EvaluateAi(List<string> arguments)
    {
        if (!MercenaryDebugCommands.TryValidateCampaign(out string error)) return error;
        if (arguments.Count < 3 || (arguments[1] != "preview" && arguments[1] != "apply"))
            return "Usage: campaign.mm_ai_evaluate <departure|dismissal|renewal> <preview|apply> <clan>";
        if (!MercenaryDebugCommands.TryFindClan(MercenaryDebugCommands.JoinArguments(arguments.Skip(2)), out Clan clan, out error)) return error;
        if (!TryGetContract(clan, out MercenaryContract contract, out error)) return error;
        bool apply = arguments[1] == "apply";
        float departure = MercenaryEvaluationService.CalculateDepartureRisk(contract);
        float dismissal = MercenaryEvaluationService.CalculateEmployerTerminationRisk(contract);
        string action = arguments[0].ToLowerInvariant();
        bool recommended;
        if (action == "departure") recommended = departure > 70f && contract.ServiceStartTime.ElapsedDaysUntilNow >= 30f;
        else if (action == "dismissal") recommended = dismissal > 70f || contract.EmployerSatisfaction < 10f;
        else if (action == "renewal") recommended = departure < 60f && dismissal < 60f && contract.Satisfaction >= 40f && contract.Reliability >= 35f;
        else return "Usage: campaign.mm_ai_evaluate <departure|dismissal|renewal> <preview|apply> <clan>";
        if (apply && recommended)
        {
            if (action == "renewal") Campaign.Current!.GetCampaignBehavior<MercenaryContractBehavior>()?.SetContractEndTime(clan, CampaignTime.DaysFromNow(60f));
            else MercenaryDismissalService.EndService(clan);
        }
        return $"decision={action}; recommended={recommended}; applied={apply && recommended}; departure_risk={departure:0.##}; dismissal_risk={dismissal:0.##}; satisfaction={contract.Satisfaction:0.##}; employer={contract.EmployerSatisfaction:0.##}; reliability={contract.Reliability:0.##}";
    }

    [CommandLineFunctionality.CommandLineArgumentFunction("mm_ai_hire", "campaign")]
    public static string EvaluateAiHire(List<string> arguments)
    {
        if (!MercenaryDebugCommands.TryValidateCampaign(out string error)) return error;
        if (arguments.Count < 2 || (arguments[0] != "preview" && arguments[0] != "apply")) return "Usage: campaign.mm_ai_hire <preview|apply> <kingdom>";
        if (!MercenaryDebugCommands.TryFindKingdom(MercenaryDebugCommands.JoinArguments(arguments.Skip(1)), out Kingdom kingdom, out error)) return error;
        Clan? candidate = Clan.All.Where(clan => MercenaryHiringService.CanAiKingdomHire(clan, kingdom))
            .OrderByDescending(clan => clan.CurrentTotalStrength + (clan.Leader?.GetRelation(kingdom.Leader) ?? 0) * 5f).FirstOrDefault();
        float need = MercenaryHiringService.CalculateMilitaryNeed(kingdom);
        if (candidate is null) return $"kingdom={kingdom.StringId}; need={need:0.###}; candidate=none; decision=no_hire";
        bool recommended = need >= 0.35f;
        if (recommended && arguments[0] == "apply") MercenaryHiringService.Hire(candidate, kingdom);
        return $"kingdom={kingdom.StringId}; need={need:0.###}; candidate={candidate.StringId}; recommended={recommended}; applied={recommended && arguments[0] == "apply"}";
    }

    [CommandLineFunctionality.CommandLineArgumentFunction("mm_party_state", "campaign")]
    public static string PartyState(List<string> arguments)
    {
        if (!MercenaryDebugCommands.TryValidateCampaign(out string error)) return error;
        if (!MercenaryDebugCommands.TryFindClan(MercenaryDebugCommands.JoinArguments(arguments), out Clan clan, out error)) return error;
        StringBuilder result = new StringBuilder($"Parties for {clan.Name}; passive contribution is capped once per clan-day:");
        foreach (var component in clan.WarPartyComponents)
        {
            MobileParty party = component.MobileParty;
            result.AppendLine().Append("id=").Append(party.StringId)
                .Append("; active=").Append(party.IsActive)
                .Append("; behavior=").Append(party.DefaultBehavior)
                .Append("; army=").Append(party.Army?.Name?.ToString() ?? "none")
                .Append("; size_ratio=").Append(party.PartySizeRatio.ToString("0.###", CultureInfo.InvariantCulture))
                .Append("; food_days=").Append(party.GetNumDaysForFoodToLast().ToString("0.##", CultureInfo.InvariantCulture));
        }
        return result.ToString();
    }

    [CommandLineFunctionality.CommandLineArgumentFunction("mm_contribution_probe", "campaign")]
    public static string ContributionProbe(List<string> arguments)
    {
        if (!MercenaryDebugCommands.TryValidateCampaign(out string error)) return error;
        if (!MercenaryDebugCommands.TryFindClan(MercenaryDebugCommands.JoinArguments(arguments), out Clan clan, out error)) return error;
        if (!TryGetContract(clan, out MercenaryContract contract, out error)) return error;
        int eligibleParties = 0;
        StringBuilder result = new StringBuilder($"Contribution probe for {clan.Name}; priority={contract.StrategicPriority}:");
        foreach (var component in clan.WarPartyComponents)
        {
            MobileParty party = component.MobileParty;
            PassiveContributionEvidence evidence = MercenaryPriorityBehavior.GetPassiveContributionEvidence(party);
            bool eligible = party.IsActive && MercenaryContributionPolicy.IsEligible(contract.StrategicPriority, evidence);
            if (eligible) eligibleParties++;
            result.AppendLine().Append("party=").Append(party.StringId)
                .Append("; evidence=").Append(evidence)
                .Append("; eligible=").Append(eligible);
        }
        bool dailyAvailable = MercenaryContributionPolicy.CanRecordDaily(
            contract.LastDailyContributionTime == CampaignTime.Zero
                ? -1f : contract.LastDailyContributionTime.ElapsedDaysUntilNow);
        result.AppendLine().Append("eligible_parties=").Append(eligibleParties)
            .Append("; daily_slot_available=").Append(dailyAvailable)
            .Append("; resulting_passive_points_now=")
            .Append(eligibleParties > 0 && dailyAvailable ? 1 : 0)
            .Append(" (clan-wide cap)");
        return result.ToString();
    }

    [CommandLineFunctionality.CommandLineArgumentFunction("mm_harmony_status", "campaign")]
    public static string HarmonyStatus(List<string> arguments)
    {
        if (arguments.Count != 0) return "Usage: campaign.mm_harmony_status";
        MethodBase[] methods = Harmony.GetAllPatchedMethods().Where(method =>
            Harmony.GetPatchInfo(method)?.Owners.Any(owner => owner.StartsWith("MercenaryManagement", StringComparison.Ordinal)) == true).ToArray();
        if (methods.Length == 0) return "No applied MercenaryManagement Harmony patches were found.";
        return "Applied MercenaryManagement patches:\n" + string.Join("\n", methods.Select(method =>
            $"{method.DeclaringType?.FullName}.{method.Name}; owners={string.Join(",", Harmony.GetPatchInfo(method)!.Owners)}"));
    }

    private static string ChangeOffer(List<string> arguments, bool accept)
    {
        if (!MercenaryDebugCommands.TryValidateCampaign(out string error)) return error;
        if (!MercenaryDebugCommands.TryFindKingdom(MercenaryDebugCommands.JoinArguments(arguments), out Kingdom kingdom, out error)) return error;
        MercenaryOfferBehavior? behavior = Campaign.Current!.GetCampaignBehavior<MercenaryOfferBehavior>();
        MercenaryOffer? offer = behavior?.GetSavedOffers().FirstOrDefault(item => item.ClanId == Clan.PlayerClan.StringId && item.KingdomId == kingdom.StringId);
        if (offer is null) return $"No saved offer from {kingdom.Name} exists for the player clan.";
        bool changed = accept ? behavior!.TryAccept(offer) : behavior!.RejectOffer(Clan.PlayerClan, kingdom);
        return changed ? (accept ? $"Accepted {kingdom.Name}'s offer." : $"Rejected {kingdom.Name}'s offer.") : "Offer operation failed validation.";
    }

    private static bool TryContract(List<string> arguments, out Clan clan, out MercenaryContract contract, out string error)
    {
        clan = null!; contract = null!;
        if (!MercenaryDebugCommands.TryValidateCampaign(out error)) return false;
        if (!MercenaryDebugCommands.TryFindClan(MercenaryDebugCommands.JoinArguments(arguments), out clan, out error)) return false;
        return TryGetContract(clan, out contract, out error);
    }

    private static bool TryGetContract(Clan clan, out MercenaryContract contract, out string error)
    {
        MercenaryContractBehavior? behavior = Campaign.Current!.GetCampaignBehavior<MercenaryContractBehavior>();
        if (behavior?.GetOrCreateContract(clan, out contract) == true) { error = string.Empty; return true; }
        contract = null!; error = $"{clan.Name} has no active mercenary contract."; return false;
    }

    private static string FormatContract(Clan clan, MercenaryContract contract)
    {
        return $"clan={clan.StringId}; employer={contract.KingdomId}; state={contract.State}; duration={contract.Duration}; end={contract.EndTime}; priority={contract.StrategicPriority}\n"
            + $"rate={contract.InfluencePrice}; contribution={contract.ContributionPoints}; last_activity={contract.LastActivity}; last_activity_time={contract.LastActivityTime}; last_daily_contribution={contract.LastDailyContributionTime}; last_evaluation={contract.LastEvaluationTime}\n"
            + $"performance={MercenaryEvaluationService.GetPerformanceSummary(contract)}; mercenary_satisfaction={contract.Satisfaction:0.##}; employer_satisfaction={contract.EmployerSatisfaction:0.##}; reliability={contract.Reliability:0.##}\n"
            + FormatRisks(clan, contract);
    }

    private static string FormatRisks(Clan clan, MercenaryContract contract)
    {
        RiskBreakdown departure = MercenaryEvaluationService.GetDepartureRiskBreakdown(contract);
        RiskBreakdown dismissal = MercenaryEvaluationService.GetEmployerTerminationRiskBreakdown(contract);
        return $"departure={departure.Total:0.##}% [base={departure.Base:0.##}; satisfaction={departure.Satisfaction:0.##}; reliability={departure.Reliability:0.##}; rate={departure.Rate:0.##}; relation={departure.Relation:0.##}; losses={departure.Losses:0.##}; military_need={departure.MilitaryNeed:0.##}; priority={departure.PriorityMismatch:0.##}; term={departure.RemainingTerm:0.##}]\n"
            + $"employer_termination={dismissal.Total:0.##}% [base={dismissal.Base:0.##}; employer_satisfaction={dismissal.Satisfaction:0.##}; reliability={dismissal.Reliability:0.##}; rate_cost={dismissal.Rate:0.##}; military_need={dismissal.MilitaryNeed:0.##}]";
    }

    private static bool TryPercent(string value, out float result)
    {
        return float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out result)
               && result >= 0f && result <= 100f && !float.IsNaN(result) && !float.IsInfinity(result);
    }

    private static bool TryPriority(string value, out MercenaryStrategicPriority priority)
    {
        switch (value.ToLowerInvariant())
        {
            case "independent": priority = MercenaryStrategicPriority.IndependentOperations; return true;
            case "armies": priority = MercenaryStrategicPriority.JoinArmies; return true;
            case "economic": priority = MercenaryStrategicPriority.EconomicWarfare; return true;
            case "defense": priority = MercenaryStrategicPriority.TerritorialDefense; return true;
            default: priority = default; return false;
        }
    }
}
