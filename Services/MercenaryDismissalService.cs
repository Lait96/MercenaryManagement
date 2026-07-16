using MercenaryManagement.Behaviors;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Localization;

namespace MercenaryManagement.Services;

internal static class MercenaryDismissalService
{
    private const int DismissalRelationPenalty = -2;

    public static bool IsServingKingdom(Clan clan, Kingdom kingdom)
    {
        return clan.IsUnderMercenaryService && clan.Kingdom == kingdom;
    }

    public static int CalculateOutstandingPayment(Clan clan)
    {
        return MercenaryPaymentService.CalculateExpectedPayment(clan);
    }

    public static bool TryGetFailureReason(Clan clan, Kingdom? kingdom, out TextObject reason)
    {
        if (kingdom is null || kingdom.IsEliminated)
        {
            reason = new TextObject(
                "{=MercenaryManagement_DismissFailedNoKingdom}The kingdom is no longer available.");
            return true;
        }

        if (!MercenaryHiringService.IsPlayerRuler(kingdom))
        {
            reason = new TextObject(
                "{=MercenaryManagement_DismissFailedNotRuler}Only the ruler of the kingdom can dismiss mercenary clans.");
            return true;
        }

        if (!IsServingKingdom(clan, kingdom))
        {
            reason = new TextObject(
                "{=MercenaryManagement_DismissFailedNotServing}This mercenary clan is no longer serving your kingdom.");
            return true;
        }

        if (clan.Leader is null || !clan.Leader.IsAlive)
        {
            reason = new TextObject(
                "{=MercenaryManagement_DismissFailedNoLeader}This clan has no available leader with whom to settle the contract.");
            return true;
        }

        int outstandingPayment = CalculateOutstandingPayment(clan);
        if (Hero.MainHero.Gold < outstandingPayment)
        {
            TextObject insufficientFunds = new TextObject(
                "{=MercenaryManagement_DismissFailedGold}You need {GOLD} denars to settle the clan's outstanding payment before dismissal.");
            insufficientFunds.SetTextVariable("GOLD", outstandingPayment);
            reason = insufficientFunds;
            return true;
        }

        reason = new TextObject(string.Empty);
        return false;
    }

    public static void Dismiss(Clan clan)
    {
        int outstandingPayment = CalculateOutstandingPayment(clan);
        if (outstandingPayment > 0)
        {
            GiveGoldAction.ApplyBetweenCharacters(
                Hero.MainHero,
                clan.Leader,
                outstandingPayment,
                false);
            ChangeClanInfluenceAction.Apply(clan, -clan.Influence);
        }

        ChangeRelationAction.ApplyPlayerRelation(
            clan.Leader,
            DismissalRelationPenalty,
            true,
            true);

        EndService(clan);
    }

    public static void EndService(Clan clan)
    {
        ChangeKingdomAction.ApplyByLeaveKingdomAsMercenary(clan, true);

        MercenaryContractBehavior? contractBehavior = Campaign.Current?
            .GetCampaignBehavior<MercenaryContractBehavior>();
        if (!clan.IsUnderMercenaryService || clan.Kingdom is null)
        {
            contractBehavior?.CompleteContract(clan);
        }
    }
}
