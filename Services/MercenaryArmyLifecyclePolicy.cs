namespace MercenaryManagement.Services;

internal enum ArmyLifecycleDecision
{
    Ignore,
    ProcessCreatedArmy,
    ReevaluateAfterLastArmyDispersed
}

internal readonly struct ArmyLifecycleSnapshot
{
    public ArmyLifecycleSnapshot(
        bool hasArmy,
        bool hasKingdom,
        bool kingdomIsEliminated,
        bool playerRulesKingdom,
        int otherActiveArmyCount)
    {
        HasArmy = hasArmy;
        HasKingdom = hasKingdom;
        KingdomIsEliminated = kingdomIsEliminated;
        PlayerRulesKingdom = playerRulesKingdom;
        OtherActiveArmyCount = otherActiveArmyCount;
    }

    public bool HasArmy { get; }
    public bool HasKingdom { get; }
    public bool KingdomIsEliminated { get; }
    public bool PlayerRulesKingdom { get; }
    public int OtherActiveArmyCount { get; }
}

internal readonly struct ArmyPartyLifecycleSnapshot
{
    public ArmyPartyLifecycleSnapshot(
        bool hasArmy,
        bool hasArmyLeader,
        bool isMainParty,
        bool isActive,
        bool isDisbanding,
        bool isArmyLeader)
    {
        HasArmy = hasArmy;
        HasArmyLeader = hasArmyLeader;
        IsMainParty = isMainParty;
        IsActive = isActive;
        IsDisbanding = isDisbanding;
        IsArmyLeader = isArmyLeader;
    }

    public bool HasArmy { get; }
    public bool HasArmyLeader { get; }
    public bool IsMainParty { get; }
    public bool IsActive { get; }
    public bool IsDisbanding { get; }
    public bool IsArmyLeader { get; }
}

internal static class MercenaryArmyLifecyclePolicy
{
    public static ArmyLifecycleDecision OnArmyCreated(ArmyLifecycleSnapshot snapshot)
    {
        return CanProcess(snapshot)
            ? ArmyLifecycleDecision.ProcessCreatedArmy
            : ArmyLifecycleDecision.Ignore;
    }

    public static ArmyLifecycleDecision OnArmyDispersed(ArmyLifecycleSnapshot snapshot)
    {
        return CanProcess(snapshot) && snapshot.OtherActiveArmyCount == 0
            ? ArmyLifecycleDecision.ReevaluateAfterLastArmyDispersed
            : ArmyLifecycleDecision.Ignore;
    }

    public static bool CanEvaluatePartyExit(ArmyPartyLifecycleSnapshot snapshot)
    {
        return snapshot.HasArmy
               && snapshot.HasArmyLeader
               && !snapshot.IsMainParty
               && snapshot.IsActive
               && !snapshot.IsDisbanding
               && !snapshot.IsArmyLeader;
    }

    private static bool CanProcess(ArmyLifecycleSnapshot snapshot)
    {
        return snapshot.HasArmy
               && snapshot.HasKingdom
               && !snapshot.KingdomIsEliminated
               && !snapshot.PlayerRulesKingdom;
    }
}
