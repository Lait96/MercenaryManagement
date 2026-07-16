using System;
using System.Linq;
using MercenaryManagement.Behaviors;
using MercenaryManagement.Contracts;
using MercenaryManagement.Services;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core.ViewModelCollection.ImageIdentifiers;
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
        _contract = GetActiveContract();
        _influencePrice = MercenaryHiringService.IsFreeMercenaryClan(clan)
            ? MercenaryHiringService.CalculateHirePrice(clan, kingdom)
            : MercenaryPaymentService.GetInfluencePrice(clan);
        AccumulatedInfluence = FormatInfluence(clan.Influence);
        _expectedPayment = MercenaryPaymentService.CalculateExpectedPayment(clan);
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
    public int InfluencePrice
    {
        get => _influencePrice;
        set
        {
            if (_influencePrice == value)
            {
                return;
            }

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
    }

    [DataSourceProperty]
    public int BaseInfluencePrice => _clan.MercenaryAwardMultiplier;

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
        OnPropertyChangedWithValue(ContractState, nameof(ContractState));
        OnPropertyChangedWithValue(ContractStart, nameof(ContractStart));
        OnPropertyChangedWithValue(ContractEnd, nameof(ContractEnd));
        OnPropertyChangedWithValue(HasActiveContract, nameof(HasActiveContract));
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
        Banner.OnFinalize();
        base.OnFinalize();
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
}
