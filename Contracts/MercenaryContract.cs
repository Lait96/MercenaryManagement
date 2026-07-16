using TaleWorlds.SaveSystem;

namespace MercenaryManagement.Contracts;

internal sealed class MercenaryContract
{
    public MercenaryContract(string clanId, string kingdomId, int influencePrice)
    {
        ClanId = clanId;
        KingdomId = kingdomId;
        InfluencePrice = influencePrice;
    }

    [SaveableProperty(1)]
    public string ClanId { get; private set; }

    [SaveableProperty(2)]
    public string KingdomId { get; private set; }

    [SaveableProperty(3)]
    public int InfluencePrice { get; private set; }

    public bool Matches(string clanId, string kingdomId)
    {
        return ClanId == clanId && KingdomId == kingdomId;
    }

    public void SetInfluencePrice(int influencePrice)
    {
        InfluencePrice = influencePrice;
    }
}
