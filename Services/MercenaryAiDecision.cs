namespace MercenaryManagement.Services;

internal enum MercenaryAiDecision
{
    ArmyCandidateRank,
    ArmyCallDistance,
    FollowArmy,
    RaidVillage,
    AttackCaravan,
    AttackVillagers,
    PassiveDefense,
    DefendVillage,
    DefendCaravan,
    DefendSettlement,
    FriendlyTerritoryAction,
    PursueBandits,
    PursueSmallEnemyParty,
    DeepEnemyRaid,
    VisitSettlement
}

internal enum MercenaryAiBehaviorSource
{
    ArmyMember,
    EngageParty,
    Military,
    Patrolling,
    VisitSettlement
}
