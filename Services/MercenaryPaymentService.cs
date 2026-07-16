using System;
using MercenaryManagement.Behaviors;
using MercenaryManagement.Contracts;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;

namespace MercenaryManagement.Services;

internal static class MercenaryPaymentService
{
    public static int GetInfluencePrice(Clan clan)
    {
        MercenaryContractBehavior? contractBehavior = Campaign.Current?
            .GetCampaignBehavior<MercenaryContractBehavior>();
        if (contractBehavior is null)
        {
            return clan.MercenaryAwardMultiplier;
        }

        if (!contractBehavior.GetOrCreateContract(clan, out MercenaryContract contract))
        {
            return clan.MercenaryAwardMultiplier;
        }

        int influencePrice = contract.InfluencePrice;

        int clampedInfluencePrice = Math.Max(
            CalculateMinimumInfluencePrice(clan),
            Math.Min(CalculateMaximumInfluencePrice(clan), influencePrice));

        if (clampedInfluencePrice != influencePrice)
        {
            contractBehavior.SetInfluencePrice(clan, clampedInfluencePrice);
        }

        return clampedInfluencePrice;
    }

    public static int CalculateMinimumInfluencePrice(Clan clan)
    {
        return Math.Max(0, (int)Math.Ceiling(clan.MercenaryAwardMultiplier * 0.85d));
    }

    public static int CalculateMaximumInfluencePrice(Clan clan)
    {
        return Math.Max(
            CalculateMinimumInfluencePrice(clan),
            (int)Math.Floor(clan.MercenaryAwardMultiplier * 1.5d));
    }

    public static bool TrySetInfluencePrice(Clan clan, Kingdom kingdom, int influencePrice)
    {
        if (!MercenaryHiringService.IsPlayerRuler(kingdom)
            || clan.Kingdom != kingdom)
        {
            return false;
        }

        return TrySetInfluencePrice(clan, influencePrice, out _);
    }

    public static bool TrySetInfluencePrice(
        Clan clan,
        int influencePrice,
        out TextObject reason)
    {
        if (!clan.IsUnderMercenaryService || clan.Kingdom is null)
        {
            reason = new TextObject(
                "{=MercenaryManagement_RateFailedNotServing}The clan is not currently serving a kingdom as mercenaries.");
            return false;
        }

        int minimumInfluencePrice = CalculateMinimumInfluencePrice(clan);
        int maximumInfluencePrice = CalculateMaximumInfluencePrice(clan);
        if (influencePrice < minimumInfluencePrice
            || influencePrice > maximumInfluencePrice)
        {
            reason = new TextObject(
                "{=MercenaryManagement_RateFailedRange}The rate must be between {MINIMUM} and {MAXIMUM} dinars per influence.");
            reason.SetTextVariable("MINIMUM", minimumInfluencePrice);
            reason.SetTextVariable("MAXIMUM", maximumInfluencePrice);
            return false;
        }

        MercenaryContractBehavior? contractBehavior = Campaign.Current?
            .GetCampaignBehavior<MercenaryContractBehavior>();
        if (contractBehavior is null)
        {
            reason = new TextObject(
                "{=MercenaryManagement_RateFailedNoStorage}Mercenary contract storage is not available in the current campaign.");
            return false;
        }

        contractBehavior.SetInfluencePrice(clan, influencePrice);
        reason = new TextObject(string.Empty);
        return true;
    }

    public static int CalculateExpectedPayment(Clan clan)
    {
        if (!clan.IsUnderMercenaryService
            || clan.Leader is null
            || clan.Kingdom is null)
        {
            return 0;
        }

        float revenueSmoothenFraction = Campaign.Current.Models.ClanFinanceModel.RevenueSmoothenFraction();
        if (revenueSmoothenFraction <= 0f)
        {
            return 0;
        }

        // Keep this in lockstep with DefaultClanFinanceModel.AddMercenaryIncome.
        return TaleWorlds.Library.MathF.Ceiling(clan.Influence * (1f / revenueSmoothenFraction))
               * GetInfluencePrice(clan);
    }
}
