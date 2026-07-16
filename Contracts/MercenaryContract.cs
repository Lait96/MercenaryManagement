using TaleWorlds.CampaignSystem;
using TaleWorlds.SaveSystem;

namespace MercenaryManagement.Contracts;

internal sealed class MercenaryContract
{
    public MercenaryContract(
        Clan clan,
        Kingdom employerKingdom,
        int influencePrice,
        CampaignTime endTime)
    {
        Clan = clan;
        EmployerKingdom = employerKingdom;
        ClanId = clan.StringId;
        KingdomId = employerKingdom.StringId;
        InfluencePrice = influencePrice;
        ServiceStartTime = CampaignTime.Now;
        EndTime = endTime;
        State = MercenaryContractState.Active;
    }

    [SaveableProperty(1)]
    public string ClanId { get; private set; }

    [SaveableProperty(2)]
    public string KingdomId { get; private set; }

    [SaveableProperty(3)]
    public int InfluencePrice { get; private set; }

    [SaveableProperty(4)]
    public CampaignTime ServiceStartTime { get; private set; }

    [SaveableProperty(5)]
    public Clan? Clan { get; private set; }

    [SaveableProperty(6)]
    public Kingdom? EmployerKingdom { get; private set; }

    [SaveableProperty(7)]
    public CampaignTime EndTime { get; private set; }

    [SaveableProperty(8)]
    public MercenaryContractState State { get; private set; }

    public bool IsActive => State == MercenaryContractState.Active;

    public bool Matches(string clanId, string kingdomId)
    {
        return ClanId == clanId && KingdomId == kingdomId;
    }

    public void AttachParticipants(Clan clan, Kingdom employerKingdom)
    {
        Clan = clan;
        EmployerKingdom = employerKingdom;
        ClanId = clan.StringId;
        KingdomId = employerKingdom.StringId;
    }

    public void SetInfluencePrice(int influencePrice)
    {
        InfluencePrice = influencePrice;
    }

    public void SetServiceStartTime(CampaignTime serviceStartTime)
    {
        ServiceStartTime = serviceStartTime;
    }

    public void SetEndTime(CampaignTime endTime)
    {
        EndTime = endTime;
    }

    public void Complete(CampaignTime completionTime)
    {
        if (!IsActive)
        {
            return;
        }

        EndTime = completionTime;
        State = MercenaryContractState.Completed;
    }

    public void EnsureServiceStartTime()
    {
        if (ServiceStartTime == CampaignTime.Zero)
        {
            ServiceStartTime = CampaignTime.Now;
        }
    }
}
