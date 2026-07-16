using TaleWorlds.CampaignSystem;
using TaleWorlds.SaveSystem;

namespace MercenaryManagement.Contracts;

internal sealed class MercenaryContract
{
    public MercenaryContract(string clanId, string kingdomId, int influencePrice)
    {
        ClanId = clanId;
        KingdomId = kingdomId;
        InfluencePrice = influencePrice;
        ServiceStartTime = CampaignTime.Now;
    }

    [SaveableProperty(1)]
    public string ClanId { get; private set; }

    [SaveableProperty(2)]
    public string KingdomId { get; private set; }

    [SaveableProperty(3)]
    public int InfluencePrice { get; private set; }

    [SaveableProperty(4)]
    public CampaignTime ServiceStartTime { get; private set; }

    public bool Matches(string clanId, string kingdomId)
    {
        return ClanId == clanId && KingdomId == kingdomId;
    }

    public void SetInfluencePrice(int influencePrice)
    {
        InfluencePrice = influencePrice;
    }

    public void SetServiceStartTime(CampaignTime serviceStartTime)
    {
        ServiceStartTime = serviceStartTime;
    }

    public void EnsureServiceStartTime()
    {
        if (ServiceStartTime == CampaignTime.Zero)
        {
            ServiceStartTime = CampaignTime.Now;
        }
    }
}
