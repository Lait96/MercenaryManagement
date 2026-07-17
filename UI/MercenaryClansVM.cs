using System.Linq;
using Bannerlord.UIExtenderEx.Attributes;
using MercenaryManagement.Services;
using MercenaryManagement.Behaviors;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.Core;

namespace MercenaryManagement.UI;

internal sealed class MercenaryClansVM : ViewModel
{
    private Kingdom? _kingdom;
    private bool _hasClans;
    private bool _hasAvailableClans;
    private bool _hasOtherKingdomClans;
    private MercenaryClanItemVM? _selectedClan;
    private MercenaryClanItemVM? _readOnlySelectedClan;
    private readonly MBBindingList<MercenaryPartyItemVM> _emptyActivePartyItems =
        new MBBindingList<MercenaryPartyItemVM>();

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
    public bool IsRulerMercenaryView => _kingdom is not null
                                         && MercenaryHiringService.IsPlayerRuler(_kingdom);

    [DataSourceProperty]
    public bool IsReadOnlyMercenaryView => _kingdom is not null && !IsRulerMercenaryView;

    [DataSourceProperty]
    public string ReadOnlyClanName => _readOnlySelectedClan?.ClanName ?? string.Empty;

    [DataSourceProperty]
    public string ReadOnlyLeaderName => _readOnlySelectedClan?.LeaderName ?? string.Empty;

    [DataSourceProperty]
    public int ReadOnlyClanLevel => _readOnlySelectedClan?.ClanLevel ?? 0;

    [DataSourceProperty]
    public int ReadOnlyStrength => _readOnlySelectedClan?.Strength ?? 0;

    [DataSourceProperty]
    public int ReadOnlyActiveParties => _readOnlySelectedClan?.ActiveParties ?? 0;

    [DataSourceProperty]
    public int ReadOnlyClanMembers => _readOnlySelectedClan?.ClanMembers ?? 0;

    [DataSourceProperty]
    public int ReadOnlyRelationToRuler => _readOnlySelectedClan?.RelationToRuler ?? 0;

    [DataSourceProperty]
    public int ReadOnlyInfluencePrice => _readOnlySelectedClan?.InfluencePrice ?? 0;

    [DataSourceProperty]
    public string ReadOnlyServiceStatus => _readOnlySelectedClan?.ServiceStatus ?? string.Empty;

    [DataSourceProperty]
    public string ReadOnlyContractState => _readOnlySelectedClan?.ContractState ?? string.Empty;

    [DataSourceProperty]
    public string ReadOnlyContractStart => _readOnlySelectedClan?.ContractStart ?? string.Empty;

    [DataSourceProperty]
    public string ReadOnlyContractEnd => _readOnlySelectedClan?.ContractEnd ?? string.Empty;

    [DataSourceProperty]
    public string ReadOnlyStrategicPriorityName =>
        _readOnlySelectedClan?.StrategicPriorityName ?? string.Empty;

    [DataSourceProperty]
    public string ReadOnlyStrategicPriorityDescription =>
        _readOnlySelectedClan?.StrategicPriorityDescription ?? string.Empty;

    [DataSourceProperty]
    public bool ReadOnlyShouldShowInfluencePrice =>
        _readOnlySelectedClan?.ShouldShowInfluencePrice == true;

    [DataSourceProperty]
    public bool ReadOnlyIsServingPlayerKingdom =>
        _readOnlySelectedClan?.IsServingPlayerKingdom == true;

    [DataSourceProperty] public int ReadOnlySatisfaction => _readOnlySelectedClan?.Satisfaction ?? 0;
    [DataSourceProperty] public int ReadOnlyReliability => _readOnlySelectedClan?.Reliability ?? 0;
    [DataSourceProperty] public int ReadOnlyDepartureRisk => _readOnlySelectedClan?.DepartureRisk ?? 0;
    [DataSourceProperty] public int ReadOnlyEmployerSatisfaction => _readOnlySelectedClan?.EmployerSatisfaction ?? 0;
    [DataSourceProperty] public int ReadOnlyEmployerTerminationRisk => _readOnlySelectedClan?.EmployerTerminationRisk ?? 0;
    [DataSourceProperty] public bool ReadOnlyIsPlayerClan => _readOnlySelectedClan?.Clan == Clan.PlayerClan;
    [DataSourceProperty] public bool ReadOnlyIsNotPlayerClan => !ReadOnlyIsPlayerClan;
    [DataSourceProperty] public string ReadOnlyPerformance => _readOnlySelectedClan?.Performance ?? string.Empty;
    [DataSourceProperty] public string ReadOnlyCurrentActivity => _readOnlySelectedClan?.CurrentActivity ?? string.Empty;
    [DataSourceProperty] public int ReadOnlyRecentEarnings => _readOnlySelectedClan?.RecentEarnings ?? 0;
    [DataSourceProperty] public MBBindingList<MercenaryPartyItemVM> ReadOnlyActivePartyItems =>
        _readOnlySelectedClan?.ActivePartyItems ?? _emptyActivePartyItems;

    [DataSourceProperty]
    public bool IsSelectedClanServingPlayerKingdom => SelectedClan?.IsServingPlayerKingdom == true;

    [DataSourceProperty]
    public bool IsSelectedClanInfluencePriceVisible => SelectedClan?.ShouldShowInfluencePrice == true;

    [DataSourceProperty]
    public bool IsSelectedClanSettingsVisible => SelectedClan?.IsServingPlayerKingdom == true;

    [DataSourceProperty]
    public bool CanEditSelectedClanStrategicPriority =>
        SelectedClan?.CanEditStrategicPriority == true;

    [DataSourceProperty]
    public bool IsSelectedClanStrategicPriorityReadOnly =>
        !CanEditSelectedClanStrategicPriority;

    [DataSourceProperty]
    public bool CanHireSelectedClan => _kingdom is not null
                                       && MercenaryHiringService.IsPlayerRuler(_kingdom)
                                       && SelectedClan?.IsFreeMercenaryClan == true;

    [DataSourceProperty]
    public bool CanDismissSelectedClan => _kingdom is not null
                                          && MercenaryHiringService.IsPlayerRuler(_kingdom)
                                          && SelectedClan?.IsServingPlayerKingdom == true;

    [DataSourceProperty]
    public bool CanRenewSelectedClan => CanDismissSelectedClan;

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
            OnPropertyChangedWithValue(
                IsSelectedClanSettingsVisible,
                nameof(IsSelectedClanSettingsVisible));
            OnPropertyChangedWithValue(
                CanEditSelectedClanStrategicPriority,
                nameof(CanEditSelectedClanStrategicPriority));
            OnPropertyChangedWithValue(
                IsSelectedClanStrategicPriorityReadOnly,
                nameof(IsSelectedClanStrategicPriorityReadOnly));
            OnPropertyChangedWithValue(CanHireSelectedClan, nameof(CanHireSelectedClan));
            OnPropertyChangedWithValue(CanDismissSelectedClan, nameof(CanDismissSelectedClan));
            OnPropertyChangedWithValue(CanRenewSelectedClan, nameof(CanRenewSelectedClan));
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
        "{=MercenaryManagement_InfluencePrice}Dinars per influence").ToString();

    [DataSourceProperty]
    public string StrategicPriorityText => new TextObject(
        "{=MercenaryManagement_StrategicPriority}Strategic priority").ToString();

    [DataSourceProperty]
    public string ClanInformationText => new TextObject(
        "{=MercenaryManagement_ClanInformation}Clan information").ToString();

    [DataSourceProperty]
    public string ContractSettingsText => new TextObject(
        "{=MercenaryManagement_ContractSettings}Contract settings").ToString();

    [DataSourceProperty]
    public string AccumulatedInfluenceText => new TextObject(
        "{=MercenaryManagement_AccumulatedInfluence}Accumulated influence").ToString();

    [DataSourceProperty]
    public string ExpectedPaymentText => new TextObject(
        "{=MercenaryManagement_ExpectedPayment}Expected payment (dinars)").ToString();

    [DataSourceProperty]
    public string ServiceStatusText => new TextObject(
        "{=MercenaryManagement_ServiceStatus}Service status").ToString();

    [DataSourceProperty]
    public string ContractStateText => new TextObject(
        "{=MercenaryManagement_ContractState}Contract state").ToString();

    [DataSourceProperty]
    public string ContractStartText => new TextObject(
        "{=MercenaryManagement_ContractStart}Contract start").ToString();

    [DataSourceProperty]
    public string ContractEndText => new TextObject(
        "{=MercenaryManagement_ContractEnd}Contract end").ToString();

    [DataSourceProperty]
    public string HireText => new TextObject("{=MercenaryManagement_Hire}Hire clan").ToString();

    [DataSourceProperty]
    public string DismissText => new TextObject("{=MercenaryManagement_Dismiss}Dismiss clan").ToString();

    [DataSourceProperty]
    public string RenewText => new TextObject("{=MercenaryManagement_Renew60Days}Renew for 60 days").ToString();

    [DataSourceProperty]
    public string SatisfactionText => new TextObject("{=MercenaryManagement_Satisfaction}Satisfaction").ToString();

    [DataSourceProperty]
    public string ReliabilityText => new TextObject("{=MercenaryManagement_Reliability}Reliability").ToString();

    [DataSourceProperty]
    public string DepartureRiskText => new TextObject("{=MercenaryManagement_DepartureRisk}Departure risk").ToString();

    [DataSourceProperty]
    public string EmployerSatisfactionText => new TextObject("{=MercenaryManagement_EmployerSatisfaction}Employer satisfaction").ToString();

    [DataSourceProperty]
    public string EmployerTerminationRiskText => new TextObject("{=MercenaryManagement_EmployerTerminationRisk}Risk of employer termination").ToString();

    [DataSourceProperty]
    public string PerformanceText => new TextObject("{=MercenaryManagement_Performance}Performance").ToString();

    [DataSourceProperty]
    public string CurrentActivityText => new TextObject("{=MercenaryManagement_CurrentActivity}Current activity").ToString();

    [DataSourceProperty]
    public string PartyNameText => new TextObject("{=MercenaryManagement_PartyName}Party").ToString();

    [DataSourceProperty]
    public string PartyLeaderText => new TextObject("{=MercenaryManagement_PartyLeader}Leader").ToString();

    [DataSourceProperty]
    public string PartyTroopsText => new TextObject("{=MercenaryManagement_PartyTroops}Troops").ToString();

    [DataSourceProperty]
    public string PartyActivityText => new TextObject("{=MercenaryManagement_PartyActivity}Activity").ToString();

    [DataSourceProperty]
    public string AcceptanceChanceText => new TextObject("{=MercenaryManagement_AcceptanceChance}Estimated acceptance chance").ToString();

    [DataSourceProperty]
    public string PreferredRoleText => new TextObject("{=MercenaryManagement_PreferredRole}Preferred role").ToString();

    [DataSourceProperty]
    public string RecentEarningsText => new TextObject("{=MercenaryManagement_RecentEarnings}Earnings in the last 7 days").ToString();

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
        OnPropertyChangedWithValue(StrategicPriorityText, nameof(StrategicPriorityText));
        OnPropertyChangedWithValue(ClanInformationText, nameof(ClanInformationText));
        OnPropertyChangedWithValue(ContractSettingsText, nameof(ContractSettingsText));
        OnPropertyChangedWithValue(AccumulatedInfluenceText, nameof(AccumulatedInfluenceText));
        OnPropertyChangedWithValue(ExpectedPaymentText, nameof(ExpectedPaymentText));
        OnPropertyChangedWithValue(ServiceStatusText, nameof(ServiceStatusText));
        OnPropertyChangedWithValue(ContractStateText, nameof(ContractStateText));
        OnPropertyChangedWithValue(ContractStartText, nameof(ContractStartText));
        OnPropertyChangedWithValue(ContractEndText, nameof(ContractEndText));
        OnPropertyChangedWithValue(HireText, nameof(HireText));
        OnPropertyChangedWithValue(DismissText, nameof(DismissText));
        OnPropertyChangedWithValue(PoachText, nameof(PoachText));
        OnPropertyChangedWithValue(EmptyStateText, nameof(EmptyStateText));
        OnPropertyChangedWithValue(NoHiredClansText, nameof(NoHiredClansText));
        OnPropertyChangedWithValue(NoAvailableClansText, nameof(NoAvailableClansText));
        OnPropertyChangedWithValue(NoOtherKingdomClansText, nameof(NoOtherKingdomClansText));
        OnPropertyChangedWithValue(
            CanEditSelectedClanStrategicPriority,
            nameof(CanEditSelectedClanStrategicPriority));
        OnPropertyChangedWithValue(
            IsSelectedClanStrategicPriorityReadOnly,
            nameof(IsSelectedClanStrategicPriorityReadOnly));

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

        NotifyReadOnlySelectedClanChanged();
    }

    public void Refresh(Kingdom? kingdom)
    {
        _kingdom = kingdom;
        SelectedClan = null;
        SetReadOnlySelectedClan(null);
        OnPropertyChangedWithValue(IsRulerMercenaryView, nameof(IsRulerMercenaryView));
        OnPropertyChangedWithValue(IsReadOnlyMercenaryView, nameof(IsReadOnlyMercenaryView));

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
                         .Where(MercenaryHiringService.IsFreeMercenaryClan)
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

        MercenaryClanItemVM? playerMercenary = Clans.FirstOrDefault(item => item.Clan == Clan.PlayerClan);
        if (playerMercenary is not null)
        {
            SelectClan(playerMercenary);
        }
        else if (HasClans)
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
        if (selectedClan is null)
        {
            return;
        }

        MercenaryClanActionController.ShowHire(selectedClan.Clan, _kingdom, Refresh);
    }

    [DataSourceMethod]
    public void ExecuteDismissSelectedClan()
    {
        MercenaryClanItemVM? selectedClan = SelectedClan;
        if (selectedClan is null)
        {
            return;
        }

        MercenaryClanActionController.ShowDismiss(selectedClan.Clan, _kingdom, Refresh);
    }

    [DataSourceMethod]
    public void ExecutePoachSelectedClan()
    {
        MercenaryClanItemVM? selectedClan = SelectedClan;
        if (selectedClan is null)
        {
            return;
        }

        MercenaryClanActionController.ShowPoach(selectedClan.Clan, _kingdom, Refresh);
    }

    [DataSourceMethod]
    public void ExecuteRenewSelectedClan()
    {
        if (_kingdom is null || !CanRenewSelectedClan || SelectedClan is null) return;
        MercenaryContractBehavior? behavior = Campaign.Current?
            .GetCampaignBehavior<MercenaryContractBehavior>();
        if (behavior?.TryGetContract(SelectedClan.Clan, out var contract) != true) return;
        float acceptance = 0.25f + contract.Satisfaction / 200f + contract.Reliability / 400f;
        if (MBRandom.RandomFloat <= System.Math.Min(0.95f, acceptance)
            && behavior.SetContractEndTime(SelectedClan.Clan, CampaignTime.DaysFromNow(60f)))
        {
            InformationManager.DisplayMessage(new InformationMessage(new TextObject(
                "{=MercenaryManagement_RenewAccepted}The clan accepted the contract renewal.").ToString()));
        }
        else InformationManager.DisplayMessage(new InformationMessage(new TextObject(
            "{=MercenaryManagement_RenewDeclined}The clan declined the contract renewal.").ToString()));
        Refresh(_kingdom);
    }

    public override void OnFinalize()
    {
        SelectedClan = null;
        SetReadOnlySelectedClan(null);

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
        if (IsReadOnlyMercenaryView)
        {
            SetReadOnlySelectedClan(clan);
            return;
        }

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

    private void SetReadOnlySelectedClan(MercenaryClanItemVM? clan)
    {
        if (_readOnlySelectedClan == clan)
        {
            return;
        }

        if (_readOnlySelectedClan is not null)
        {
            _readOnlySelectedClan.IsSelected = false;
        }

        _readOnlySelectedClan = clan;
        if (clan is not null)
        {
            clan.IsSelected = true;
        }

        NotifyReadOnlySelectedClanChanged();
    }

    private void NotifyReadOnlySelectedClanChanged()
    {
        OnPropertyChangedWithValue(ReadOnlyClanName, nameof(ReadOnlyClanName));
        OnPropertyChangedWithValue(ReadOnlyLeaderName, nameof(ReadOnlyLeaderName));
        OnPropertyChangedWithValue(ReadOnlyClanLevel, nameof(ReadOnlyClanLevel));
        OnPropertyChangedWithValue(ReadOnlyStrength, nameof(ReadOnlyStrength));
        OnPropertyChangedWithValue(ReadOnlyActiveParties, nameof(ReadOnlyActiveParties));
        OnPropertyChangedWithValue(ReadOnlyClanMembers, nameof(ReadOnlyClanMembers));
        OnPropertyChangedWithValue(ReadOnlyRelationToRuler, nameof(ReadOnlyRelationToRuler));
        OnPropertyChangedWithValue(ReadOnlyInfluencePrice, nameof(ReadOnlyInfluencePrice));
        OnPropertyChangedWithValue(ReadOnlyServiceStatus, nameof(ReadOnlyServiceStatus));
        OnPropertyChangedWithValue(ReadOnlyContractState, nameof(ReadOnlyContractState));
        OnPropertyChangedWithValue(ReadOnlyContractStart, nameof(ReadOnlyContractStart));
        OnPropertyChangedWithValue(ReadOnlyContractEnd, nameof(ReadOnlyContractEnd));
        OnPropertyChangedWithValue(
            ReadOnlyStrategicPriorityName,
            nameof(ReadOnlyStrategicPriorityName));
        OnPropertyChangedWithValue(
            ReadOnlyStrategicPriorityDescription,
            nameof(ReadOnlyStrategicPriorityDescription));
        OnPropertyChangedWithValue(
            ReadOnlyShouldShowInfluencePrice,
            nameof(ReadOnlyShouldShowInfluencePrice));
        OnPropertyChangedWithValue(
            ReadOnlyIsServingPlayerKingdom,
            nameof(ReadOnlyIsServingPlayerKingdom));
        OnPropertyChangedWithValue(ReadOnlySatisfaction, nameof(ReadOnlySatisfaction));
        OnPropertyChangedWithValue(ReadOnlyReliability, nameof(ReadOnlyReliability));
        OnPropertyChangedWithValue(ReadOnlyDepartureRisk, nameof(ReadOnlyDepartureRisk));
        OnPropertyChangedWithValue(ReadOnlyEmployerSatisfaction, nameof(ReadOnlyEmployerSatisfaction));
        OnPropertyChangedWithValue(ReadOnlyEmployerTerminationRisk, nameof(ReadOnlyEmployerTerminationRisk));
        OnPropertyChangedWithValue(ReadOnlyIsPlayerClan, nameof(ReadOnlyIsPlayerClan));
        OnPropertyChangedWithValue(ReadOnlyIsNotPlayerClan, nameof(ReadOnlyIsNotPlayerClan));
        OnPropertyChangedWithValue(ReadOnlyPerformance, nameof(ReadOnlyPerformance));
        OnPropertyChangedWithValue(ReadOnlyCurrentActivity, nameof(ReadOnlyCurrentActivity));
        OnPropertyChangedWithValue(ReadOnlyRecentEarnings, nameof(ReadOnlyRecentEarnings));
        OnPropertyChangedWithValue(ReadOnlyActivePartyItems, nameof(ReadOnlyActivePartyItems));
    }
}
