using System;
using System.Linq;
using Bannerlord.UIExtenderEx.Attributes;
using MercenaryManagement.Services;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core.ViewModelCollection.ImageIdentifiers;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace MercenaryManagement.UI;

internal sealed class MercenaryClansVM : ViewModel
{
    private Kingdom? _kingdom;
    private bool _hasClans;
    private bool _hasAvailableClans;
    private bool _hasOtherKingdomClans;
    private MercenaryClanItemVM? _selectedClan;

    public MercenaryClansVM()
    {
        Clans = new MBBindingList<MercenaryClanItemVM>();
        AvailableClans = new MBBindingList<MercenaryClanItemVM>();
        OtherKingdomClans = new MBBindingList<MercenaryClanItemVM>();
    }

    [DataSourceProperty]
    public MBBindingList<MercenaryClanItemVM> Clans { get; }

    [DataSourceProperty]
    public MBBindingList<MercenaryClanItemVM> AvailableClans { get; }

    [DataSourceProperty]
    public MBBindingList<MercenaryClanItemVM> OtherKingdomClans { get; }

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
            OnPropertyChangedWithValue(!value, nameof(AreHiredClansEmpty));
            OnPropertyChangedWithValue(HasAnyClans, nameof(HasAnyClans));
            OnPropertyChangedWithValue(IsEmpty, nameof(IsEmpty));
        }
    }

    [DataSourceProperty]
    public bool HasAvailableClans
    {
        get => _hasAvailableClans;
        private set
        {
            if (_hasAvailableClans == value)
            {
                return;
            }

            _hasAvailableClans = value;
            OnPropertyChangedWithValue(value);
            OnPropertyChangedWithValue(!value, nameof(AreAvailableClansEmpty));
            OnPropertyChangedWithValue(HasAnyClans, nameof(HasAnyClans));
            OnPropertyChangedWithValue(IsEmpty, nameof(IsEmpty));
        }
    }

    [DataSourceProperty]
    public bool HasOtherKingdomClans
    {
        get => _hasOtherKingdomClans;
        private set
        {
            if (_hasOtherKingdomClans == value)
            {
                return;
            }

            _hasOtherKingdomClans = value;
            OnPropertyChangedWithValue(value);
            OnPropertyChangedWithValue(!value, nameof(AreOtherKingdomClansEmpty));
            OnPropertyChangedWithValue(HasAnyClans, nameof(HasAnyClans));
            OnPropertyChangedWithValue(IsEmpty, nameof(IsEmpty));
        }
    }

    [DataSourceProperty]
    public bool HasAnyClans => HasClans || HasAvailableClans || HasOtherKingdomClans;

    [DataSourceProperty]
    public bool IsEmpty => !HasAnyClans;

    [DataSourceProperty]
    public bool AreHiredClansEmpty => !HasClans;

    [DataSourceProperty]
    public bool AreAvailableClansEmpty => !HasAvailableClans;

    [DataSourceProperty]
    public bool AreOtherKingdomClansEmpty => !HasOtherKingdomClans;

    [DataSourceProperty]
    public bool IsSelectedClanServingPlayerKingdom => SelectedClan?.IsServingPlayerKingdom == true;

    [DataSourceProperty]
    public bool IsSelectedClanInfluencePriceVisible => SelectedClan?.ShouldShowInfluencePrice == true;

    [DataSourceProperty]
    public bool CanHireSelectedClan => _kingdom is not null
                                       && MercenaryHiringService.IsPlayerRuler(_kingdom)
                                       && SelectedClan?.IsFreeMercenaryClan == true;

    [DataSourceProperty]
    public bool CanDismissSelectedClan => _kingdom is not null
                                          && MercenaryHiringService.IsPlayerRuler(_kingdom)
                                          && SelectedClan?.IsServingPlayerKingdom == true;

    [DataSourceProperty]
    public bool CanPoachSelectedClan => _kingdom is not null
                                        && MercenaryHiringService.IsPlayerRuler(_kingdom)
                                        && SelectedClan?.IsServingOtherKingdom == true;

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
            OnPropertyChangedWithValue(
                IsSelectedClanServingPlayerKingdom,
                nameof(IsSelectedClanServingPlayerKingdom));
            OnPropertyChangedWithValue(
                IsSelectedClanInfluencePriceVisible,
                nameof(IsSelectedClanInfluencePriceVisible));
            OnPropertyChangedWithValue(CanHireSelectedClan, nameof(CanHireSelectedClan));
            OnPropertyChangedWithValue(CanDismissSelectedClan, nameof(CanDismissSelectedClan));
            OnPropertyChangedWithValue(CanPoachSelectedClan, nameof(CanPoachSelectedClan));
        }
    }

    [DataSourceProperty]
    public string TitleText => new TextObject("{=MercenaryManagement_MercenaryClans}Mercenary clans").ToString();

    [DataSourceProperty]
    public string HiredClansTitleText => new TextObject(
        "{=MercenaryManagement_HiredClans}Serving this kingdom").ToString();

    [DataSourceProperty]
    public string AvailableClansTitleText => new TextObject(
        "{=MercenaryManagement_AvailableClans}Available for hire").ToString();

    [DataSourceProperty]
    public string OtherKingdomClansTitleText => new TextObject(
        "{=MercenaryManagement_OtherKingdomClans}Serving other kingdoms").ToString();

    [DataSourceProperty]
    public string ClanText => new TextObject("{=MercenaryManagement_Clan}Clan").ToString();

    [DataSourceProperty]
    public string LeaderText => new TextObject("{=MercenaryManagement_Leader}Leader").ToString();

    [DataSourceProperty]
    public string StrengthText => new TextObject("{=MercenaryManagement_Strength}Strength").ToString();

    [DataSourceProperty]
    public string ActivePartiesText => new TextObject("{=MercenaryManagement_ActiveParties}Active parties").ToString();

    [DataSourceProperty]
    public string ActivePartiesColumnText => new TextObject(
        "{=MercenaryManagement_PartiesColumn}Parties").ToString();

    [DataSourceProperty]
    public string ClanLevelText => new TextObject("{=MercenaryManagement_ClanLevel}Clan level").ToString();

    [DataSourceProperty]
    public string ClanLevelColumnText => new TextObject(
        "{=MercenaryManagement_LevelColumn}Level").ToString();

    [DataSourceProperty]
    public string ClanMembersText => new TextObject("{=MercenaryManagement_ClanMembers}Clan members").ToString();

    [DataSourceProperty]
    public string RelationToRulerText => new TextObject(
        "{=MercenaryManagement_RelationToRuler}Leader's relation to the ruler").ToString();

    [DataSourceProperty]
    public string RelationColumnText => new TextObject(
        "{=MercenaryManagement_RelationColumn}Relation").ToString();

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
    public string HireText => new TextObject("{=MercenaryManagement_Hire}Hire clan").ToString();

    [DataSourceProperty]
    public string DismissText => new TextObject("{=MercenaryManagement_Dismiss}Dismiss clan").ToString();

    [DataSourceProperty]
    public string PoachText => new TextObject("{=MercenaryManagement_Poach}Poach clan").ToString();

    [DataSourceProperty]
    public string EmptyStateText => new TextObject(
        "{=MercenaryManagement_NoMercenaryClans}No mercenary clans are serving this kingdom or available for hire.").ToString();

    [DataSourceProperty]
    public string NoHiredClansText => new TextObject(
        "{=MercenaryManagement_NoHiredClans}No mercenary clans are serving this kingdom.").ToString();

    [DataSourceProperty]
    public string NoAvailableClansText => new TextObject(
        "{=MercenaryManagement_NoAvailableClans}No independent mercenary clans are available.").ToString();

    [DataSourceProperty]
    public string NoOtherKingdomClansText => new TextObject(
        "{=MercenaryManagement_NoOtherKingdomClans}No mercenary clans are serving other kingdoms.").ToString();

    public override void RefreshValues()
    {
        base.RefreshValues();

        OnPropertyChangedWithValue(TitleText, nameof(TitleText));
        OnPropertyChangedWithValue(HiredClansTitleText, nameof(HiredClansTitleText));
        OnPropertyChangedWithValue(AvailableClansTitleText, nameof(AvailableClansTitleText));
        OnPropertyChangedWithValue(OtherKingdomClansTitleText, nameof(OtherKingdomClansTitleText));
        OnPropertyChangedWithValue(ClanText, nameof(ClanText));
        OnPropertyChangedWithValue(LeaderText, nameof(LeaderText));
        OnPropertyChangedWithValue(StrengthText, nameof(StrengthText));
        OnPropertyChangedWithValue(ActivePartiesText, nameof(ActivePartiesText));
        OnPropertyChangedWithValue(ActivePartiesColumnText, nameof(ActivePartiesColumnText));
        OnPropertyChangedWithValue(ClanLevelText, nameof(ClanLevelText));
        OnPropertyChangedWithValue(ClanLevelColumnText, nameof(ClanLevelColumnText));
        OnPropertyChangedWithValue(ClanMembersText, nameof(ClanMembersText));
        OnPropertyChangedWithValue(RelationToRulerText, nameof(RelationToRulerText));
        OnPropertyChangedWithValue(RelationColumnText, nameof(RelationColumnText));
        OnPropertyChangedWithValue(InfluencePriceText, nameof(InfluencePriceText));
        OnPropertyChangedWithValue(AccumulatedInfluenceText, nameof(AccumulatedInfluenceText));
        OnPropertyChangedWithValue(ExpectedPaymentText, nameof(ExpectedPaymentText));
        OnPropertyChangedWithValue(ServiceStatusText, nameof(ServiceStatusText));
        OnPropertyChangedWithValue(HireText, nameof(HireText));
        OnPropertyChangedWithValue(DismissText, nameof(DismissText));
        OnPropertyChangedWithValue(PoachText, nameof(PoachText));
        OnPropertyChangedWithValue(EmptyStateText, nameof(EmptyStateText));
        OnPropertyChangedWithValue(NoHiredClansText, nameof(NoHiredClansText));
        OnPropertyChangedWithValue(NoAvailableClansText, nameof(NoAvailableClansText));
        OnPropertyChangedWithValue(NoOtherKingdomClansText, nameof(NoOtherKingdomClansText));

        foreach (MercenaryClanItemVM clan in Clans)
        {
            clan.RefreshValues();
        }

        foreach (MercenaryClanItemVM clan in AvailableClans)
        {
            clan.RefreshValues();
        }

        foreach (MercenaryClanItemVM clan in OtherKingdomClans)
        {
            clan.RefreshValues();
        }
    }

    public void Refresh(Kingdom? kingdom)
    {
        _kingdom = kingdom;
        SelectedClan = null;

        foreach (MercenaryClanItemVM clan in Clans)
        {
            clan.OnFinalize();
        }

        Clans.Clear();

        foreach (MercenaryClanItemVM clan in AvailableClans)
        {
            clan.OnFinalize();
        }

        AvailableClans.Clear();

        foreach (MercenaryClanItemVM clan in OtherKingdomClans)
        {
            clan.OnFinalize();
        }

        OtherKingdomClans.Clear();

        if (kingdom is not null)
        {
            foreach (Clan clan in kingdom.Clans
                         .Where(clan => clan.IsUnderMercenaryService)
                         .OrderBy(clan => clan.Name.ToString()))
            {
                Clans.Add(new MercenaryClanItemVM(clan, kingdom, SelectClan));
            }

            foreach (Clan clan in Clan.All
                         .Where(clan => clan.IsMinorFaction
                                        && !clan.IsBanditFaction
                                        && !clan.IsEliminated
                                        && !clan.IsUnderMercenaryService
                                        && clan.Kingdom is null)
                         .OrderBy(clan => clan.Name.ToString()))
            {
                AvailableClans.Add(new MercenaryClanItemVM(clan, kingdom, SelectClan));
            }

            foreach (Clan clan in Kingdom.All
                         .Where(otherKingdom => otherKingdom != kingdom
                                                && !otherKingdom.IsEliminated)
                         .OrderBy(otherKingdom => otherKingdom.Name.ToString())
                         .SelectMany(otherKingdom => otherKingdom.Clans
                             .Where(clan => !clan.IsEliminated
                                            && clan.IsUnderMercenaryService)
                             .OrderBy(clan => clan.Name.ToString())))
            {
                OtherKingdomClans.Add(new MercenaryClanItemVM(clan, kingdom, SelectClan));
            }
        }

        HasClans = Clans.Count > 0;
        HasAvailableClans = AvailableClans.Count > 0;
        HasOtherKingdomClans = OtherKingdomClans.Count > 0;

        if (HasClans)
        {
            SelectClan(Clans[0]);
        }
        else if (HasAvailableClans)
        {
            SelectClan(AvailableClans[0]);
        }
        else if (HasOtherKingdomClans)
        {
            SelectClan(OtherKingdomClans[0]);
        }
    }

    [DataSourceMethod]
    public void ExecuteHireSelectedClan()
    {
        MercenaryClanItemVM? selectedClan = SelectedClan;
        Kingdom? kingdom = _kingdom;
        if (selectedClan is null)
        {
            return;
        }

        if (MercenaryHiringService.TryGetFailureReason(selectedClan.Clan, kingdom, out TextObject reason))
        {
            InformationManager.DisplayMessage(new InformationMessage(reason.ToString()));
            return;
        }

        int influencePrice = MercenaryHiringService.CalculateHirePrice(selectedClan.Clan, kingdom!);
        TextObject confirmation = new TextObject(
            "{=MercenaryManagement_HireConfirmation}Hire the {CLAN} clan for {PRICE} denars per influence point?");
        confirmation.SetTextVariable("CLAN", selectedClan.Clan.Name);
        confirmation.SetTextVariable("PRICE", influencePrice);

        InformationManager.ShowInquiry(
            new InquiryData(
                new TextObject("{=MercenaryManagement_HireConfirmationTitle}Hire mercenaries").ToString(),
                confirmation.ToString(),
                true,
                true,
                new TextObject("{=MercenaryManagement_ConfirmHire}Hire").ToString(),
                new TextObject("{=MercenaryManagement_Cancel}Cancel").ToString(),
                () => HireClan(selectedClan.Clan, kingdom!),
                null),
            true);
    }

    [DataSourceMethod]
    public void ExecuteDismissSelectedClan()
    {
        MercenaryClanItemVM? selectedClan = SelectedClan;
        Kingdom? kingdom = _kingdom;
        if (selectedClan is null)
        {
            return;
        }

        if (MercenaryDismissalService.TryGetFailureReason(selectedClan.Clan, kingdom, out TextObject reason))
        {
            InformationManager.DisplayMessage(new InformationMessage(reason.ToString()));
            return;
        }

        int outstandingPayment = MercenaryDismissalService.CalculateOutstandingPayment(selectedClan.Clan);
        TextObject confirmation = outstandingPayment > 0
            ? new TextObject(
                "{=MercenaryManagement_DismissConfirmationWithPayment}Dismiss the {CLAN} clan and pay the outstanding {GOLD} denars?")
            : new TextObject(
                "{=MercenaryManagement_DismissConfirmation}Dismiss the {CLAN} clan from your kingdom's service?");
        confirmation.SetTextVariable("CLAN", selectedClan.Clan.Name);
        confirmation.SetTextVariable("GOLD", outstandingPayment);

        InformationManager.ShowInquiry(
            new InquiryData(
                new TextObject("{=MercenaryManagement_DismissConfirmationTitle}Dismiss mercenaries").ToString(),
                confirmation.ToString(),
                true,
                true,
                new TextObject("{=MercenaryManagement_ConfirmDismiss}Dismiss").ToString(),
                new TextObject("{=MercenaryManagement_Cancel}Cancel").ToString(),
                () => DismissClan(selectedClan.Clan, kingdom!),
                null),
            true);
    }

    [DataSourceMethod]
    public void ExecutePoachSelectedClan()
    {
        MercenaryClanItemVM? selectedClan = SelectedClan;
        Kingdom? targetKingdom = _kingdom;
        if (selectedClan is null)
        {
            return;
        }

        if (MercenaryPoachingService.TryGetFailureReason(
                selectedClan.Clan,
                targetKingdom,
                out TextObject reason))
        {
            InformationManager.DisplayMessage(new InformationMessage(reason.ToString()));
            return;
        }

        Kingdom currentKingdom = selectedClan.Clan.Kingdom!;
        int influencePrice = MercenaryHiringService.CalculateHirePrice(
            selectedClan.Clan,
            targetKingdom!);
        TextObject confirmation = new TextObject(
            "{=MercenaryManagement_PoachConfirmation}Persuade the {CLAN} clan to leave {CURRENT_KINGDOM} and enter your kingdom's service for {PRICE} denars per influence point?");
        confirmation.SetTextVariable("CLAN", selectedClan.Clan.Name);
        confirmation.SetTextVariable("CURRENT_KINGDOM", currentKingdom.Name);
        confirmation.SetTextVariable("PRICE", influencePrice);

        InformationManager.ShowInquiry(
            new InquiryData(
                new TextObject(
                    "{=MercenaryManagement_PoachConfirmationTitle}Recruit serving mercenaries").ToString(),
                confirmation.ToString(),
                true,
                true,
                new TextObject("{=MercenaryManagement_ConfirmPoach}Recruit").ToString(),
                new TextObject("{=MercenaryManagement_Cancel}Cancel").ToString(),
                () => PoachClan(selectedClan.Clan, currentKingdom, targetKingdom!),
                null),
            true);
    }

    public override void OnFinalize()
    {
        SelectedClan = null;

        foreach (MercenaryClanItemVM clan in Clans)
        {
            clan.OnFinalize();
        }

        Clans.Clear();

        foreach (MercenaryClanItemVM clan in AvailableClans)
        {
            clan.OnFinalize();
        }

        AvailableClans.Clear();

        foreach (MercenaryClanItemVM clan in OtherKingdomClans)
        {
            clan.OnFinalize();
        }

        OtherKingdomClans.Clear();
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

    private void HireClan(Clan clan, Kingdom kingdom)
    {
        if (MercenaryHiringService.TryGetFailureReason(clan, kingdom, out TextObject reason))
        {
            InformationManager.DisplayMessage(new InformationMessage(reason.ToString()));
            Refresh(kingdom);
            return;
        }

        MercenaryHiringService.Hire(clan, kingdom);

        if (clan.IsUnderMercenaryService && clan.Kingdom == kingdom)
        {
            TextObject success = new TextObject(
                "{=MercenaryManagement_HireSucceeded}The {CLAN} clan has entered your kingdom's service.");
            success.SetTextVariable("CLAN", clan.Name);
            InformationManager.DisplayMessage(new InformationMessage(success.ToString()));
        }
        else
        {
            InformationManager.DisplayMessage(new InformationMessage(
                new TextObject(
                    "{=MercenaryManagement_HireFailedUnknown}The clan could not enter your kingdom's service.")
                    .ToString()));
        }

        Refresh(kingdom);
    }

    private void DismissClan(Clan clan, Kingdom kingdom)
    {
        if (MercenaryDismissalService.TryGetFailureReason(clan, kingdom, out TextObject reason))
        {
            InformationManager.DisplayMessage(new InformationMessage(reason.ToString()));
            Refresh(kingdom);
            return;
        }

        MercenaryDismissalService.Dismiss(clan);

        if (!clan.IsUnderMercenaryService && clan.Kingdom is null)
        {
            TextObject success = new TextObject(
                "{=MercenaryManagement_DismissSucceeded}The {CLAN} clan has left your kingdom's service.");
            success.SetTextVariable("CLAN", clan.Name);
            InformationManager.DisplayMessage(new InformationMessage(success.ToString()));
        }
        else
        {
            InformationManager.DisplayMessage(new InformationMessage(
                new TextObject(
                    "{=MercenaryManagement_DismissFailedUnknown}The clan could not be dismissed from your kingdom's service.")
                    .ToString()));
        }

        Refresh(kingdom);
    }

    private void PoachClan(Clan clan, Kingdom expectedCurrentKingdom, Kingdom targetKingdom)
    {
        if (clan.Kingdom != expectedCurrentKingdom)
        {
            InformationManager.DisplayMessage(new InformationMessage(
                new TextObject(
                    "{=MercenaryManagement_PoachFailedEmployerChanged}The clan's current employer has changed. Select the clan again to make a new offer.")
                    .ToString()));
            Refresh(targetKingdom);
            return;
        }

        if (MercenaryPoachingService.TryGetFailureReason(clan, targetKingdom, out TextObject reason))
        {
            InformationManager.DisplayMessage(new InformationMessage(reason.ToString()));
            Refresh(targetKingdom);
            return;
        }

        MercenaryPoachingService.Poach(clan, targetKingdom);

        if (clan.IsUnderMercenaryService && clan.Kingdom == targetKingdom)
        {
            TextObject success = new TextObject(
                "{=MercenaryManagement_PoachSucceeded}The {CLAN} clan has left its former employer and entered your kingdom's service.");
            success.SetTextVariable("CLAN", clan.Name);
            InformationManager.DisplayMessage(new InformationMessage(success.ToString()));
        }
        else
        {
            InformationManager.DisplayMessage(new InformationMessage(
                new TextObject(
                    "{=MercenaryManagement_PoachFailedUnknown}The clan could not switch to your kingdom's service.")
                    .ToString()));
        }

        Refresh(targetKingdom);
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
        InfluencePrice = MercenaryHiringService.IsFreeMercenaryClan(clan)
            ? MercenaryHiringService.CalculateHirePrice(clan, kingdom)
            : clan.MercenaryAwardMultiplier;
        AccumulatedInfluence = FormatInfluence(clan.Influence);
        ExpectedPayment = MercenaryPaymentService.CalculateExpectedPayment(clan);
        RefreshValues();
    }

    [DataSourceProperty]
    public BannerImageIdentifierVM Banner { get; }

    internal Clan Clan => _clan;

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
    public bool IsServingPlayerKingdom => _clan.IsUnderMercenaryService && _clan.Kingdom == _kingdom;

    [DataSourceProperty]
    public bool IsFreeMercenaryClan => MercenaryHiringService.IsFreeMercenaryClan(_clan);

    [DataSourceProperty]
    public bool IsServingOtherKingdom => MercenaryPoachingService.IsServingAnotherKingdom(_clan, _kingdom);

    [DataSourceProperty]
    public bool ShouldShowInfluencePrice => IsServingPlayerKingdom || IsFreeMercenaryClan;

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
        TextObject serviceStatus;

        if (IsServingPlayerKingdom)
        {
            serviceStatus = new TextObject("{=MercenaryManagement_InService}Serving this kingdom");
        }
        else if (_clan.IsUnderMercenaryService && _clan.Kingdom is not null)
        {
            serviceStatus = new TextObject("{=MercenaryManagement_InOtherKingdom}Serving {KINGDOM}");
            serviceStatus.SetTextVariable("KINGDOM", _clan.Kingdom.Name);
        }
        else
        {
            bool hasFailureReason = MercenaryHiringService.IsPlayerRuler(_kingdom)
                ? MercenaryHiringService.TryGetFailureReason(_clan, _kingdom, out _)
                : MercenaryHiringService.TryGetInherentFailureReason(_clan, _kingdom, out _);

            serviceStatus = hasFailureReason
                ? new TextObject("{=MercenaryManagement_UnavailableForHire}Unavailable for hire")
                : new TextObject("{=MercenaryManagement_NotInService}Available for hire");
        }

        ServiceStatus = serviceStatus.ToString();
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
