using System;
using MercenaryManagement.Behaviors;
using TaleWorlds.CampaignSystem;

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

        if (!contractBehavior.TryGetInfluencePrice(clan, out int influencePrice))
        {
            return clan.MercenaryAwardMultiplier;
        }

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
        if (influencePrice < CalculateMinimumInfluencePrice(clan)
            || influencePrice > CalculateMaximumInfluencePrice(clan)
            || !MercenaryHiringService.IsPlayerRuler(kingdom)
            || !clan.IsUnderMercenaryService
            || clan.Kingdom != kingdom)
        {
            return false;
        }

        MercenaryContractBehavior? contractBehavior = Campaign.Current?
            .GetCampaignBehavior<MercenaryContractBehavior>();
        if (contractBehavior is null)
        {
            return false;
        }

        contractBehavior.SetInfluencePrice(clan, influencePrice);
        return true;
    }

    public static void BeginContract(Clan clan)
    {
        Campaign.Current?
            .GetCampaignBehavior<MercenaryContractBehavior>()?
            .BeginContract(clan);
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
