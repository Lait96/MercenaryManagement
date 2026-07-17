using TaleWorlds.CampaignSystem;
using TaleWorlds.SaveSystem;

namespace MercenaryManagement.Contracts;

internal sealed class MercenaryOffer
{
    public MercenaryOffer(
        Clan clan,
        Kingdom kingdom,
        int influencePrice,
        MercenaryStrategicPriority priority,
        MercenaryContractDuration duration,
        CampaignTime expirationTime)
    {
        ClanId = clan.StringId;
        KingdomId = kingdom.StringId;
        Clan = clan;
        Kingdom = kingdom;
        InfluencePrice = influencePrice;
        StrategicPriority = priority;
        Duration = duration;
        CreatedTime = CampaignTime.Now;
        ExpirationTime = expirationTime;
    }

    [SaveableProperty(1)] public string ClanId { get; private set; }
    [SaveableProperty(2)] public string KingdomId { get; private set; }
    [SaveableProperty(3)] public Clan? Clan { get; private set; }
    [SaveableProperty(4)] public Kingdom? Kingdom { get; private set; }
    [SaveableProperty(5)] public int InfluencePrice { get; private set; }
    [SaveableProperty(6)] public MercenaryStrategicPriority StrategicPriority { get; private set; }
    [SaveableProperty(7)] public MercenaryContractDuration Duration { get; private set; }
    [SaveableProperty(8)] public CampaignTime CreatedTime { get; private set; }
    [SaveableProperty(9)] public CampaignTime ExpirationTime { get; private set; }

    public bool IsExpired => ExpirationTime != CampaignTime.Zero && ExpirationTime.IsPast;

    public void AttachParticipants(Clan clan, Kingdom kingdom)
    {
        Clan = clan;
        Kingdom = kingdom;
        ClanId = clan.StringId;
        KingdomId = kingdom.StringId;
    }
}
