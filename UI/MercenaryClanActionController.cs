using System;
using MercenaryManagement.Services;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace MercenaryManagement.UI;

internal static class MercenaryClanActionController
{
    public static void ShowHire(Clan clan, Kingdom? kingdom, Action<Kingdom> refresh)
    {
        if (MercenaryHiringService.TryGetFailureReason(clan, kingdom, out TextObject reason))
        {
            Display(reason);
            return;
        }

        int price = MercenaryHiringService.CalculateHirePrice(clan, kingdom!);
        TextObject confirmation = new TextObject("{=MercenaryManagement_HireConfirmation}Hire the {CLAN} clan for {PRICE} denars per influence point?");
        confirmation.SetTextVariable("CLAN", clan.Name);
        confirmation.SetTextVariable("PRICE", price);
        ShowConfirmation("{=MercenaryManagement_HireConfirmationTitle}Hire mercenaries", confirmation,
            "{=MercenaryManagement_ConfirmHire}Hire", () => Hire(clan, kingdom!, refresh));
    }

    public static void ShowDismiss(Clan clan, Kingdom? kingdom, Action<Kingdom> refresh)
    {
        if (MercenaryDismissalService.TryGetFailureReason(clan, kingdom, out TextObject reason))
        {
            Display(reason);
            return;
        }

        int payment = MercenaryDismissalService.CalculateOutstandingPayment(clan);
        TextObject confirmation = payment > 0
            ? new TextObject("{=MercenaryManagement_DismissConfirmationWithPayment}Dismiss the {CLAN} clan and pay the outstanding {GOLD} denars?")
            : new TextObject("{=MercenaryManagement_DismissConfirmation}Dismiss the {CLAN} clan from your kingdom's service?");
        confirmation.SetTextVariable("CLAN", clan.Name);
        confirmation.SetTextVariable("GOLD", payment);
        ShowConfirmation("{=MercenaryManagement_DismissConfirmationTitle}Dismiss mercenaries", confirmation,
            "{=MercenaryManagement_ConfirmDismiss}Dismiss", () => Dismiss(clan, kingdom!, refresh));
    }

    public static void ShowPoach(Clan clan, Kingdom? targetKingdom, Action<Kingdom> refresh)
    {
        if (MercenaryPoachingService.TryGetFailureReason(clan, targetKingdom, out TextObject reason))
        {
            Display(reason);
            return;
        }

        Kingdom currentKingdom = clan.Kingdom!;
        int price = MercenaryHiringService.CalculateHirePrice(clan, targetKingdom!);
        TextObject confirmation = new TextObject("{=MercenaryManagement_PoachConfirmation}Persuade the {CLAN} clan to leave {CURRENT_KINGDOM} and enter your kingdom's service for {PRICE} denars per influence point?");
        confirmation.SetTextVariable("CLAN", clan.Name);
        confirmation.SetTextVariable("CURRENT_KINGDOM", currentKingdom.Name);
        confirmation.SetTextVariable("PRICE", price);
        ShowConfirmation("{=MercenaryManagement_PoachConfirmationTitle}Recruit serving mercenaries", confirmation,
            "{=MercenaryManagement_ConfirmPoach}Recruit",
            () => Poach(clan, currentKingdom, targetKingdom!, refresh));
    }

    private static void Hire(Clan clan, Kingdom kingdom, Action<Kingdom> refresh)
    {
        if (MercenaryHiringService.TryGetFailureReason(clan, kingdom, out TextObject reason))
        {
            Display(reason);
            refresh(kingdom);
            return;
        }

        MercenaryHiringService.Hire(clan, kingdom);
        TextObject result = clan.IsUnderMercenaryService && clan.Kingdom == kingdom
            ? new TextObject("{=MercenaryManagement_HireSucceeded}The {CLAN} clan has entered your kingdom's service.")
            : new TextObject("{=MercenaryManagement_HireFailedUnknown}The clan could not enter your kingdom's service.");
        result.SetTextVariable("CLAN", clan.Name);
        Display(result);
        refresh(kingdom);
    }

    private static void Dismiss(Clan clan, Kingdom kingdom, Action<Kingdom> refresh)
    {
        if (MercenaryDismissalService.TryGetFailureReason(clan, kingdom, out TextObject reason))
        {
            Display(reason);
            refresh(kingdom);
            return;
        }

        MercenaryDismissalService.Dismiss(clan);
        TextObject result = !clan.IsUnderMercenaryService && clan.Kingdom is null
            ? new TextObject("{=MercenaryManagement_DismissSucceeded}The {CLAN} clan has left your kingdom's service.")
            : new TextObject("{=MercenaryManagement_DismissFailedUnknown}The clan could not be dismissed from your kingdom's service.");
        result.SetTextVariable("CLAN", clan.Name);
        Display(result);
        refresh(kingdom);
    }

    private static void Poach(Clan clan, Kingdom expectedKingdom, Kingdom targetKingdom, Action<Kingdom> refresh)
    {
        if (clan.Kingdom != expectedKingdom)
        {
            Display(new TextObject("{=MercenaryManagement_PoachFailedEmployerChanged}The clan's current employer has changed. Select the clan again to make a new offer."));
            refresh(targetKingdom);
            return;
        }

        if (MercenaryPoachingService.TryGetFailureReason(clan, targetKingdom, out TextObject reason))
        {
            Display(reason);
            refresh(targetKingdom);
            return;
        }

        MercenaryPoachingService.Poach(clan, targetKingdom);
        TextObject result = clan.IsUnderMercenaryService && clan.Kingdom == targetKingdom
            ? new TextObject("{=MercenaryManagement_PoachSucceeded}The {CLAN} clan has left its former employer and entered your kingdom's service.")
            : new TextObject("{=MercenaryManagement_PoachFailedUnknown}The clan could not switch to your kingdom's service.");
        result.SetTextVariable("CLAN", clan.Name);
        Display(result);
        refresh(targetKingdom);
    }

    private static void ShowConfirmation(string title, TextObject text, string confirm, Action action)
    {
        InformationManager.ShowInquiry(new InquiryData(new TextObject(title).ToString(), text.ToString(), true, true,
            new TextObject(confirm).ToString(), new TextObject("{=MercenaryManagement_Cancel}Cancel").ToString(), action, null), true);
    }

    private static void Display(TextObject text)
    {
        InformationManager.DisplayMessage(new InformationMessage(text.ToString()));
    }
}
