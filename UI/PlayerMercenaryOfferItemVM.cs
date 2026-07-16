using System;
using System.Linq;
using Bannerlord.UIExtenderEx.Attributes;
using MercenaryManagement.Contracts;
using MercenaryManagement.Services;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core.ViewModelCollection.ImageIdentifiers;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace MercenaryManagement.UI;

internal sealed class PlayerMercenaryOfferItemVM : ViewModel
{
    private readonly Action<PlayerMercenaryOfferItemVM> _onSelect;
    private bool _isSelected;

    public PlayerMercenaryOfferItemVM(Clan clan, Kingdom kingdom, Action<PlayerMercenaryOfferItemVM> onSelect)
    {
        _onSelect = onSelect;
        Banner = new BannerImageIdentifierVM(kingdom.Banner, true);
        KingdomName = kingdom.Name.ToString();
        RulerName = kingdom.Leader?.Name.ToString() ?? string.Empty;
        InfluencePrice = MercenaryHiringService.CalculateHirePrice(clan, kingdom);
        RelationToRuler = clan.Leader is not null && kingdom.Leader is not null
            ? clan.Leader.GetRelation(kingdom.Leader)
            : 0;

        MercenaryStrategicPriority priority = MercenaryHiringService.GetSuggestedPriority(kingdom);
        StrategicPriority = MercenaryPriorityText.GetName(priority).ToString();
        StrategicPriorityDescription = MercenaryPriorityText.GetDescription(priority).ToString();
        CurrentWars = FormatWars(kingdom);
    }

    [DataSourceProperty] public BannerImageIdentifierVM Banner { get; }
    [DataSourceProperty] public string KingdomName { get; }
    [DataSourceProperty] public string RulerName { get; }
    [DataSourceProperty] public string CurrentWars { get; }
    [DataSourceProperty] public int InfluencePrice { get; }
    [DataSourceProperty] public string StrategicPriority { get; }
    [DataSourceProperty] public string StrategicPriorityDescription { get; }
    [DataSourceProperty] public int RelationToRuler { get; }
    [DataSourceProperty] public string CurrentWarsText => Localize("MercenaryManagement_CurrentWars", "Current wars");
    [DataSourceProperty] public string InfluencePriceText => Localize("MercenaryManagement_InfluencePrice", "Dinars per influence");
    [DataSourceProperty] public string RelationToRulerText => Localize("MercenaryManagement_RelationToRuler", "Relation to ruler");
    [DataSourceProperty] public string RulerText => Localize("MercenaryManagement_Ruler", "Ruler");
    [DataSourceProperty]
    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected == value) return;
            _isSelected = value;
            OnPropertyChangedWithValue(value);
        }
    }

    [DataSourceMethod]
    public void ExecuteSelect() => _onSelect(this);

    public override void OnFinalize()
    {
        Banner.OnFinalize();
        base.OnFinalize();
    }

    private static string FormatWars(Kingdom kingdom)
    {
        string[] enemies = Kingdom.All
            .Where(other => !other.IsEliminated
                            && other != kingdom
                            && FactionManager.IsAtWarAgainstFaction(kingdom, other))
            .OrderBy(other => other.Name.ToString())
            .Select(other => other.Name.ToString())
            .ToArray();
        return enemies.Length == 0
            ? new TextObject("{=MercenaryManagement_NoCurrentWars}No current wars").ToString()
            : string.Join(", ", enemies);
    }

    private static string Localize(string id, string fallback) =>
        new TextObject($"{{={id}}}{fallback}").ToString();
}
