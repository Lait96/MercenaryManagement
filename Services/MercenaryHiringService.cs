using MercenaryManagement.Behaviors;
using MercenaryManagement.Contracts;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Localization;

namespace MercenaryManagement.Services;

internal static class MercenaryHiringService
{
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

        if (!MercenaryHiringLimitPolicy.CanHireAnother(hiredMercenaryClans, MercenaryHiringLimitPolicy.MaximumHiredMercenaryClans))
        {
            reason = new TextObject(
                "{=MercenaryManagement_HireFailedLimit}Your kingdom already employs the maximum of {MAXIMUM} mercenary clans.");
            reason.SetTextVariable("MAXIMUM", MercenaryHiringLimitPolicy.MaximumHiredMercenaryClans);
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
        int nativePrice = Campaign.Current.Models.MinorFactionsModel
            .GetMercenaryAwardFactorToJoinKingdom(clan, kingdom, true);
        return nativePrice > 0
            ? nativePrice
            : MercenaryPaymentService.GetBaseInfluencePrice(clan);
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
        if (!MercenaryHiringLimitPolicy.CanHireAnother(hiredMercenaryClans, MercenaryHiringLimitPolicy.MaximumHiredMercenaryClans))
        {
            reason = new TextObject("{=MercenaryManagement_HireFailedLimit}The kingdom already employs the maximum of {MAXIMUM} mercenary clans.");
            reason.SetTextVariable("MAXIMUM", MercenaryHiringLimitPolicy.MaximumHiredMercenaryClans);
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
        return GetSuggestedPriority(kingdom, kingdom.Armies.Count > 0);
    }

    internal static MercenaryStrategicPriority GetSuggestedPriority(
        Kingdom kingdom,
        bool allowArmyPriority)
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
            return allowArmyPriority
                ? MercenaryStrategicPriority.JoinArmies
                : kingdom.CurrentTotalStrength < enemyStrength
                    ? MercenaryStrategicPriority.TerritorialDefense
                    : MercenaryStrategicPriority.EconomicWarfare;
        }

        return kingdom.CurrentTotalStrength < enemyStrength
            ? MercenaryStrategicPriority.EconomicWarfare
            : allowArmyPriority
                ? MercenaryStrategicPriority.JoinArmies
                : MercenaryStrategicPriority.EconomicWarfare;
    }

    public static bool CanAiKingdomHire(Clan clan, Kingdom kingdom)
    {
        if (!MercenaryAiControlPolicy.CanMakeAutonomousContractDecision(clan == Clan.PlayerClan)
            || kingdom.IsEliminated
            || kingdom.Leader is null
            || !kingdom.Leader.IsAlive
            || TryGetInherentFailureReason(clan, kingdom, out _))
        {
            return false;
        }

        int hiredCount = kingdom.Clans.Count(candidate => candidate.IsUnderMercenaryService);
        int price = CalculateHirePrice(clan, kingdom);
        return MercenaryHiringLimitPolicy.CanHireAnother(hiredCount, MercenaryHiringLimitPolicy.MaximumHiredMercenaryClans)
               && clan.Leader!.GetRelation(kingdom.Leader) > -10
               && kingdom.Leader.Gold >= price * RequiredGoldInDailyWages
               && (kingdom.RulingClan is null || kingdom.RulingClan.DebtToKingdom <= MaximumKingdomDebt);
    }

    public static float CalculateMilitaryNeed(Kingdom kingdom)
    {
        Kingdom[] enemies = Kingdom.All.Where(other => other != kingdom
            && !other.IsEliminated
            && FactionManager.IsAtWarAgainstFaction(kingdom, other)).ToArray();
        if (enemies.Length == 0) return 0f;
        float enemyStrength = enemies.Sum(enemy => enemy.CurrentTotalStrength);
        float ratio = enemyStrength / System.Math.Max(1f, kingdom.CurrentTotalStrength);
        return System.Math.Min(1f, 0.25f * enemies.Length + ratio * 0.5f);
    }

    public static int CalculateAcceptanceChance(Clan clan, Kingdom kingdom, int offeredRate)
    {
        int relation = clan.Leader?.GetRelation(kingdom.Leader) ?? 0;
        int baseRate = MercenaryPaymentService.GetBaseInfluencePrice(clan);
        int rateBonus = (offeredRate - baseRate) * 50 / baseRate;
        MercenaryContractBehavior? behavior = Campaign.Current?
            .GetCampaignBehavior<MercenaryContractBehavior>();
        int reliabilityBonus = (int)System.Math.Round(((behavior?.GetReputation(clan, kingdom) ?? 50f) - 50f) * 0.3f);
        int baseChance = System.Math.Max(5,
            System.Math.Min(95, 55 + relation + rateBonus + reliabilityBonus));
        return IsPlayerRuler(kingdom)
            ? MercenaryPlayerRecruitmentPolicy.ApplyPlayerRulerAcceptanceAdvantage(baseChance)
            : baseChance;
    }

    public static void Hire(Clan clan, Kingdom kingdom)
    {
        Hire(clan, kingdom, CalculateHirePrice(clan, kingdom),
            MercenaryContractDuration.Days60,
            MercenaryStrategicPriority.IndependentOperations);
    }

    public static void Hire(Clan clan, Kingdom kingdom, MercenaryOffer offer)
    {
        Hire(clan, kingdom, offer.InfluencePrice, offer.Duration, offer.StrategicPriority);
    }

    private static void Hire(Clan clan, Kingdom kingdom, int awardFactor,
        MercenaryContractDuration duration, MercenaryStrategicPriority priority)
    {
        CampaignTime serviceCommitment = GetEndTime(duration);

        // A finite default keeps renewal meaningful while preserving Bannerlord's
        // native kingdom membership and payment lifecycle.
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
                out MercenaryContract contract);
            contract.SetDuration(duration, serviceCommitment, GetWarTargetId(kingdom, duration));
            contract.SetStrategicPriority(priority);
        }
    }

    private static CampaignTime GetEndTime(MercenaryContractDuration duration)
    {
        switch (duration)
        {
            case MercenaryContractDuration.Days30: return CampaignTime.DaysFromNow(30f);
            case MercenaryContractDuration.Days120: return CampaignTime.DaysFromNow(120f);
            case MercenaryContractDuration.UntilCurrentWarEnds:
            case MercenaryContractDuration.Indefinite: return CampaignTime.Zero;
            default: return CampaignTime.DaysFromNow(60f);
        }
    }

    private static string GetWarTargetId(Kingdom kingdom, MercenaryContractDuration duration)
    {
        if (duration != MercenaryContractDuration.UntilCurrentWarEnds) return string.Empty;
        return Kingdom.All.FirstOrDefault(other => other != kingdom
            && !other.IsEliminated && FactionManager.IsAtWarAgainstFaction(kingdom, other))?.StringId
               ?? string.Empty;
    }
}
