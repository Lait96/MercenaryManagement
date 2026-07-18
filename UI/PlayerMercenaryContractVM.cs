using System;
using System.Linq;
using Bannerlord.UIExtenderEx.Attributes;
using MercenaryManagement.Behaviors;
using MercenaryManagement.Contracts;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.Core;
using MercenaryManagement.Services;

namespace MercenaryManagement.UI;

internal sealed class PlayerMercenaryContractVM : ViewModel
{
    private MercenaryContract? _contract;
    private string _unavailableReason = string.Empty;
    private PlayerMercenaryOfferItemVM? _selectedOffer;
    private PlayerServiceKingdomItemVM? _selectedServiceKingdom;
    private bool _isOfferingServices;
    private bool _isShowingHistory;

    public PlayerMercenaryContractVM()
    {
        AvailableOffers = new MBBindingList<PlayerMercenaryOfferItemVM>();
        ServiceKingdoms = new MBBindingList<PlayerServiceKingdomItemVM>();
        ContractHistory = new MBBindingList<MercenaryContractHistoryItemVM>();
    }

    [DataSourceProperty] public MBBindingList<PlayerMercenaryOfferItemVM> AvailableOffers { get; }
    [DataSourceProperty] public bool HasAvailableOffers => HasNoActiveContract && AvailableOffers.Count > 0;
    [DataSourceProperty] public bool HasNoAvailableOffers => HasNoActiveContract && AvailableOffers.Count == 0;
    [DataSourceProperty] public PlayerMercenaryOfferItemVM? SelectedOffer
    {
        get => _selectedOffer;
        private set
        {
            if (_selectedOffer == value) return;
            _selectedOffer = value;
            OnPropertyChangedWithValue(value);
            OnPropertyChangedWithValue(value is not null, nameof(HasSelectedOffer));
        }
    }
    [DataSourceProperty] public bool HasSelectedOffer => SelectedOffer is not null;
    [DataSourceProperty] public MBBindingList<PlayerServiceKingdomItemVM> ServiceKingdoms { get; }
    [DataSourceProperty] public bool HasServiceKingdoms => ServiceKingdoms.Count > 0;
    [DataSourceProperty] public bool IsBrowsingOffers => HasNoActiveContract && !IsOfferingServices;
    [DataSourceProperty] public bool CanOfferServices => IsBrowsingOffers && HasServiceKingdoms;
    [DataSourceProperty] public bool ShouldShowAvailableOffers => IsBrowsingOffers && HasAvailableOffers;
    [DataSourceProperty] public bool ShouldShowNoAvailableOffers => IsBrowsingOffers && HasNoAvailableOffers;
    [DataSourceProperty]
    public bool IsOfferingServices
    {
        get => _isOfferingServices;
        private set
        {
            if (_isOfferingServices == value) return;
            _isOfferingServices = value;
            OnPropertyChangedWithValue(value);
            OnPropertyChangedWithValue(IsBrowsingOffers, nameof(IsBrowsingOffers));
            OnPropertyChangedWithValue(CanOfferServices, nameof(CanOfferServices));
            OnPropertyChangedWithValue(ShouldShowAvailableOffers, nameof(ShouldShowAvailableOffers));
            OnPropertyChangedWithValue(ShouldShowNoAvailableOffers, nameof(ShouldShowNoAvailableOffers));
        }
    }
    [DataSourceProperty]
    public PlayerServiceKingdomItemVM? SelectedServiceKingdom
    {
        get => _selectedServiceKingdom;
        private set
        {
            if (_selectedServiceKingdom == value) return;
            _selectedServiceKingdom = value;
            OnPropertyChangedWithValue(value);
            OnPropertyChangedWithValue(value is not null, nameof(HasSelectedServiceKingdom));
        }
    }
    [DataSourceProperty] public bool HasSelectedServiceKingdom => SelectedServiceKingdom is not null;
    [DataSourceProperty] public MBBindingList<MercenaryContractHistoryItemVM> ContractHistory { get; }
    [DataSourceProperty] public bool HasContractHistory => ContractHistory.Count > 0;
    [DataSourceProperty] public bool HasNoContractHistory => ContractHistory.Count == 0;
    [DataSourceProperty]
    public bool IsShowingHistory
    {
        get => _isShowingHistory;
        private set
        {
            if (_isShowingHistory == value) return;
            _isShowingHistory = value;
            OnPropertyChangedWithValue(value);
        }
    }

    [DataSourceProperty]
    public bool IsUnavailable => !string.IsNullOrEmpty(_unavailableReason);

    [DataSourceProperty]
    public bool HasActiveContract => !IsUnavailable && _contract?.IsActive == true;

    [DataSourceProperty]
    public bool HasNoActiveContract => !IsUnavailable && !HasActiveContract;

    [DataSourceProperty]
    public string UnavailableReason => _unavailableReason;

    [DataSourceProperty]
    public string EmployerKingdom => _contract?.EmployerKingdom?.Name.ToString() ?? string.Empty;

    [DataSourceProperty]
    public string EmployerRuler => _contract?.EmployerKingdom?.Leader?.Name.ToString() ?? string.Empty;

    [DataSourceProperty]
    public string ContractStart => FormatDate(_contract?.ServiceStartTime ?? CampaignTime.Zero);

    [DataSourceProperty]
    public string ContractEnd => FormatEndDate(_contract?.EndTime ?? CampaignTime.Zero);

    [DataSourceProperty]
    public string ContractAge
    {
        get
        {
            float days = _contract is null
                ? 0f
                : Math.Max(0f, _contract.ServiceStartTime.ElapsedDaysUntilNow);
            TextObject text = new TextObject("{=MercenaryManagement_ContractAgeValue}{DAYS} days");
            text.SetTextVariable("DAYS", days.ToString("0.#"));
            return text.ToString();
        }
    }

    [DataSourceProperty]
    public int InfluencePrice => _contract?.InfluencePrice ?? 0;

    [DataSourceProperty]
    public string StrategicPriority => GetStrategicPriorityName(
        _contract?.StrategicPriority ?? MercenaryStrategicPriority.IndependentOperations).ToString();

    [DataSourceProperty]
    public string StrategicPriorityDescription => GetStrategicPriorityDescription(
        _contract?.StrategicPriority ?? MercenaryStrategicPriority.IndependentOperations).ToString();

    [DataSourceProperty]
    public string EmployerTitleText => Localize("MercenaryManagement_EmployerTitle", "Employer");

    [DataSourceProperty]
    public string KingdomText => Localize("MercenaryManagement_Kingdom", "Kingdom");

    [DataSourceProperty]
    public string RulerText => Localize("MercenaryManagement_Ruler", "Ruler");

    [DataSourceProperty]
    public string ContractTitleText => Localize("MercenaryManagement_ContractTitle", "Contract");

    [DataSourceProperty]
    public string ContractStartText => Localize("MercenaryManagement_ContractStart", "Contract start");

    [DataSourceProperty]
    public string ContractEndText => Localize("MercenaryManagement_ContractEnd", "Contract end");

    [DataSourceProperty]
    public string ContractAgeText => Localize("MercenaryManagement_ContractAge", "Contract age");

    [DataSourceProperty]
    public string InfluencePriceText => Localize("MercenaryManagement_InfluencePrice", "Dinars per influence");

    [DataSourceProperty]
    public string StrategicPriorityTitleText => Localize("MercenaryManagement_StrategicPriority", "Strategic priority");

    [DataSourceProperty]
    public string NoActiveContractText => Localize(
        "MercenaryManagement_PlayerNoActiveContract",
        "Your clan is not currently serving any kingdom.");

    [DataSourceProperty] public string AvailableOffersTitleText => Localize("MercenaryManagement_AvailableOffers", "Available offers");
    [DataSourceProperty] public string AvailableOffersHintText => Localize("MercenaryManagement_AvailableOffersHint", "Select a kingdom to view its terms");
    [DataSourceProperty] public string NoAvailableOffersText => Localize("MercenaryManagement_NoAvailableOffers", "No kingdom is currently ready to hire your clan.");
    [DataSourceProperty] public string RelationToRulerText => Localize("MercenaryManagement_RelationColumn", "Relation");
    [DataSourceProperty] public string CurrentWarsText => Localize("MercenaryManagement_CurrentWars", "Current wars");
    [DataSourceProperty] public string OfferRateText => Localize("MercenaryManagement_OfferRate", "Rate");
    [DataSourceProperty] public int ExpectedPayment => MercenaryPaymentService.CalculateExpectedPayment(Clan.PlayerClan);
    [DataSourceProperty] public int Satisfaction => (int)Math.Round(_contract?.EmployerSatisfaction ?? 0f);
    [DataSourceProperty] public int EmployerTerminationRisk => (int)Math.Round(
        _contract is null ? 0f : MercenaryEvaluationService.CalculateEmployerTerminationRisk(_contract));
    [DataSourceProperty] public string Performance => _contract is null ? string.Empty : MercenaryEvaluationService.GetPerformanceSummary(_contract);
    [DataSourceProperty] public string CurrentActivity => _contract is null ? string.Empty : MercenaryEvaluationService.GetActivitySummary(_contract);
    [DataSourceProperty] public int RecentEarnings => _contract?.RecentEarnings ?? 0;
    [DataSourceProperty] public string AcceptOfferText => Localize("MercenaryManagement_AcceptOffer", "Accept offer");
    [DataSourceProperty] public string OfferServicesText => Localize("MercenaryManagement_OfferServices", "Offer services");
    [DataSourceProperty] public string OfferServicesTitleText => Localize("MercenaryManagement_OfferServicesTitle", "Offer services to a kingdom");
    [DataSourceProperty] public string OfferServicesHintText => Localize("MercenaryManagement_OfferServicesHint", "Select a kingdom that has not made an offer. Its ruler will return calculated terms or refuse the request.");
    [DataSourceProperty] public string SubmitServiceRequestText => Localize("MercenaryManagement_SubmitServiceRequest", "Send request");
    [DataSourceProperty] public string BackToOffersText => Localize("MercenaryManagement_BackToOffers", "Back to offers");
    [DataSourceProperty] public string RequestRevisionText => Localize("MercenaryManagement_RequestRevision", "Request better terms");
    [DataSourceProperty] public string EndContractText => Localize("MercenaryManagement_EndContract", "End contract");
    [DataSourceProperty] public string ExpectedPaymentText => Localize("MercenaryManagement_ExpectedPayment", "Expected payment (dinars)");
    [DataSourceProperty] public string SatisfactionText => Localize("MercenaryManagement_EmployerSatisfaction", "Employer satisfaction");
    [DataSourceProperty] public string EmployerTerminationRiskText => Localize(
        "MercenaryManagement_EmployerTerminationRisk", "Risk of employer termination");
    [DataSourceProperty] public string PerformanceText => Localize("MercenaryManagement_Performance", "Performance");
    [DataSourceProperty] public string CurrentActivityText => Localize("MercenaryManagement_CurrentActivity", "Current activity");
    [DataSourceProperty] public string RecentEarningsText => Localize("MercenaryManagement_RecentEarnings", "Earnings in the last 7 days");
    [DataSourceProperty] public string HistoryText => Localize("MercenaryManagement_ContractHistory", "Contract history");
    [DataSourceProperty] public string NoHistoryText => Localize("MercenaryManagement_NoContractHistory", "No completed contracts have been recorded.");
    [DataSourceProperty] public string HistoryEmployerText => Localize("MercenaryManagement_HistoryEmployer", "Employer");
    [DataSourceProperty] public string HistoryPeriodText => Localize("MercenaryManagement_HistoryPeriod", "Service period");
    [DataSourceProperty] public string HistoryReasonText => Localize("MercenaryManagement_HistoryReason", "Completion reason");
    [DataSourceProperty] public string HistoryResultsText => Localize("MercenaryManagement_HistoryResults", "Contribution / reputation");
    [DataSourceProperty] public string HistoryRelationsText => Localize("MercenaryManagement_HistoryRelations", "Clan / employer satisfaction; relation");

    [DataSourceMethod]
    public void ExecuteShowHistory() => IsShowingHistory = !IsShowingHistory;

    [DataSourceMethod]
    public void ExecuteAcceptOffer()
    {
        PlayerMercenaryOfferItemVM? offer = SelectedOffer;
        Clan clan = Clan.PlayerClan;
        if (offer is null)
        {
            Refresh();
            return;
        }
        if (MercenaryHiringService.TryGetPlayerOfferFailureReason(clan, offer.Kingdom, out TextObject reason))
        {
            InformationManager.DisplayMessage(new InformationMessage(reason.ToString()));
            Refresh();
            return;
        }

        bool accepted = Campaign.Current?.GetCampaignBehavior<MercenaryOfferBehavior>()?
            .TryAccept(offer.Offer) == true;
        InformationManager.DisplayMessage(new InformationMessage(accepted
            ? Localize("MercenaryManagement_OfferAccepted", "Your clan entered the kingdom's service.")
            : Localize("MercenaryManagement_OfferAcceptFailed", "The offer is no longer available.")));
        Refresh();
    }

    [DataSourceMethod]
    public void ExecuteShowOfferServices()
    {
        if (!HasNoActiveContract || !HasServiceKingdoms) return;
        IsOfferingServices = true;
    }

    [DataSourceMethod]
    public void ExecuteBackToOffers()
    {
        IsOfferingServices = false;
    }

    [DataSourceMethod]
    public void ExecuteSubmitServiceRequest()
    {
        PlayerServiceKingdomItemVM? selected = SelectedServiceKingdom;
        MercenaryOfferBehavior? behavior = Campaign.Current?
            .GetCampaignBehavior<MercenaryOfferBehavior>();
        if (selected is null || behavior is null)
        {
            Display(Localize("MercenaryManagement_ServiceRequestUnavailable", "The service request is no longer available."));
            Refresh();
            return;
        }

        MercenaryOffer? offer = behavior.RequestOffer(Clan.PlayerClan, selected.Kingdom);
        if (offer is null)
        {
            Display(Localize("MercenaryManagement_ServiceRequestDeclined", "The kingdom declined your request for service."));
        }
        else
        {
            TextObject response = new TextObject(
                "{=MercenaryManagement_ServiceRequestCounteroffer}{KINGDOM} returned an offer of {RATE} dinars per influence. Review and accept it from the offer list.");
            response.SetTextVariable("KINGDOM", selected.Kingdom.Name);
            response.SetTextVariable("RATE", offer.InfluencePrice);
            Display(response.ToString());
        }

        Refresh();
    }

    [DataSourceMethod]
    public void ExecuteRequestRevision()
    {
        if (_contract is null)
        {
            Display(Localize("MercenaryManagement_ContractActionUnavailable", "The contract is no longer active."));
            Refresh();
            return;
        }
        if (_contract.LastRevisionRequestTime != CampaignTime.Zero
            && _contract.LastRevisionRequestTime.ElapsedDaysUntilNow < 1f)
        {
            Display(Localize(
                "MercenaryManagement_RevisionCooldown",
                "Your employer will not consider another revision until one day after the previous request."));
            return;
        }
        int revisedRate = Math.Min(
            MercenaryPaymentService.CalculateMaximumInfluencePrice(Clan.PlayerClan),
            _contract.InfluencePrice + Math.Max(1, _contract.InfluencePrice / 10));
        if (revisedRate <= _contract.InfluencePrice)
        {
            Display(Localize("MercenaryManagement_RevisionAtMaximum", "Your contract already uses the maximum available rate."));
            return;
        }
        _contract.RecordRevisionRequest();
        float acceptance = 0.2f + _contract.Reliability / 200f
                           + MercenaryHiringService.CalculateMilitaryNeed(_contract.EmployerKingdom!) * 0.3f;
        acceptance += CalculateRevisionProgressModifier(_contract);
        if (MBRandom.RandomFloat < Math.Min(0.9f, acceptance))
        {
            bool changed = MercenaryPaymentService.TrySetInfluencePrice(
                Clan.PlayerClan, revisedRate, out TextObject reason);
            if (changed)
            {
                _contract.RecordSuccessfulRevision();
                Display(Localize("MercenaryManagement_RevisionAccepted", "Your employer accepted the revised rate."));
            }
            else Display(reason.ToString());
        }
        else Display(Localize(
            "MercenaryManagement_RevisionDeclined", "Your employer declined to revise the contract."));
        Refresh();
    }

    private static float CalculateRevisionProgressModifier(MercenaryContract contract)
    {
        if (contract.LastSuccessfulRevisionTime == CampaignTime.Zero) return 0f;
        int progress = contract.ContributionPoints
                       - contract.ContributionPointsAtLastSuccessfulRevision;
        if (progress < 0) return -0.3f;
        if (progress < 3) return -0.2f;
        if (progress < 10) return -0.1f;
        if (progress >= 20) return 0.1f;
        return 0f;
    }

    [DataSourceMethod]
    public void ExecuteEndContract()
    {
        if (_contract?.Clan != Clan.PlayerClan)
        {
            Display(Localize("MercenaryManagement_ContractActionUnavailable", "The contract is no longer active."));
            Refresh();
            return;
        }

        InformationManager.ShowInquiry(new InquiryData(
            Localize("MercenaryManagement_EndContractTitle", "End mercenary contract"),
            Localize("MercenaryManagement_EndContractConfirmation", "End your clan's current mercenary contract?"),
            true, true,
            Localize("MercenaryManagement_EndContract", "End contract"),
            Localize("MercenaryManagement_Cancel", "Cancel"),
            ConfirmEndContract,
            null), true);
    }

    private void ConfirmEndContract()
    {
        MercenaryContract? contract = _contract;
        if (contract?.Clan != Clan.PlayerClan) return;
        if (contract.EndTime != CampaignTime.Zero && !contract.EndTime.IsPast)
            contract.AdjustEvaluation(-10f, -8f);
        MercenaryDismissalService.EndService(Clan.PlayerClan,
            MercenaryContractEndReason.MercenaryLeft);
        bool ended = !Clan.PlayerClan.IsUnderMercenaryService || Clan.PlayerClan.Kingdom is null;
        Display(ended
            ? Localize("MercenaryManagement_PlayerContractEnded", "Your mercenary contract has ended.")
            : Localize("MercenaryManagement_PlayerContractEndFailed", "The contract could not be ended in the current situation."));
        Refresh();
    }

    private static void Display(string text)
    {
        InformationManager.DisplayMessage(new InformationMessage(text));
    }

    public void Refresh()
    {
        Clan? clan = Clan.PlayerClan;
        _contract = null;
        _unavailableReason = GetUnavailableReason(clan);
        ClearOffers();
        ClearServiceKingdoms();
        ContractHistory.Clear();
        IsOfferingServices = false;

        MercenaryContractBehavior? behavior = Campaign.Current?
            .GetCampaignBehavior<MercenaryContractBehavior>();
        if (clan is not null)
            foreach (MercenaryContractHistoryEntry entry in
                     (behavior?.GetHistory(clan) ?? Array.Empty<MercenaryContractHistoryEntry>())
                     .OrderByDescending(entry => entry.ServiceEndTime.ToDays))
                ContractHistory.Add(new MercenaryContractHistoryItemVM(entry));

        if (string.IsNullOrEmpty(_unavailableReason) && clan is not null)
        {
            if (behavior?.GetOrCreateContract(clan, out MercenaryContract contract) == true)
            {
                _contract = contract;
            }

            if (_contract?.IsActive != true)
            {
                MercenaryOfferBehavior? offerBehavior = Campaign.Current?
                    .GetCampaignBehavior<MercenaryOfferBehavior>();
                offerBehavior?.RefreshPlayerOffers();
                foreach (MercenaryOffer offer in (offerBehavior?.GetOffersFor(clan)
                             ?? Enumerable.Empty<MercenaryOffer>())
                             .OrderByDescending(offer => offer.InfluencePrice)
                             .ThenBy(offer => offer.Kingdom?.Name.ToString()))
                {
                    AvailableOffers.Add(new PlayerMercenaryOfferItemVM(offer, SelectOffer));
                }

                if (AvailableOffers.Count > 0) SelectOffer(AvailableOffers[0]);

                foreach (Kingdom kingdom in (offerBehavior?.GetServiceRequestKingdoms(clan)
                             ?? Enumerable.Empty<Kingdom>())
                         .OrderBy(kingdom => kingdom.Name.ToString()))
                {
                    ServiceKingdoms.Add(new PlayerServiceKingdomItemVM(
                        kingdom,
                        SelectServiceKingdom));
                }
                if (ServiceKingdoms.Count > 0) SelectServiceKingdom(ServiceKingdoms[0]);
            }
        }

        NotifyAllProperties();
    }

    public override void RefreshValues()
    {
        base.RefreshValues();
        Refresh();
    }

    private void NotifyAllProperties()
    {
        OnPropertyChangedWithValue(IsUnavailable, nameof(IsUnavailable));
        OnPropertyChangedWithValue(HasActiveContract, nameof(HasActiveContract));
        OnPropertyChangedWithValue(HasNoActiveContract, nameof(HasNoActiveContract));
        OnPropertyChangedWithValue(HasAvailableOffers, nameof(HasAvailableOffers));
        OnPropertyChangedWithValue(HasNoAvailableOffers, nameof(HasNoAvailableOffers));
        OnPropertyChangedWithValue(HasServiceKingdoms, nameof(HasServiceKingdoms));
        OnPropertyChangedWithValue(IsBrowsingOffers, nameof(IsBrowsingOffers));
        OnPropertyChangedWithValue(CanOfferServices, nameof(CanOfferServices));
        OnPropertyChangedWithValue(ShouldShowAvailableOffers, nameof(ShouldShowAvailableOffers));
        OnPropertyChangedWithValue(ShouldShowNoAvailableOffers, nameof(ShouldShowNoAvailableOffers));
        OnPropertyChangedWithValue(UnavailableReason, nameof(UnavailableReason));
        OnPropertyChangedWithValue(EmployerKingdom, nameof(EmployerKingdom));
        OnPropertyChangedWithValue(EmployerRuler, nameof(EmployerRuler));
        OnPropertyChangedWithValue(ContractStart, nameof(ContractStart));
        OnPropertyChangedWithValue(ContractEnd, nameof(ContractEnd));
        OnPropertyChangedWithValue(ContractAge, nameof(ContractAge));
        OnPropertyChangedWithValue(InfluencePrice, nameof(InfluencePrice));
        OnPropertyChangedWithValue(StrategicPriority, nameof(StrategicPriority));
        OnPropertyChangedWithValue(StrategicPriorityDescription, nameof(StrategicPriorityDescription));
        OnPropertyChangedWithValue(ExpectedPayment, nameof(ExpectedPayment));
        OnPropertyChangedWithValue(Satisfaction, nameof(Satisfaction));
        OnPropertyChangedWithValue(EmployerTerminationRisk, nameof(EmployerTerminationRisk));
        OnPropertyChangedWithValue(Performance, nameof(Performance));
        OnPropertyChangedWithValue(CurrentActivity, nameof(CurrentActivity));
        OnPropertyChangedWithValue(RecentEarnings, nameof(RecentEarnings));
        OnPropertyChangedWithValue(HasContractHistory, nameof(HasContractHistory));
        OnPropertyChangedWithValue(HasNoContractHistory, nameof(HasNoContractHistory));
    }

    public override void OnFinalize()
    {
        ClearOffers();
        ClearServiceKingdoms();
        base.OnFinalize();
    }

    private void SelectOffer(PlayerMercenaryOfferItemVM offer)
    {
        if (SelectedOffer == offer) return;
        if (SelectedOffer is not null) SelectedOffer.IsSelected = false;
        SelectedOffer = offer;
        offer.IsSelected = true;
    }

    private void ClearOffers()
    {
        SelectedOffer = null;
        foreach (PlayerMercenaryOfferItemVM offer in AvailableOffers) offer.OnFinalize();
        AvailableOffers.Clear();
    }

    private void SelectServiceKingdom(PlayerServiceKingdomItemVM kingdom)
    {
        if (SelectedServiceKingdom == kingdom) return;
        if (SelectedServiceKingdom is not null) SelectedServiceKingdom.IsSelected = false;
        SelectedServiceKingdom = kingdom;
        kingdom.IsSelected = true;
    }

    private void ClearServiceKingdoms()
    {
        SelectedServiceKingdom = null;
        foreach (PlayerServiceKingdomItemVM kingdom in ServiceKingdoms) kingdom.OnFinalize();
        ServiceKingdoms.Clear();
    }

    private static string GetUnavailableReason(Clan? clan)
    {
        if (clan?.Kingdom is not null
            && !clan.IsUnderMercenaryService
            && clan.Kingdom.Leader != Hero.MainHero)
        {
            return Localize(
                "MercenaryManagement_PlayerContractUnavailableVassal",
                "A vassal cannot serve as a mercenary.");
        }

        if (clan?.Kingdom?.Leader == Hero.MainHero)
        {
            return Localize(
                "MercenaryManagement_PlayerContractUnavailableRuler",
                "A kingdom ruler cannot serve as a mercenary.");
        }

        if (clan is not null && clan.Fiefs.Count > 0)
        {
            return Localize(
                "MercenaryManagement_PlayerContractUnavailableFiefs",
                "Mercenary service is unavailable while your clan owns settlements.");
        }

        return string.Empty;
    }

    private static string FormatDate(CampaignTime time)
    {
        return time == CampaignTime.Zero ? string.Empty : time.ToString();
    }

    private static string FormatEndDate(CampaignTime time)
    {
        return time == CampaignTime.Zero || time == CampaignTime.Never
            ? Localize("MercenaryManagement_Indefinite", "Indefinite")
            : time.ToString();
    }

    private static TextObject GetStrategicPriorityName(MercenaryStrategicPriority priority)
    {
        return MercenaryPriorityText.GetName(priority);
    }

    private static TextObject GetStrategicPriorityDescription(MercenaryStrategicPriority priority)
    {
        return MercenaryPriorityText.GetDescription(priority);
    }

    private static string Localize(string id, string fallback) =>
        new TextObject($"{{={id}}}{fallback}").ToString();
}
