namespace MercenaryManagement.Services;

internal static class MercenaryAiControlPolicy
{
    public static bool CanMakeAutonomousContractDecision(bool isPlayerClan)
    {
        return !isPlayerClan;
    }
}
