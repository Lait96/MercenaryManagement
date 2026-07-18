using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using MercenaryManagement.Behaviors;
using MercenaryManagement.Contracts;
using MercenaryManagement.Services;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;

namespace MercenaryManagement.Commands;

public static class MercenaryHistoryTestCommands
{
    [CommandLineFunctionality.CommandLineArgumentFunction("mm_contract_history", "campaign")]
    public static string ContractHistory(List<string> arguments)
    {
        if (!TryGetBehaviorAndClan(arguments, out MercenaryContractBehavior behavior,
                out Clan clan, out string error)) return error;

        IReadOnlyList<MercenaryContractHistoryEntry> history = behavior.GetHistory(clan);
        if (history.Count == 0)
            return $"No completed contracts recorded for {clan.Name} ({clan.StringId}); reputation={behavior.GetReputation(clan, null):0.##}.";

        StringBuilder output = new StringBuilder()
            .Append("Contract history for ").Append(clan.Name)
            .Append(" (").Append(clan.StringId).Append(")")
            .Append("; entries=").Append(history.Count)
            .Append("; general_reputation=")
            .Append(behavior.GetReputation(clan, null).ToString("0.##", CultureInfo.InvariantCulture));
        foreach (MercenaryContractHistoryEntry entry in history.OrderByDescending(item => item.ServiceEndTime.ToDays))
        {
            output.AppendLine().Append("kingdom=").Append(entry.KingdomId)
                .Append("; employer=").Append(entry.EmployerName)
                .Append("; ruler=").Append(entry.RulerName)
                .Append("; start=").Append(entry.ServiceStartTime)
                .Append("; end=").Append(entry.ServiceEndTime)
                .Append("; reason=").Append(entry.EndReason)
                .Append("; contribution=").Append(entry.ContributionPoints)
                .Append("; mercenary_satisfaction=").Append(Format(entry.MercenarySatisfaction))
                .Append("; employer_satisfaction=").Append(Format(entry.EmployerSatisfaction))
                .Append("; reliability=").Append(Format(entry.Reliability))
                .Append("; relation=").Append(entry.FinalRelation);
        }
        return output.ToString();
    }

    [CommandLineFunctionality.CommandLineArgumentFunction("mm_contract_reputation", "campaign")]
    public static string ContractReputation(List<string> arguments)
    {
        if (!MercenaryDebugCommands.TryValidateCampaign(out string error)) return error;
        int separator = arguments.IndexOf("|");
        IEnumerable<string> clanArguments = separator < 0 ? arguments : arguments.Take(separator);
        if (!MercenaryDebugCommands.TryFindClan(
                MercenaryDebugCommands.JoinArguments(clanArguments), out Clan clan, out error)) return error;
        Kingdom? kingdom = null;
        if (separator >= 0
            && !MercenaryDebugCommands.TryFindKingdom(
                MercenaryDebugCommands.JoinArguments(arguments.Skip(separator + 1)),
                out kingdom, out error)) return error;

        MercenaryContractBehavior? behavior = Campaign.Current!
            .GetCampaignBehavior<MercenaryContractBehavior>();
        if (behavior is null) return "MercenaryContractBehavior is not registered.";
        float general = behavior.GetReputation(clan, null);
        float negotiated = behavior.GetReputation(clan, kingdom);
        return $"clan={clan.StringId}; entries={behavior.GetHistory(clan).Count}; general={Format(general)}"
               + (kingdom is null
                   ? string.Empty
                   : $"; kingdom={kingdom.StringId}; negotiation_reputation={Format(negotiated)}");
    }

    [CommandLineFunctionality.CommandLineArgumentFunction("mm_history_test", "campaign")]
    public static string RunHistoryTest(List<string> arguments)
    {
        if (!MercenaryDebugCommands.TryValidateCampaign(out string error)) return error;
        if (arguments.Count < 2 || !string.Equals(arguments[arguments.Count - 1], "confirm",
                StringComparison.OrdinalIgnoreCase))
            return "Usage: campaign.mm_history_test <active mercenary clan> confirm. WARNING: ends the current service; use a disposable test save.";
        if (!MercenaryDebugCommands.TryFindClan(
                MercenaryDebugCommands.JoinArguments(arguments.Take(arguments.Count - 1)),
                out Clan clan, out error)) return error;

        MercenaryContractBehavior? behavior = Campaign.Current!
            .GetCampaignBehavior<MercenaryContractBehavior>();
        if (behavior is null) return "FAIL | MercenaryContractBehavior is not registered.";
        if (!behavior.TryGetContract(clan, out MercenaryContract contract))
            return $"FAIL | {clan.Name} has no active contract. Hire it first, then repeat the command.";

        Kingdom? employer = contract.EmployerKingdom;
        if (employer is null)
            return "FAIL | active contract has no employer object; native API test cannot continue.";

        int historyBefore = behavior.GetHistory(clan).Count;
        float reputationBefore = behavior.GetReputation(clan, employer);
        int relationBefore = clan.Leader is not null && employer.Leader is not null
            ? clan.Leader.GetRelation(employer.Leader) : 0;

        // Deterministic state makes the resulting reputation observable while the
        // actual exit still travels through Bannerlord's native kingdom action/event.
        contract.SetEvaluation(82f, 88f, CampaignTime.Now);
        contract.SetEmployerSatisfaction(86f);
        contract.RecordContribution(75, "Diagnostic:contract history API test");
        int expectedContribution = contract.ContributionPoints;

        MercenaryDismissalService.EndService(clan, MercenaryContractEndReason.MercenaryLeft);
        IReadOnlyList<MercenaryContractHistoryEntry> history = behavior.GetHistory(clan);
        MercenaryContractHistoryEntry? entry = history.Count > historyBefore
            ? history[history.Count - 1] : null;

        TestReport report = new TestReport();
        report.Check(!clan.IsUnderMercenaryService && clan.Kingdom is null,
            "native ChangeKingdomAction removed mercenary service");
        report.Check(history.Count == historyBefore + 1,
            "native OnClanChangedKingdom event created exactly one history entry");
        report.Check(entry is not null && entry.ClanId == clan.StringId
                    && entry.KingdomId == employer.StringId,
            "history snapshot retained both API participants after kingdom detachment");
        report.Check(entry?.EndReason == MercenaryContractEndReason.MercenaryLeft,
            "pending completion reason survived the native API callback");
        report.Check(entry?.ContributionPoints == expectedContribution
                    && Math.Abs((entry?.EmployerSatisfaction ?? 0f) - 86f) < 0.01f
                    && Math.Abs((entry?.Reliability ?? 0f) - 88f) < 0.01f,
            "final performance and both-side evaluation were snapshotted");
        report.Check(entry?.FinalRelation == relationBefore,
            "final game relation was captured before participant references changed");

        int countBeforeDuplicateProbe = history.Count;
        bool duplicateCompleted = behavior.CompleteContract(clan,
            MercenaryContractEndReason.EmployerDismissed);
        report.Check(!duplicateCompleted && behavior.GetHistory(clan).Count == countBeforeDuplicateProbe,
            "repeated completion is idempotent and cannot duplicate history");

        float reputationAfter = behavior.GetReputation(clan, employer);
        report.Check(IsPercentage(reputationAfter)
                    && Math.Abs(reputationAfter - reputationBefore) > 0.001f,
            "new history changes bounded reputation used in repeat negotiations");
        report.Check(entry is not null && entry.ServiceEndTime != CampaignTime.Zero
                    && entry.ServiceEndTime.ToDays >= entry.ServiceStartTime.ToDays,
            "service dates from CampaignTime API form a valid completed interval");

        return report.Build(clan, employer, historyBefore, reputationBefore, reputationAfter);
    }

    private static bool TryGetBehaviorAndClan(IEnumerable<string> arguments,
        out MercenaryContractBehavior behavior, out Clan clan, out string error)
    {
        behavior = null!;
        clan = null!;
        if (!MercenaryDebugCommands.TryValidateCampaign(out error)) return false;
        if (!MercenaryDebugCommands.TryFindClan(
                MercenaryDebugCommands.JoinArguments(arguments), out clan, out error)) return false;
        behavior = Campaign.Current!.GetCampaignBehavior<MercenaryContractBehavior>();
        if (behavior is not null) return true;
        error = "MercenaryContractBehavior is not registered.";
        return false;
    }

    private static bool IsPercentage(float value) => !float.IsNaN(value)
        && !float.IsInfinity(value) && value >= 0f && value <= 100f;

    private static string Format(float value) =>
        value.ToString("0.##", CultureInfo.InvariantCulture);

    private sealed class TestReport
    {
        private readonly StringBuilder _output = new StringBuilder();
        private int _passed;
        private int _failed;

        public void Check(bool condition, string name)
        {
            if (condition) _passed++; else _failed++;
            _output.Append(condition ? "PASS | " : "FAIL | ").AppendLine(name);
        }

        public string Build(Clan clan, Kingdom employer, int historyBefore,
            float reputationBefore, float reputationAfter)
        {
            _output.Append("STATE | clan=").Append(clan.StringId)
                .Append("; former_employer=").Append(employer.StringId)
                .Append("; history_before=").Append(historyBefore)
                .Append("; reputation_before=").Append(Format(reputationBefore))
                .Append("; reputation_after=").Append(Format(reputationAfter)).AppendLine()
                .Append("SUMMARY | pass=").Append(_passed)
                .Append("; fail=").Append(_failed)
                .Append("; result=").Append(_failed == 0 ? "PASS" : "FAIL").AppendLine()
                .Append("IMPORTANT | discard this test save; the command intentionally ended service.");
            return _output.ToString();
        }
    }
}
