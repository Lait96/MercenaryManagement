using System;
using MercenaryManagement.Contracts;
using MercenaryManagement.Services;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace MercenaryManagement.UI;

internal sealed class MercenaryContractHistoryItemVM : ViewModel
{
    private readonly MercenaryContractHistoryEntry _entry;

    public MercenaryContractHistoryItemVM(MercenaryContractHistoryEntry entry)
    {
        _entry = entry;
    }

    [DataSourceProperty] public string EmployerName => _entry.EmployerName;
    [DataSourceProperty] public string RulerName => _entry.RulerName;
    [DataSourceProperty] public string EmployerSummary => FormatText(
        "MercenaryManagement_HistoryEmployerValue", "{EMPLOYER}\nRuler: {RULER}",
        ("EMPLOYER", _entry.EmployerName), ("RULER", _entry.RulerName));
    [DataSourceProperty] public string ServicePeriod => FormatText(
        "MercenaryManagement_HistoryPeriodValue", "Start: {START}\nEnd: {END}",
        ("START", _entry.ServiceStartTime.ToString()),
        ("END", _entry.ServiceEndTime.ToString()));
    [DataSourceProperty] public string EndReason => GetEndReason(_entry.EndReason).ToString();
    [DataSourceProperty] public int ContributionPoints => _entry.ContributionPoints;
    [DataSourceProperty] public int MercenarySatisfaction => (int)Math.Round(_entry.MercenarySatisfaction);
    [DataSourceProperty] public int EmployerSatisfaction => (int)Math.Round(_entry.EmployerSatisfaction);
    [DataSourceProperty] public int FinalRelation => _entry.FinalRelation;
    [DataSourceProperty] public int Reputation => (int)Math.Round(MercenaryReputationModel.Calculate(
        new ContractReputationInputs(_entry.Reliability, _entry.EmployerSatisfaction,
            _entry.ContributionPoints, _entry.FinalRelation,
            (float)Math.Max(0d, _entry.ServiceEndTime.ToDays - _entry.ServiceStartTime.ToDays))));
    [DataSourceProperty] public string ResultsSummary => FormatText(
        "MercenaryManagement_HistoryResultsValue",
        "Contribution: {CONTRIBUTION}\nReputation: {REPUTATION}",
        ("CONTRIBUTION", ContributionPoints.ToString()),
        ("REPUTATION", Reputation.ToString()));
    [DataSourceProperty] public string RelationsSummary => FormatText(
        "MercenaryManagement_HistoryRelationsValue",
        "Clan: {CLAN}\nEmployer: {EMPLOYER}\nRelation: {RELATION}",
        ("CLAN", MercenarySatisfaction.ToString()),
        ("EMPLOYER", EmployerSatisfaction.ToString()),
        ("RELATION", FinalRelation.ToString()));

    private static TextObject GetEndReason(MercenaryContractEndReason reason)
    {
        switch (reason)
        {
            case MercenaryContractEndReason.TermExpired:
                return new TextObject("{=MercenaryManagement_HistoryReasonExpired}Term completed");
            case MercenaryContractEndReason.WarEnded:
                return new TextObject("{=MercenaryManagement_HistoryReasonWarEnded}War ended");
            case MercenaryContractEndReason.EmployerDismissed:
                return new TextObject("{=MercenaryManagement_HistoryReasonDismissed}Dismissed by employer");
            case MercenaryContractEndReason.MercenaryLeft:
                return new TextObject("{=MercenaryManagement_HistoryReasonLeft}Mercenary clan left");
            case MercenaryContractEndReason.PartyUnavailable:
                return new TextObject("{=MercenaryManagement_HistoryReasonUnavailable}A party became unavailable");
            case MercenaryContractEndReason.ChangedKingdom:
                return new TextObject("{=MercenaryManagement_HistoryReasonChangedKingdom}Employer changed");
            default:
                return new TextObject("{=MercenaryManagement_HistoryReasonUnknown}Reason not recorded");
        }
    }

    private static string FormatText(string id, string fallback,
        params (string Name, string Value)[] variables)
    {
        TextObject text = new TextObject($"{{={id}}}{fallback}");
        foreach ((string name, string value) in variables)
            text.SetTextVariable(name, value);
        return text.ToString();
    }
}
