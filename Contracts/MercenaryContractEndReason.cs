namespace MercenaryManagement.Contracts;

internal enum MercenaryContractEndReason
{
    Unknown = 0,
    TermExpired = 1,
    WarEnded = 2,
    EmployerDismissed = 3,
    MercenaryLeft = 4,
    PartyUnavailable = 5,
    ChangedKingdom = 6
}
