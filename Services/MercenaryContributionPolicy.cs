using System;

namespace MercenaryManagement.Services;

internal enum PassiveContributionEvidence
{
    None,
    ServingInArmy,
    EconomicOperation,
    ActiveTerritorialDefense
}

internal static class MercenaryContributionPolicy
{
    public static bool IsEligible(
        Contracts.MercenaryStrategicPriority priority,
        PassiveContributionEvidence evidence)
    {
        switch (priority)
        {
            case Contracts.MercenaryStrategicPriority.JoinArmies:
                return evidence == PassiveContributionEvidence.ServingInArmy;
            case Contracts.MercenaryStrategicPriority.EconomicWarfare:
                return evidence == PassiveContributionEvidence.EconomicOperation;
            case Contracts.MercenaryStrategicPriority.TerritorialDefense:
                return evidence == PassiveContributionEvidence.ActiveTerritorialDefense;
            default:
                return false;
        }
    }

    public static bool CanRecordDaily(float elapsedDaysSinceLastRecord)
    {
        if (float.IsNaN(elapsedDaysSinceLastRecord)
            || float.IsInfinity(elapsedDaysSinceLastRecord))
        {
            return false;
        }

        return elapsedDaysSinceLastRecord < 0f || elapsedDaysSinceLastRecord >= 1f;
    }
}
