using MercenaryManagement.Behaviors;
using MercenaryManagement.Contracts;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Localization;

namespace MercenaryManagement.Services;

internal static class MercenaryHiringService
{
    private const int MaximumHiredMercenaryClans = 3;
    private const int MaximumKingdomDebt = 1000;
    private const int RequiredGoldInDailyWages = 20;

    public static bool IsPlayerRuler(Kingdom kingdom)
    {
        return kingdom.Leader == Hero.MainHero
               && Clan.PlayerClan.Kingdom == kingdom
               && !Clan.PlayerClan.IsUnderMercenaryService;
    }

    public static bool IsFreeMercenaryClan(Clan clan)
    {
        return clan.IsMinorFaction
               && !clan.IsBanditFaction
               && !clan.IsEliminated
               && !clan.IsUnderMercenaryService
               && clan.Kingdom is null;
    }

    public static bool TryGetFailureReason(Clan clan, Kingdom? kingdom, out TextObject reason)
    {
        if (kingdom is null || kingdom.IsEliminated)
        {
            reason = new TextObject(
                "{=MercenaryManagement_HireFailedNoKingdom}The kingdom is no longer available.");
            return true;
        }

        if (!IsPlayerRuler(kingdom))
        {
            reason = new TextObject(
                "{=MercenaryManagement_HireFailedNotRuler}Only the ruler of the kingdom can hire mercenary clans.");
            return true;
        }

        if (TryGetInherentFailureReason(clan, kingdom, out reason))
        {
            return true;
        }

        return TryGetEmployerFailureReason(clan, kingdom, out reason);
    }

    public static bool TryGetEmployerFailureReason(Clan clan, Kingdom kingdom, out TextObject reason)
    {
        int hiredMercenaryClans = 0;
        foreach (Clan kingdomClan in kingdom.Clans)
        {
            if (kingdomClan.IsUnderMercenaryService)
            {
                hiredMercenaryClans++;
            }
        }

        if (hiredMercenaryClans >= MaximumHiredMercenaryClans)
        {
            reason = new TextObject(
                "{=MercenaryManagement_HireFailedLimit}Your kingdom already employs the maximum of three mercenary clans.");
            return true;
        }

        if (clan.Leader!.GetRelation(kingdom.Leader) <= -10)
        {
            reason = new TextObject(
                "{=MercenaryManagement_HireFailedRelation}The clan leader's relation with the ruler is too poor to accept service.");
            return true;
        }

        int awardFactor = CalculateHirePrice(clan, kingdom);
        if (Hero.MainHero.Gold < awardFactor * RequiredGoldInDailyWages)
        {
            reason = new TextObject(
                "{=MercenaryManagement_HireFailedGold}The ruler does not have enough dinars to guarantee the clan's initial wages.");
            return true;
        }

        if (Clan.PlayerClan.DebtToKingdom > MaximumKingdomDebt)
        {
            reason = new TextObject(
                "{=MercenaryManagement_HireFailedDebt}The ruler's clan has too much unpaid debt to the kingdom.");
            return true;
        }

        reason = new TextObject(string.Empty);
        return false;
    }

    public static bool TryGetInherentFailureReason(Clan clan, Kingdom kingdom, out TextObject reason)
    {
        if (!clan.IsMinorFaction || clan.IsBanditFaction)
        {
            reason = new TextObject(
                "{=MercenaryManagement_HireFailedNotMercenary}This clan is not an eligible mercenary clan.");
            return true;
        }

        if (clan.IsEliminated)
        {
            reason = new TextObject(
                "{=MercenaryManagement_HireFailedEliminated}This mercenary clan has been eliminated.");
            return true;
        }

        if (clan.IsUnderMercenaryService)
        {
            reason = clan.Kingdom == kingdom
                ? new TextObject(
                    "{=MercenaryManagement_HireFailedAlreadyServing}This mercenary clan is already serving the specified kingdom.")
                : new TextObject(
                    "{=MercenaryManagement_HireFailedServingOther}This mercenary clan is already serving another kingdom.");
            return true;
        }

        if (clan.Kingdom is not null)
        {
            reason = new TextObject(
                "{=MercenaryManagement_HireFailedKingdomMember}This clan is already a non-mercenary member of a kingdom.");
            return true;
        }

        if (TryGetLeaderFailureReason(clan, out reason))
        {
            return true;
        }

        if (FactionManager.IsAtWarAgainstFaction(clan.MapFaction, kingdom))
        {
            reason = new TextObject(
                "{=MercenaryManagement_HireFailedAtWar}The clan cannot be hired while it is at war with your kingdom.");
            return true;
        }

        reason = new TextObject(string.Empty);
        return false;
    }

    public static bool TryGetLeaderFailureReason(Clan clan, out TextObject reason)
    {
        if (clan.Leader is null || !clan.Leader.IsAlive)
        {
            reason = new TextObject(
                "{=MercenaryManagement_HireFailedNoLeader}This clan has no available leader who can enter into service.");
            return true;
        }

        if (clan.Leader.IsPrisoner)
        {
            reason = new TextObject(
                "{=MercenaryManagement_HireFailedLeaderPrisoner}The clan leader is a prisoner and cannot enter into service.");
            return true;
        }

        reason = new TextObject(string.Empty);
        return false;
    }

    public static int CalculateHirePrice(Clan clan, Kingdom kingdom)
    {
        return Campaign.Current.Models.MinorFactionsModel
            .GetMercenaryAwardFactorToJoinKingdom(clan, kingdom, true);
    }

    public static bool TryGetPlayerOfferFailureReason(Clan clan, Kingdom kingdom, out TextObject reason)
    {
        if (kingdom.IsEliminated || kingdom.Leader is null || !kingdom.Leader.IsAlive)
        {
            reason = new TextObject("{=MercenaryManagement_OfferKingdomUnavailable}The kingdom has no ruler available to make an offer.");
            return true;
        }

        if (TryGetLeaderFailureReason(clan, out reason))
        {
            return true;
        }

        if (FactionManager.IsAtWarAgainstFaction(clan.MapFaction, kingdom))
        {
            reason = new TextObject("{=MercenaryManagement_HireFailedAtWar}The clan cannot be hired while it is at war with this kingdom.");
            return true;
        }

        int hiredMercenaryClans = kingdom.Clans.Count(kingdomClan => kingdomClan.IsUnderMercenaryService);
        if (hiredMercenaryClans >= MaximumHiredMercenaryClans)
        {
            reason = new TextObject("{=MercenaryManagement_HireFailedLimit}The kingdom already employs the maximum of three mercenary clans.");
            return true;
        }

        if (clan.Leader!.GetRelation(kingdom.Leader) <= -10)
        {
            reason = new TextObject("{=MercenaryManagement_HireFailedRelation}The clan leader's relation with the ruler is too poor to accept service.");
            return true;
        }

        int awardFactor = CalculateHirePrice(clan, kingdom);
        if (kingdom.Leader.Gold < awardFactor * RequiredGoldInDailyWages)
        {
            reason = new TextObject("{=MercenaryManagement_OfferInsufficientFunds}The ruler cannot guarantee the clan's initial wages.");
            return true;
        }

        if (kingdom.RulingClan is not null && kingdom.RulingClan.DebtToKingdom > MaximumKingdomDebt)
        {
            reason = new TextObject("{=MercenaryManagement_HireFailedDebt}The ruling clan has too much unpaid debt to the kingdom.");
            return true;
        }

        reason = new TextObject(string.Empty);
        return false;
    }

    public static MercenaryStrategicPriority GetSuggestedPriority(Kingdom kingdom)
    {
        Kingdom[] enemies = Kingdom.All
            .Where(other => !other.IsEliminated
                            && other != kingdom
                            && FactionManager.IsAtWarAgainstFaction(kingdom, other))
            .ToArray();
        if (enemies.Length == 0)
        {
            return MercenaryStrategicPriority.IndependentOperations;
        }

        float enemyStrength = enemies.Sum(enemy => enemy.CurrentTotalStrength);
        if (kingdom.CurrentTotalStrength < enemyStrength * 0.75f)
        {
            return MercenaryStrategicPriority.TerritorialDefense;
        }

        if (enemies.Length > 1)
        {
            return MercenaryStrategicPriority.JoinArmies;
        }

        return kingdom.CurrentTotalStrength < enemyStrength
            ? MercenaryStrategicPriority.EconomicWarfare
            : MercenaryStrategicPriority.JoinArmies;
    }

    public static void Hire(Clan clan, Kingdom kingdom)
    {
        int awardFactor = CalculateHirePrice(clan, kingdom);
        CampaignTime serviceCommitment = CampaignTime.Zero;

        // The current contract scenario follows Bannerlord's indefinite mercenary service.
        ChangeKingdomAction.ApplyByJoinFactionAsMercenary(
            clan,
            kingdom,
            serviceCommitment,
            awardFactor,
            true);

        MercenaryContractBehavior? contractBehavior = Campaign.Current?
            .GetCampaignBehavior<MercenaryContractBehavior>();
        if (contractBehavior is not null)
        {
            contractBehavior.StartContract(
                clan,
                kingdom,
                awardFactor,
                serviceCommitment,
                out _);
        }
    }
}
