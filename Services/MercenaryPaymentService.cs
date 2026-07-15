using TaleWorlds.CampaignSystem;

namespace MercenaryManagement.Services;

internal static class MercenaryPaymentService
{
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
               * clan.MercenaryAwardMultiplier;
    }
}
