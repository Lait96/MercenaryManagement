using System;
using Bannerlord.UIExtenderEx.Attributes;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core.ViewModelCollection.ImageIdentifiers;
using TaleWorlds.Library;

namespace MercenaryManagement.UI;

internal sealed class PlayerServiceKingdomItemVM : ViewModel
{
    private readonly Action<PlayerServiceKingdomItemVM> _onSelect;
    private bool _isSelected;

    public PlayerServiceKingdomItemVM(
        Kingdom kingdom,
        Action<PlayerServiceKingdomItemVM> onSelect)
    {
        Kingdom = kingdom;
        _onSelect = onSelect;
        Banner = new BannerImageIdentifierVM(kingdom.Banner, true);
        KingdomName = kingdom.Name.ToString();
        RulerName = kingdom.Leader?.Name.ToString() ?? string.Empty;
        RelationToRuler = Clan.PlayerClan.Leader is not null && kingdom.Leader is not null
            ? Clan.PlayerClan.Leader.GetRelation(kingdom.Leader)
            : 0;
    }

    internal Kingdom Kingdom { get; }

    [DataSourceProperty] public BannerImageIdentifierVM Banner { get; }
    [DataSourceProperty] public string KingdomName { get; }
    [DataSourceProperty] public string RulerName { get; }
    [DataSourceProperty] public int RelationToRuler { get; }

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
}
