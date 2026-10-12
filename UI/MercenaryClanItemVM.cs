using System;
using System.Linq;
using MercenaryManagement.Behaviors;
using MercenaryManagement.Contracts;
using MercenaryManagement.Services;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core.ViewModelCollection.ImageIdentifiers;
using TaleWorlds.Core.ViewModelCollection.Selector;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace MercenaryManagement.UI;

internal sealed class MercenaryClanItemVM : ViewModel
{
    private readonly Clan _clan;
    private readonly Kingdom _kingdom;
    private readonly Action<MercenaryClanItemVM> _onSelect;
    private MercenaryContract? _contract;
    private string _clanName = string.Empty;
    private string _leaderName = string.Empty;
    private string _serviceStatus = string.Empty;
    private int _influencePrice;
    private int _expectedPayment;
    private bool _isSelected;
    private bool _isRefreshingPrioritySelector;
    private bool _isSettingInfluencePrice;

    public MercenaryClanItemVM(Clan clan, Kingdom kingdom, Action<MercenaryClanItemVM> onSelect)
    {
        _clan = clan;
        _kingdom = kingdom;
        _onSelect = onSelect;
        ActivePartyItems = new MBBindingList<MercenaryPartyItemVM>();
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
        _contract = GetActiveContract();
        PrioritySelector = new SelectorVM<SelectorItemVM>(
            GetStrategicPriorityNames(),
            (int)StrategicPriority,
            null!);
        PrioritySelector.SetOnChangeAction(OnStrategicPrioritySelected);
        _influencePrice = MercenaryHiringService.IsFreeMercenaryClan(clan)
            ? MercenaryHiringService.CalculateHirePrice(clan, kingdom)
            : MercenaryPaymentService.GetInfluencePrice(clan);
        AccumulatedInfluence = FormatInfluence(clan.Influence);
        _expectedPayment = MercenaryPaymentService.CalculateExpectedPayment(clan);
        RefreshValues();
    }

    [DataSourceProperty]
    public BannerImageIdentifierVM Banner { get; }

    [DataSourceProperty]
    public MBBindingList<MercenaryPartyItemVM> ActivePartyItems { get; }

    [DataSourceProperty]
    public bool HasActivePartyItems => ActivePartyItems.Count > 0;

    [DataSourceProperty] public string ActivePartiesText =>
        new TextObject("{=MercenaryManagement_ActiveParties}Active parties").ToString();
    [DataSourceProperty] public string PartyNameText =>
        new TextObject("{=MercenaryManagement_PartyName}Party").ToString();
    [DataSourceProperty] public string PartyLeaderText =>
        new TextObject("{=MercenaryManagement_PartyLeader}Leader").ToString();
    [DataSourceProperty] public string PartyTroopsText =>
        new TextObject("{=MercenaryManagement_PartyTroops}Troops").ToString();
    [DataSourceProperty] public string PartyActivityText =>
        new TextObject("{=MercenaryManagement_PartyActivity}Activity").ToString();

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
    public int InfluencePrice
    {
        get => _influencePrice;
        set
        {
            // The slider and the text input clamp the value to MinimumInfluencePrice..
            // MaximumInfluencePrice and push the clamped value back here. When the value
            // is refused (a free clan shows its hire price, which lies outside that range,
            // and TrySetInfluencePrice refuses any rate for a clan not in service), the old
            // value is announced again, the widget clamps it again, and so on until the
            // stack overflows. A set that arrives while this one is still running is that
            // echo: it is ignored.
            if (_influencePrice == value || _isSettingInfluencePrice)
            {
                return;
            }

            _isSettingInfluencePrice = true;
            try
            {
                if (!MercenaryPaymentService.TrySetInfluencePrice(_clan, _kingdom, value))
                {
                    OnPropertyChangedWithValue(_influencePrice);
                    return;
                }

                _influencePrice = value;
                OnPropertyChangedWithValue(value);
                OnPropertyChangedWithValue(
                    InfluencePricePercentageText,
                    nameof(InfluencePricePercentageText));

                ExpectedPayment = MercenaryPaymentService.CalculateExpectedPayment(_clan);
            }
            finally
            {
                _isSettingInfluencePrice = false;
            }
        }
    }

    [DataSourceProperty]
    public int BaseInfluencePrice => MercenaryPaymentService.GetBaseInfluencePrice(_clan);

    [DataSourceProperty]
    public string BaseInfluencePriceText
    {
        get
        {
            TextObject text = new TextObject(
                "{=MercenaryManagement_BaseInfluencePrice}Base: {PRICE}");
            text.SetTextVariable("PRICE", BaseInfluencePrice);
            return text.ToString();
        }
    }

    [DataSourceProperty]
    public string InfluencePricePercentageText => $"{CalculateInfluencePricePercentage()}%";

    [DataSourceProperty]
    public int MinimumInfluencePrice => MercenaryPaymentService.CalculateMinimumInfluencePrice(_clan);

    [DataSourceProperty]
    public int MaximumInfluencePrice => MercenaryPaymentService.CalculateMaximumInfluencePrice(_clan);

    [DataSourceProperty]
    public bool CanEditInfluencePrice => IsServingPlayerKingdom
                                         && MercenaryHiringService.IsPlayerRuler(_kingdom);

    [DataSourceProperty]
    public bool IsInfluencePriceReadOnly => !CanEditInfluencePrice;

    [DataSourceProperty]
    public SelectorVM<SelectorItemVM> PrioritySelector { get; }

    [DataSourceProperty]
    public string StrategicPriorityDescription => GetStrategicPriorityDescription(
        StrategicPriority).ToString();

    [DataSourceProperty]
    public string StrategicPriorityName => GetStrategicPriorityName(
        StrategicPriority).ToString();

    [DataSourceProperty]
    public bool CanEditStrategicPriority => IsServingPlayerKingdom
                                             && MercenaryHiringService.IsPlayerRuler(_kingdom);

    [DataSourceProperty]
    public string AccumulatedInfluence { get; }

    [DataSourceProperty]
    public int ExpectedPayment
    {
        get => _expectedPayment;
        private set
        {
            if (_expectedPayment == value)
            {
                return;
            }

            _expectedPayment = value;
            OnPropertyChangedWithValue(value);
        }
    }

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
    public string ContractState => _contract is null
        ? new TextObject("{=MercenaryManagement_NoActiveContract}No active contract").ToString()
        : new TextObject("{=MercenaryManagement_ContractActive}Active").ToString();

    [DataSourceProperty]
    public string ContractStart => _contract is null
        ? string.Empty
        : FormatPastTime(_contract.ServiceStartTime);

    [DataSourceProperty]
    public string ContractEnd => _contract is null
        ? string.Empty
        : FormatEndTime(_contract.EndTime);

    [DataSourceProperty]
    public bool HasActiveContract => _contract?.IsActive == true;

    [DataSourceProperty] public int Satisfaction => (int)Math.Round(_contract?.Satisfaction ?? 0f);
    [DataSourceProperty] public int EmployerSatisfaction => (int)Math.Round(_contract?.EmployerSatisfaction ?? 0f);
    [DataSourceProperty] public int Reliability => (int)Math.Round(GetReliability());
    [DataSourceProperty] public int DepartureRisk => (int)Math.Round(_contract is null ? 0f : MercenaryEvaluationService.CalculateDepartureRisk(_contract));
    [DataSourceProperty] public int EmployerTerminationRisk => (int)Math.Round(
        _contract is null ? 0f : MercenaryEvaluationService.CalculateEmployerTerminationRisk(_contract));
    [DataSourceProperty] public string Performance => _contract is null ? string.Empty : MercenaryEvaluationService.GetPerformanceSummary(_contract);
    [DataSourceProperty] public string CurrentActivity => _contract is null ? string.Empty : MercenaryEvaluationService.GetActivitySummary(_contract);
    [DataSourceProperty] public int RecentEarnings => _contract?.RecentEarnings ?? 0;
    [DataSourceProperty] public int AcceptanceChance => CalculateAcceptanceChance();
    [DataSourceProperty] public string PreferredRole => GetPreferredRole();

    [DataSourceProperty]
    public bool IsServingPlayerKingdom => _contract?.EmployerKingdom == _kingdom;

    [DataSourceProperty]
    public bool IsFreeMercenaryClan => MercenaryHiringService.IsFreeMercenaryClan(_clan);

    [DataSourceProperty]
    public bool IsServingOtherKingdom => MercenaryPoachingService.IsServingAnotherKingdom(_clan, _kingdom);

    [DataSourceProperty]
    public bool ShouldShowInfluencePrice => IsFreeMercenaryClan;

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

        OnPropertyChangedWithValue(BaseInfluencePrice, nameof(BaseInfluencePrice));
        OnPropertyChangedWithValue(BaseInfluencePriceText, nameof(BaseInfluencePriceText));
        OnPropertyChangedWithValue(MinimumInfluencePrice, nameof(MinimumInfluencePrice));
        OnPropertyChangedWithValue(MaximumInfluencePrice, nameof(MaximumInfluencePrice));
        OnPropertyChangedWithValue(
            InfluencePricePercentageText,
            nameof(InfluencePricePercentageText));
        _contract = GetActiveContract();
        RefreshActiveParties();
        RefreshPrioritySelector();
        OnPropertyChangedWithValue(ContractState, nameof(ContractState));
        OnPropertyChangedWithValue(ContractStart, nameof(ContractStart));
        OnPropertyChangedWithValue(ContractEnd, nameof(ContractEnd));
        OnPropertyChangedWithValue(HasActiveContract, nameof(HasActiveContract));
        OnPropertyChangedWithValue(Satisfaction, nameof(Satisfaction));
        OnPropertyChangedWithValue(EmployerSatisfaction, nameof(EmployerSatisfaction));
        OnPropertyChangedWithValue(Reliability, nameof(Reliability));
        OnPropertyChangedWithValue(DepartureRisk, nameof(DepartureRisk));
        OnPropertyChangedWithValue(EmployerTerminationRisk, nameof(EmployerTerminationRisk));
        OnPropertyChangedWithValue(Performance, nameof(Performance));
        OnPropertyChangedWithValue(CurrentActivity, nameof(CurrentActivity));
        OnPropertyChangedWithValue(RecentEarnings, nameof(RecentEarnings));
        OnPropertyChangedWithValue(AcceptanceChance, nameof(AcceptanceChance));
        OnPropertyChangedWithValue(PreferredRole, nameof(PreferredRole));
        OnPropertyChangedWithValue(HasActivePartyItems, nameof(HasActivePartyItems));
        OnPropertyChangedWithValue(
            StrategicPriorityDescription,
            nameof(StrategicPriorityDescription));
        OnPropertyChangedWithValue(
            StrategicPriorityName,
            nameof(StrategicPriorityName));
        OnPropertyChangedWithValue(
            CanEditStrategicPriority,
            nameof(CanEditStrategicPriority));
        ClanName = _clan.Name.ToString();
        LeaderName = _clan.Leader?.Name.ToString() ?? string.Empty;
        TextObject serviceStatus;

        if (_contract is not null && _contract.EmployerKingdom == _kingdom)
        {
            serviceStatus = new TextObject("{=MercenaryManagement_InService}Serving this kingdom");
        }
        else if (_contract?.EmployerKingdom is not null)
        {
            serviceStatus = new TextObject("{=MercenaryManagement_InOtherKingdom}Serving {KINGDOM}");
            serviceStatus.SetTextVariable("KINGDOM", _contract.EmployerKingdom.Name);
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
        foreach (MercenaryPartyItemVM party in ActivePartyItems) party.OnFinalize();
        ActivePartyItems.Clear();
        PrioritySelector.OnFinalize();
        Banner.OnFinalize();
        base.OnFinalize();
    }

    private void RefreshActiveParties()
    {
        foreach (MercenaryPartyItemVM party in ActivePartyItems) party.OnFinalize();
        ActivePartyItems.Clear();
        foreach (MobileParty party in _clan.WarPartyComponents
                     .Select(component => component.MobileParty)
                     .Where(party => party.IsActive && !party.IsDisbanding)
                     .OrderByDescending(party => party.MemberRoster.TotalManCount))
        {
            ActivePartyItems.Add(new MercenaryPartyItemVM(party));
        }
    }

    private static string FormatInfluence(float influence)
    {
        TextObject text = new TextObject("{INFLUENCE}");
        text.SetTextVariable("INFLUENCE", influence, 2);
        return text.ToString();
    }

    private int CalculateInfluencePricePercentage()
    {
        if (BaseInfluencePrice <= 0)
        {
            return 100;
        }

        int percentage = (int)Math.Round(
            _influencePrice * 100d / BaseInfluencePrice,
            MidpointRounding.AwayFromZero);
        return Math.Max(85, Math.Min(150, percentage));
    }

    private MercenaryContract? GetActiveContract()
    {
        MercenaryContractBehavior? contractBehavior = Campaign.Current?
            .GetCampaignBehavior<MercenaryContractBehavior>();
        return contractBehavior is not null
               && contractBehavior.GetOrCreateContract(_clan, out MercenaryContract contract)
            ? contract
            : null;
    }

    private MercenaryStrategicPriority StrategicPriority =>
        _contract?.StrategicPriority ?? MercenaryStrategicPriority.IndependentOperations;

    private void OnStrategicPrioritySelected(SelectorVM<SelectorItemVM> selector)
    {
        if (_isRefreshingPrioritySelector)
        {
            return;
        }

        MercenaryStrategicPriority selectedPriority = GetStrategicPriority(selector.SelectedIndex);
        if (!MercenaryPriorityService.TrySetStrategicPriority(
                _clan,
                _kingdom,
                selectedPriority))
        {
            RefreshPrioritySelector();
            return;
        }

        _contract = GetActiveContract();
        OnPropertyChangedWithValue(
            StrategicPriorityDescription,
            nameof(StrategicPriorityDescription));
        OnPropertyChangedWithValue(
            StrategicPriorityName,
            nameof(StrategicPriorityName));
    }

    private void RefreshPrioritySelector()
    {
        _isRefreshingPrioritySelector = true;
        PrioritySelector.Refresh(
            GetStrategicPriorityNames(),
            (int)StrategicPriority,
            OnStrategicPrioritySelected);
        _isRefreshingPrioritySelector = false;
    }

    private static TextObject[] GetStrategicPriorityNames()
    {
        return new[]
        {
            GetStrategicPriorityName(MercenaryStrategicPriority.IndependentOperations),
            GetStrategicPriorityName(MercenaryStrategicPriority.JoinArmies),
            GetStrategicPriorityName(MercenaryStrategicPriority.EconomicWarfare),
            GetStrategicPriorityName(MercenaryStrategicPriority.TerritorialDefense),
            GetStrategicPriorityName(MercenaryStrategicPriority.Recovery)
        };
    }

    private static TextObject GetStrategicPriorityName(
        MercenaryStrategicPriority strategicPriority)
    {
        return MercenaryPriorityText.GetName(strategicPriority);
    }

    private static MercenaryStrategicPriority GetStrategicPriority(int selectedIndex)
    {
        switch (selectedIndex)
        {
            case (int)MercenaryStrategicPriority.JoinArmies:
                return MercenaryStrategicPriority.JoinArmies;
            case (int)MercenaryStrategicPriority.EconomicWarfare:
                return MercenaryStrategicPriority.EconomicWarfare;
            case (int)MercenaryStrategicPriority.TerritorialDefense:
                return MercenaryStrategicPriority.TerritorialDefense;
            case (int)MercenaryStrategicPriority.Recovery:
                return MercenaryStrategicPriority.Recovery;
            default:
                return MercenaryStrategicPriority.IndependentOperations;
        }
    }

    private static TextObject GetStrategicPriorityDescription(
        MercenaryStrategicPriority strategicPriority)
    {
        return MercenaryPriorityText.GetDescription(strategicPriority);
    }

    private static string FormatPastTime(CampaignTime time)
    {
        TextObject text = new TextObject("{=MercenaryManagement_DaysAgo}{DAYS} days ago");
        text.SetTextVariable("DAYS", Math.Max(0f, time.ElapsedDaysUntilNow).ToString("0.#"));
        return text.ToString();
    }

    private static string FormatEndTime(CampaignTime time)
    {
        if (time == CampaignTime.Zero || time == CampaignTime.Never)
        {
            return new TextObject("{=MercenaryManagement_Indefinite}Indefinite").ToString();
        }

        TextObject text = time.IsPast
            ? new TextObject("{=MercenaryManagement_DaysAgo}{DAYS} days ago")
            : new TextObject("{=MercenaryManagement_DaysRemaining}{DAYS} days remaining");
        float days = time.IsPast
            ? time.ElapsedDaysUntilNow
            : time.RemainingDaysFromNow;
        text.SetTextVariable("DAYS", Math.Max(0f, days).ToString("0.#"));
        return text.ToString();
    }

    private int CalculateAcceptanceChance()
    {
        if (!IsFreeMercenaryClan || _clan.Leader is null || _kingdom.Leader is null) return 0;
        return MercenaryHiringService.CalculateAcceptanceChance(_clan, _kingdom, InfluencePrice);
    }

    private float GetReliability()
    {
        MercenaryContractBehavior? behavior = Campaign.Current?
            .GetCampaignBehavior<MercenaryContractBehavior>();
        return _contract?.Reliability ?? behavior?.GetReliability(_clan) ?? 50f;
    }

    private string GetPreferredRole()
    {
        MercenaryStrategicPriority priority = ActiveParties >= 3
            ? MercenaryStrategicPriority.EconomicWarfare
            : Strength >= 300 ? MercenaryStrategicPriority.JoinArmies
            : MercenaryStrategicPriority.IndependentOperations;
        return MercenaryPriorityText.GetName(priority).ToString();
    }
}
