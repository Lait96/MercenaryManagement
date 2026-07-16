using System;
using Bannerlord.UIExtenderEx.Attributes;
using MercenaryManagement.Behaviors;
using MercenaryManagement.Contracts;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace MercenaryManagement.UI;

internal sealed class PlayerMercenaryContractVM : ViewModel
{
    private MercenaryContract? _contract;
    private string _unavailableReason = string.Empty;

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
    public string InfluencePriceText => Localize("MercenaryManagement_InfluencePrice", "Denars per influence");

    [DataSourceProperty]
    public string StrategicPriorityTitleText => Localize("MercenaryManagement_StrategicPriority", "Strategic priority");

    [DataSourceProperty]
    public string NoActiveContractText => Localize(
        "MercenaryManagement_PlayerNoActiveContract",
        "Your clan is not currently serving any kingdom.");

    public void Refresh()
    {
        Clan? clan = Clan.PlayerClan;
        _contract = null;
        _unavailableReason = GetUnavailableReason(clan);

        if (string.IsNullOrEmpty(_unavailableReason) && clan is not null)
        {
            MercenaryContractBehavior? behavior = Campaign.Current?
                .GetCampaignBehavior<MercenaryContractBehavior>();
            if (behavior?.GetOrCreateContract(clan, out MercenaryContract contract) == true)
            {
                _contract = contract;
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
        OnPropertyChangedWithValue(UnavailableReason, nameof(UnavailableReason));
        OnPropertyChangedWithValue(EmployerKingdom, nameof(EmployerKingdom));
        OnPropertyChangedWithValue(EmployerRuler, nameof(EmployerRuler));
        OnPropertyChangedWithValue(ContractStart, nameof(ContractStart));
        OnPropertyChangedWithValue(ContractEnd, nameof(ContractEnd));
        OnPropertyChangedWithValue(ContractAge, nameof(ContractAge));
        OnPropertyChangedWithValue(InfluencePrice, nameof(InfluencePrice));
        OnPropertyChangedWithValue(StrategicPriority, nameof(StrategicPriority));
        OnPropertyChangedWithValue(StrategicPriorityDescription, nameof(StrategicPriorityDescription));
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
