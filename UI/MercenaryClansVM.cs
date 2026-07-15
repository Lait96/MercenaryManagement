using System;
using System.Linq;
using MercenaryManagement.Services;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core.ViewModelCollection.ImageIdentifiers;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace MercenaryManagement.UI;

internal sealed class MercenaryClansVM : ViewModel
{
    private bool _hasClans;
    private MercenaryClanItemVM? _selectedClan;

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
    public MercenaryClanItemVM? SelectedClan
    {
        get => _selectedClan;
        private set
        {
            if (_selectedClan == value)
            {
                return;
            }

            _selectedClan = value;
            OnPropertyChangedWithValue(value);
        }
    }

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
    public string ClanLevelText => new TextObject("{=MercenaryManagement_ClanLevel}Clan level").ToString();

    [DataSourceProperty]
    public string ClanMembersText => new TextObject("{=MercenaryManagement_ClanMembers}Clan members").ToString();

    [DataSourceProperty]
    public string RelationToRulerText => new TextObject(
        "{=MercenaryManagement_RelationToRuler}Leader's relation to the ruler").ToString();

    [DataSourceProperty]
    public string InfluencePriceText => new TextObject(
        "{=MercenaryManagement_InfluencePrice}Denars per influence").ToString();

    [DataSourceProperty]
    public string AccumulatedInfluenceText => new TextObject(
        "{=MercenaryManagement_AccumulatedInfluence}Accumulated influence").ToString();

    [DataSourceProperty]
    public string ExpectedPaymentText => new TextObject(
        "{=MercenaryManagement_ExpectedPayment}Expected payment (denars)").ToString();

    [DataSourceProperty]
    public string ServiceStatusText => new TextObject(
        "{=MercenaryManagement_ServiceStatus}Service status").ToString();

    [DataSourceProperty]
    public string EmptyStateText => new TextObject(
        "{=MercenaryManagement_NoMercenaryClans}No mercenary clans are currently serving this kingdom.").ToString();

    public override void RefreshValues()
    {
        base.RefreshValues();

        OnPropertyChangedWithValue(TitleText, nameof(TitleText));
        OnPropertyChangedWithValue(ClanText, nameof(ClanText));
        OnPropertyChangedWithValue(LeaderText, nameof(LeaderText));
        OnPropertyChangedWithValue(StrengthText, nameof(StrengthText));
        OnPropertyChangedWithValue(ActivePartiesText, nameof(ActivePartiesText));
        OnPropertyChangedWithValue(ClanLevelText, nameof(ClanLevelText));
        OnPropertyChangedWithValue(ClanMembersText, nameof(ClanMembersText));
        OnPropertyChangedWithValue(RelationToRulerText, nameof(RelationToRulerText));
        OnPropertyChangedWithValue(InfluencePriceText, nameof(InfluencePriceText));
        OnPropertyChangedWithValue(AccumulatedInfluenceText, nameof(AccumulatedInfluenceText));
        OnPropertyChangedWithValue(ExpectedPaymentText, nameof(ExpectedPaymentText));
        OnPropertyChangedWithValue(ServiceStatusText, nameof(ServiceStatusText));
        OnPropertyChangedWithValue(EmptyStateText, nameof(EmptyStateText));

        foreach (MercenaryClanItemVM clan in Clans)
        {
            clan.RefreshValues();
        }
    }

    public void Refresh(Kingdom? kingdom)
    {
        SelectedClan = null;

        foreach (MercenaryClanItemVM clan in Clans)
        {
            clan.OnFinalize();
        }

        Clans.Clear();

        if (kingdom is not null)
        {
            foreach (Clan clan in kingdom.Clans
                         .Where(clan => clan.IsUnderMercenaryService)
                         .OrderBy(clan => clan.Name.ToString()))
            {
                Clans.Add(new MercenaryClanItemVM(clan, kingdom, SelectClan));
            }
        }

        HasClans = Clans.Count > 0;

        if (HasClans)
        {
            SelectClan(Clans[0]);
        }
    }

    public override void OnFinalize()
    {
        SelectedClan = null;

        foreach (MercenaryClanItemVM clan in Clans)
        {
            clan.OnFinalize();
        }

        Clans.Clear();
        base.OnFinalize();
    }

    private void SelectClan(MercenaryClanItemVM clan)
    {
        if (SelectedClan == clan)
        {
            return;
        }

        if (SelectedClan is not null)
        {
            SelectedClan.IsSelected = false;
        }

        SelectedClan = clan;
        clan.IsSelected = true;
    }
}

internal sealed class MercenaryClanItemVM : ViewModel
{
    private readonly Clan _clan;
    private readonly Kingdom _kingdom;
    private readonly Action<MercenaryClanItemVM> _onSelect;
    private string _clanName = string.Empty;
    private string _leaderName = string.Empty;
    private string _serviceStatus = string.Empty;
    private bool _isSelected;

    public MercenaryClanItemVM(Clan clan, Kingdom kingdom, Action<MercenaryClanItemVM> onSelect)
    {
        _clan = clan;
        _kingdom = kingdom;
        _onSelect = onSelect;
        Banner = new BannerImageIdentifierVM(clan.Banner, true);
        ClanLevel = clan.Tier;
        Strength = (int)Math.Round(clan.CurrentTotalStrength);
        ActiveParties = clan.WarPartyComponents.Count(component => component.MobileParty.IsActive);
        ClanMembers = clan.Heroes.Count(hero =>
            !hero.IsDisabled
            && !hero.IsNotSpawned
            && hero.IsAlive
            && !hero.IsChild);
        RelationToRuler = clan.Leader is not null && kingdom.Leader is not null
            ? clan.Leader.GetRelation(kingdom.Leader)
            : 0;
        InfluencePrice = clan.MercenaryAwardMultiplier;
        AccumulatedInfluence = FormatInfluence(clan.Influence);
        ExpectedPayment = MercenaryPaymentService.CalculateExpectedPayment(clan);
        RefreshValues();
    }

    [DataSourceProperty]
    public BannerImageIdentifierVM Banner { get; }

    [DataSourceProperty]
    public string ClanName
    {
        get => _clanName;
        private set
        {
            if (_clanName == value)
            {
                return;
            }

            _clanName = value;
            OnPropertyChangedWithValue(value);
        }
    }

    [DataSourceProperty]
    public string LeaderName
    {
        get => _leaderName;
        private set
        {
            if (_leaderName == value)
            {
                return;
            }

            _leaderName = value;
            OnPropertyChangedWithValue(value);
        }
    }

    [DataSourceProperty]
    public int ClanLevel { get; }

    [DataSourceProperty]
    public int Strength { get; }

    [DataSourceProperty]
    public int ActiveParties { get; }

    [DataSourceProperty]
    public int ClanMembers { get; }

    [DataSourceProperty]
    public int RelationToRuler { get; }

    [DataSourceProperty]
    public int InfluencePrice { get; }

    [DataSourceProperty]
    public string AccumulatedInfluence { get; }

    [DataSourceProperty]
    public int ExpectedPayment { get; }

    [DataSourceProperty]
    public string ServiceStatus
    {
        get => _serviceStatus;
        private set
        {
            if (_serviceStatus == value)
            {
                return;
            }

            _serviceStatus = value;
            OnPropertyChangedWithValue(value);
        }
    }

    [DataSourceProperty]
    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected == value)
            {
                return;
            }

            _isSelected = value;
            OnPropertyChangedWithValue(value);
        }
    }

    public void ExecuteSelect()
    {
        _onSelect(this);
    }

    public override void RefreshValues()
    {
        base.RefreshValues();

        ClanName = _clan.Name.ToString();
        LeaderName = _clan.Leader?.Name.ToString() ?? string.Empty;
        ServiceStatus = new TextObject(
            _clan.IsUnderMercenaryService && _clan.Kingdom == _kingdom
                ? "{=MercenaryManagement_InService}In service"
                : "{=MercenaryManagement_NotInService}Not in service").ToString();
    }

    public override void OnFinalize()
    {
        Banner.OnFinalize();
        base.OnFinalize();
    }

    private static string FormatInfluence(float influence)
    {
        TextObject text = new TextObject("{INFLUENCE}");
        text.SetTextVariable("INFLUENCE", influence, 2);
        return text.ToString();
    }
}
