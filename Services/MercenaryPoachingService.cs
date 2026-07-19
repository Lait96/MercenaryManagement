using System.Linq;
using MercenaryManagement.Behaviors;
using MercenaryManagement.Contracts;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Localization;

namespace MercenaryManagement.Services;

internal static class MercenaryPoachingService
{
    public static bool IsServingAnotherKingdom(Clan clan, Kingdom kingdom)
    {
        return clan.IsMinorFaction
               && !clan.IsBanditFaction
               && !clan.IsEliminated
               && clan.IsUnderMercenaryService
               && clan.Kingdom is not null
               && clan.Kingdom != kingdom;
    }

    public static bool TryGetFailureReason(Clan clan, Kingdom? targetKingdom, out TextObject reason)
    {
        if (targetKingdom is null || targetKingdom.IsEliminated)
        {
            reason = new TextObject(
                "{=MercenaryManagement_PoachFailedNoKingdom}The kingdom is no longer available.");
            return true;
        }

        if (!MercenaryHiringService.IsPlayerRuler(targetKingdom))
        {
            reason = new TextObject(
                "{=MercenaryManagement_PoachFailedNotRuler}Only the ruler of the kingdom can recruit mercenary clans from other kingdoms.");
            return true;
        }

        if (!IsServingAnotherKingdom(clan, targetKingdom))
        {
            reason = new TextObject(
                "{=MercenaryManagement_PoachFailedNotServingOther}This clan is no longer serving another kingdom as mercenaries.");
            return true;
        }

        if (MercenaryHiringService.TryGetLeaderFailureReason(clan, out reason))
        {
            return true;
        }

        Kingdom currentKingdom = clan.Kingdom!;
        if (FactionManager.IsAtWarAgainstFaction(currentKingdom, targetKingdom))
        {
            reason = new TextObject(
                "{=MercenaryManagement_PoachFailedKingdomsAtWar}The clan cannot switch sides while its current kingdom is at war with your kingdom.");
            return true;
        }

        if (Campaign.Current.Models.DiplomacyModel.IsAtConstantWar(clan, targetKingdom))
        {
            reason = new TextObject(
                "{=MercenaryManagement_PoachFailedConstantWar}The clan cannot enter your kingdom's service because the factions are permanent enemies.");
            return true;
        }

        float elapsedServiceDays = 0f;
        MercenaryContractBehavior? contractBehavior = Campaign.Current?
            .GetCampaignBehavior<MercenaryContractBehavior>();
        if (contractBehavior is not null
            && contractBehavior.GetOrCreateContract(clan, out MercenaryContract contract))
        {
            elapsedServiceDays = contract.ServiceStartTime.ElapsedDaysUntilNow;
        }

        if (!MercenaryPlayerRecruitmentPolicy.HasCompletedProtectedService(elapsedServiceDays))
        {
            reason = new TextObject(
                "{=MercenaryManagement_PoachFailedCommitment}The clan will consider changing employers after its first {DAYS} days of service.");
            reason.SetTextVariable("DAYS", MercenaryPlayerRecruitmentPolicy.ProtectedServiceDays);
            return true;
        }

        if (clan.WarPartyComponents.Any(component => component.MobileParty.MapEvent is not null))
        {
            reason = new TextObject(
                "{=MercenaryManagement_PoachFailedInBattle}The clan cannot switch kingdoms while one of its parties is participating in a battle.");
            return true;
        }

        // These are the existing kingdom-side hiring limits, not poaching-specific rules.
        return MercenaryHiringService.TryGetEmployerFailureReason(clan, targetKingdom, out reason);
    }

    public static void Poach(Clan clan, Kingdom targetKingdom)
    {
        int awardFactor = MercenaryHiringService.CalculateHirePrice(clan, targetKingdom);
        CampaignTime serviceCommitment = CampaignTime.DaysFromNow(60f);

        // Bannerlord's native action ends the old service and starts the new one.
        ChangeKingdomAction.ApplyByJoinFactionAsMercenary(
            clan,
            targetKingdom,
            serviceCommitment,
            awardFactor,
            true);

        MercenaryContractBehavior? contractBehavior = Campaign.Current?
            .GetCampaignBehavior<MercenaryContractBehavior>();
        if (contractBehavior is not null)
        {
            contractBehavior.StartContract(
                clan,
                targetKingdom,
                awardFactor,
                serviceCommitment,
                out _);
        }
    }
}
