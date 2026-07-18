using TaleWorlds.CampaignSystem;
using TaleWorlds.SaveSystem;

namespace MercenaryManagement.Contracts;

internal sealed class MercenaryContractHistoryEntry
{
    public MercenaryContractHistoryEntry(MercenaryContract contract,
        MercenaryContractEndReason endReason, CampaignTime endTime, int finalRelation)
    {
        ClanId = contract.ClanId;
        KingdomId = contract.KingdomId;
        EmployerName = contract.EmployerKingdom?.Name.ToString() ?? contract.KingdomId;
        RulerName = contract.EmployerKingdom?.Leader?.Name.ToString() ?? string.Empty;
        ServiceStartTime = contract.ServiceStartTime;
        ServiceEndTime = endTime;
        EndReason = endReason;
        InfluencePrice = contract.InfluencePrice;
        ContributionPoints = contract.ContributionPoints;
        MercenarySatisfaction = contract.Satisfaction;
        EmployerSatisfaction = contract.EmployerSatisfaction;
        Reliability = contract.Reliability;
        FinalRelation = finalRelation;
    }

    [SaveableProperty(1)] public string ClanId { get; private set; }
    [SaveableProperty(2)] public string KingdomId { get; private set; }
    [SaveableProperty(3)] public string EmployerName { get; private set; }
    [SaveableProperty(4)] public string RulerName { get; private set; }
    [SaveableProperty(5)] public CampaignTime ServiceStartTime { get; private set; }
    [SaveableProperty(6)] public CampaignTime ServiceEndTime { get; private set; }
    [SaveableProperty(7)] public MercenaryContractEndReason EndReason { get; private set; }
    [SaveableProperty(8)] public int InfluencePrice { get; private set; }
    [SaveableProperty(9)] public int ContributionPoints { get; private set; }
    [SaveableProperty(10)] public float MercenarySatisfaction { get; private set; }
    [SaveableProperty(11)] public float EmployerSatisfaction { get; private set; }
    [SaveableProperty(12)] public float Reliability { get; private set; }
    [SaveableProperty(13)] public int FinalRelation { get; private set; }
}
