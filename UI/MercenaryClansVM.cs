using System;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core.ViewModelCollection.ImageIdentifiers;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace MercenaryManagement.UI;

internal sealed class MercenaryClansVM : ViewModel
{
    private bool _hasClans;

    public MercenaryClansVM()
    {
        Clans = new MBBindingList<MercenaryClanItemVM>();
    }

    [DataSourceProperty]
    public MBBindingList<MercenaryClanItemVM> Clans { get; }

    [DataSourceProperty]
    public bool HasClans
    {
        get => _hasClans;
        private set
        {
            if (_hasClans == value)
            {
                return;
            }

            _hasClans = value;
            OnPropertyChangedWithValue(value);
            OnPropertyChangedWithValue(!value, nameof(IsEmpty));
        }
    }

    [DataSourceProperty]
    public bool IsEmpty => !HasClans;

    [DataSourceProperty]
    public string TitleText => new TextObject("{=MercenaryManagement_MercenaryClans}Mercenary clans").ToString();

    [DataSourceProperty]
    public string ClanText => new TextObject("{=MercenaryManagement_Clan}Clan").ToString();

    [DataSourceProperty]
    public string LeaderText => new TextObject("{=MercenaryManagement_Leader}Leader").ToString();

    [DataSourceProperty]
    public string StrengthText => new TextObject("{=MercenaryManagement_Strength}Strength").ToString();

    [DataSourceProperty]
    public string ActivePartiesText => new TextObject("{=MercenaryManagement_ActiveParties}Active parties").ToString();

    [DataSourceProperty]
    public string EmptyStateText => new TextObject(
        "{=MercenaryManagement_NoMercenaryClans}No mercenary clans are currently serving this kingdom.").ToString();

    public void Refresh(Kingdom? kingdom)
    {
        Clans.Clear();

        if (kingdom is not null)
        {
            foreach (Clan clan in kingdom.Clans
                         .Where(clan => clan.IsUnderMercenaryService)
                         .OrderBy(clan => clan.Name.ToString()))
            {
                Clans.Add(new MercenaryClanItemVM(clan));
            }
        }

        HasClans = Clans.Count > 0;
    }
}

internal sealed class MercenaryClanItemVM : ViewModel
{
    public MercenaryClanItemVM(Clan clan)
    {
        Banner = new BannerImageIdentifierVM(clan.Banner, true);
        ClanName = clan.Name.ToString();
        LeaderName = clan.Leader?.Name.ToString() ?? string.Empty;
        Strength = (int)Math.Round(clan.CurrentTotalStrength);
        ActiveParties = clan.WarPartyComponents.Count(component => component.MobileParty.IsActive);
    }

    [DataSourceProperty]
    public BannerImageIdentifierVM Banner { get; }

    [DataSourceProperty]
    public string ClanName { get; }

    [DataSourceProperty]
    public string LeaderName { get; }

    [DataSourceProperty]
    public int Strength { get; }

    [DataSourceProperty]
    public int ActiveParties { get; }
}
