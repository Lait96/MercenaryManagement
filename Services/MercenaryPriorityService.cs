using MercenaryManagement.Behaviors;
using MercenaryManagement.Contracts;
using TaleWorlds.CampaignSystem;

namespace MercenaryManagement.Services;

internal static class MercenaryPriorityService
{
    public static bool TrySetStrategicPriority(
        Clan clan,
        Kingdom kingdom,
        MercenaryStrategicPriority strategicPriority)
    {
        if (!MercenaryHiringService.IsPlayerRuler(kingdom)
            || !clan.IsUnderMercenaryService
            || clan.Kingdom != kingdom
            || !IsDefined(strategicPriority))
        {
            return false;
        }

        MercenaryContractBehavior? contractBehavior = Campaign.Current?
            .GetCampaignBehavior<MercenaryContractBehavior>();
        return contractBehavior?.SetStrategicPriority(clan, strategicPriority) == true;
    }

    private static bool IsDefined(MercenaryStrategicPriority strategicPriority)
    {
        switch (strategicPriority)
        {
            case MercenaryStrategicPriority.IndependentOperations:
            case MercenaryStrategicPriority.JoinArmies:
            case MercenaryStrategicPriority.EconomicWarfare:
            case MercenaryStrategicPriority.TerritorialDefense:
                return true;
            default:
                return false;
        }
    }
}
