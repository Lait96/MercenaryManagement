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

    public PlayerMercenaryOfferItemVM(MercenaryOffer offer, Action<PlayerMercenaryOfferItemVM> onSelect)
    {
        Clan clan = offer.Clan!;
        Kingdom kingdom = offer.Kingdom!;
        Offer = offer;
        Kingdom = kingdom;
        _onSelect = onSelect;
        Banner = new BannerImageIdentifierVM(kingdom.Banner, true);
        KingdomName = kingdom.Name.ToString();
        RulerName = kingdom.Leader?.Name.ToString() ?? string.Empty;
        InfluencePrice = offer.InfluencePrice;
        RelationToRuler = clan.Leader is not null && kingdom.Leader is not null
            ? clan.Leader.GetRelation(kingdom.Leader)
            : 0;

        MercenaryStrategicPriority priority = offer.StrategicPriority;
        StrategicPriority = MercenaryPriorityText.GetName(priority).ToString();
        StrategicPriorityDescription = MercenaryPriorityText.GetDescription(priority).ToString();
        CurrentWars = FormatWars(kingdom);
        ExpectedDuration = FormatDuration(offer.Duration);
        EstimatedDanger = GetDanger(kingdom);
        InterestReason = GetInterestReason(kingdom, priority);
    }

    internal Kingdom Kingdom { get; }
    internal MercenaryOffer Offer { get; }
    internal MercenaryStrategicPriority Priority => Offer.StrategicPriority;

    [DataSourceProperty] public BannerImageIdentifierVM Banner { get; }
    [DataSourceProperty] public string KingdomName { get; }
    [DataSourceProperty] public string RulerName { get; }
    [DataSourceProperty] public string CurrentWars { get; }
    [DataSourceProperty] public int InfluencePrice { get; }
    [DataSourceProperty] public string StrategicPriority { get; }
    [DataSourceProperty] public string StrategicPriorityDescription { get; }
    [DataSourceProperty] public int RelationToRuler { get; }
    [DataSourceProperty] public string ExpectedDuration { get; }
    [DataSourceProperty] public string EstimatedDanger { get; }
    [DataSourceProperty] public string InterestReason { get; }
    [DataSourceProperty] public string CurrentWarsText => Localize("MercenaryManagement_CurrentWars", "Current wars");
    [DataSourceProperty] public string InfluencePriceText => Localize("MercenaryManagement_InfluencePrice", "Dinars per influence");
    [DataSourceProperty] public string RelationToRulerText => Localize("MercenaryManagement_RelationToRuler", "Relation to ruler");
    [DataSourceProperty] public string RulerText => Localize("MercenaryManagement_Ruler", "Ruler");
    [DataSourceProperty] public string DurationText => Localize("MercenaryManagement_Duration", "Duration");
    [DataSourceProperty] public string DangerText => Localize("MercenaryManagement_Danger", "Estimated danger");
    [DataSourceProperty] public string InterestText => Localize("MercenaryManagement_Interest", "Why this kingdom is interested");
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

    private static string GetDanger(Kingdom kingdom)
    {
        float need = MercenaryHiringService.CalculateMilitaryNeed(kingdom);
        return need >= 0.8f ? Localize("MercenaryManagement_DangerHigh", "High")
            : need >= 0.4f ? Localize("MercenaryManagement_DangerMedium", "Moderate")
            : Localize("MercenaryManagement_DangerLow", "Low");
    }

    private static string GetInterestReason(Kingdom kingdom, MercenaryStrategicPriority priority)
    {
        if (MercenaryHiringService.CalculateMilitaryNeed(kingdom) <= 0f)
            return Localize("MercenaryManagement_InterestReserve", "The kingdom wants flexible forces available for emerging threats.");
        return priority == MercenaryStrategicPriority.TerritorialDefense
            ? Localize("MercenaryManagement_InterestDefense", "The kingdom needs help protecting its territory.")
            : Localize("MercenaryManagement_InterestWar", "The kingdom needs additional strength for its current wars.");
    }

    private static string FormatDuration(MercenaryContractDuration duration)
    {
        switch (duration)
        {
            case MercenaryContractDuration.Days30: return Localize("MercenaryManagement_Duration30", "30 days");
            case MercenaryContractDuration.Days120: return Localize("MercenaryManagement_Duration120", "120 days");
            case MercenaryContractDuration.UntilCurrentWarEnds: return Localize("MercenaryManagement_DurationWar", "Until the current war ends");
            case MercenaryContractDuration.Indefinite: return Localize("MercenaryManagement_Indefinite", "Indefinite");
            default: return Localize("MercenaryManagement_Duration60", "60 days");
        }
    }

    private static string Localize(string id, string fallback) =>
        new TextObject($"{{={id}}}{fallback}").ToString();
}
