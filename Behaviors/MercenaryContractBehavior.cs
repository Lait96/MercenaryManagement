using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;

namespace MercenaryManagement.Behaviors;

internal sealed class MercenaryContractBehavior : CampaignBehaviorBase
{
    private Dictionary<string, int> _influencePrices = new Dictionary<string, int>();

    public override void RegisterEvents()
    {
        CampaignEvents.OnClanChangedKingdomEvent.AddNonSerializedListener(
            this,
            OnClanChangedKingdom);
    }

    public override void SyncData(IDataStore dataStore)
    {
        dataStore.SyncData("MercenaryManagement_InfluencePrices", ref _influencePrices);
    }

    public bool TryGetInfluencePrice(Clan clan, out int influencePrice)
    {
        return _influencePrices.TryGetValue(clan.StringId, out influencePrice);
    }

    public void SetInfluencePrice(Clan clan, int influencePrice)
    {
        _influencePrices[clan.StringId] = influencePrice;
    }

    private void OnClanChangedKingdom(
        Clan clan,
        Kingdom oldKingdom,
        Kingdom newKingdom,
        ChangeKingdomAction.ChangeKingdomActionDetail detail,
        bool showNotification)
    {
        if (oldKingdom == newKingdom)
        {
            return;
        }

        _influencePrices.Remove(clan.StringId);
    }
}
