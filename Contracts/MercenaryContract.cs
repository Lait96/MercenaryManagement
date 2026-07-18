using TaleWorlds.CampaignSystem;
using TaleWorlds.SaveSystem;
using MercenaryManagement.Services;

namespace MercenaryManagement.Contracts;

internal sealed class MercenaryContract
{
    public MercenaryContract(
        Clan clan,
        Kingdom employerKingdom,
        int influencePrice,
        CampaignTime endTime)
    {
        Clan = clan;
        EmployerKingdom = employerKingdom;
        ClanId = clan.StringId;
        KingdomId = employerKingdom.StringId;
        InfluencePrice = influencePrice;
        ServiceStartTime = CampaignTime.Now;
        EndTime = endTime;
        State = MercenaryContractState.Active;
        StrategicPriority = MercenaryStrategicPriority.IndependentOperations;
        Duration = GetDuration(endTime);
        Satisfaction = 50f;
        EmployerSatisfaction = 50f;
        Reliability = 50f;
        LastEvaluationTime = CampaignTime.Now;
    }

    [SaveableProperty(1)]
    public string ClanId { get; private set; }

    [SaveableProperty(2)]
    public string KingdomId { get; private set; }

    [SaveableProperty(3)]
    public int InfluencePrice { get; private set; }

    [SaveableProperty(4)]
    public CampaignTime ServiceStartTime { get; private set; }

    [SaveableProperty(5)]
    public Clan? Clan { get; private set; }

    [SaveableProperty(6)]
    public Kingdom? EmployerKingdom { get; private set; }

    [SaveableProperty(7)]
    public CampaignTime EndTime { get; private set; }

    [SaveableProperty(8)]
    public MercenaryContractState State { get; private set; }

    [SaveableProperty(9)]
    public MercenaryStrategicPriority StrategicPriority { get; private set; }

    [SaveableProperty(10)]
    public float Satisfaction { get; private set; }

    [SaveableProperty(11)]
    public float Reliability { get; private set; }

    [SaveableProperty(12)]
    public int ContributionPoints { get; private set; }

    [SaveableProperty(13)]
    public CampaignTime LastEvaluationTime { get; private set; }

    [SaveableProperty(14)]
    public MercenaryContractDuration Duration { get; private set; }

    [SaveableProperty(15)]
    public string WarTargetKingdomId { get; private set; } = string.Empty;

    [SaveableProperty(16)] public string LastActivity { get; private set; } = string.Empty;
    [SaveableProperty(17)] public CampaignTime LastActivityTime { get; private set; }
    [SaveableProperty(18)] public int RecentEarnings { get; private set; }
    [SaveableProperty(19)] public CampaignTime EarningsPeriodStart { get; private set; }
    [SaveableProperty(20)] public int RewardedContributionMilestones { get; private set; }
    [SaveableProperty(21)] public float EmployerSatisfaction { get; private set; }
    [SaveableProperty(22)] public CampaignTime LastContributionDecayTime { get; private set; }
    [SaveableProperty(23)] public CampaignTime LastRevisionRequestTime { get; private set; }
    [SaveableProperty(24)] public CampaignTime LastSuccessfulRevisionTime { get; private set; }
    [SaveableProperty(25)] public int ContributionPointsAtLastSuccessfulRevision { get; private set; }
    [SaveableProperty(26)] public CampaignTime LastPriorityChangeTime { get; private set; }
    [SaveableProperty(27)] public CampaignTime LastDailyContributionTime { get; private set; }
    [SaveableProperty(28)] public MercenaryStrategicPriority PreviousStrategicPriority { get; private set; }
    [SaveableProperty(29)] public CampaignTime TemporaryPriorityEndTime { get; private set; }
    [SaveableProperty(30)] public float RecoveryTargetPartySizeRatio { get; private set; }

    public bool IsActive => State == MercenaryContractState.Active;
    public bool HasTemporaryPriority => StrategicPriority == MercenaryStrategicPriority.Recovery
                                        && TemporaryPriorityEndTime != CampaignTime.Zero;

    public bool Matches(string clanId, string kingdomId)
    {
        return ClanId == clanId && KingdomId == kingdomId;
    }

    public void AttachParticipants(Clan clan, Kingdom employerKingdom)
    {
        Clan = clan;
        EmployerKingdom = employerKingdom;
        ClanId = clan.StringId;
        KingdomId = employerKingdom.StringId;
    }

    public void SetInfluencePrice(int influencePrice)
    {
        InfluencePrice = influencePrice;
    }

    public void SetServiceStartTime(CampaignTime serviceStartTime)
    {
        ServiceStartTime = serviceStartTime;
    }

    public void SetEndTime(CampaignTime endTime)
    {
        EndTime = endTime;
        Duration = GetDuration(endTime);
    }

    public void SetDuration(MercenaryContractDuration duration, CampaignTime endTime, string warTargetKingdomId = "")
    {
        Duration = duration;
        EndTime = endTime;
        WarTargetKingdomId = warTargetKingdomId ?? string.Empty;
    }

    public void SetStrategicPriority(MercenaryStrategicPriority strategicPriority)
    {
        if (StrategicPriority == strategicPriority)
        {
            if (LastPriorityChangeTime == CampaignTime.Zero)
                LastPriorityChangeTime = CampaignTime.Now;
            return;
        }
        StrategicPriority = strategicPriority;
        LastPriorityChangeTime = CampaignTime.Now;
        ClearTemporaryPriority();
    }

    public void SetRecoveryPriority(float durationDays, float targetPartySizeRatio)
    {
        if (StrategicPriority != MercenaryStrategicPriority.Recovery)
            PreviousStrategicPriority = StrategicPriority;
        PreviousStrategicPriority = MercenaryRecoveryPolicy.GetRestoredPriority(
            PreviousStrategicPriority);
        StrategicPriority = MercenaryStrategicPriority.Recovery;
        TemporaryPriorityEndTime = CampaignTime.DaysFromNow(
            MercenaryRecoveryPolicy.NormalizeDurationDays(durationDays));
        RecoveryTargetPartySizeRatio = MercenaryRecoveryPolicy
            .NormalizeTargetPartySizeRatio(targetPartySizeRatio);
        LastPriorityChangeTime = CampaignTime.Now;
    }

    public bool RestorePreviousPriority()
    {
        if (StrategicPriority != MercenaryStrategicPriority.Recovery)
            return false;
        StrategicPriority = MercenaryRecoveryPolicy.GetRestoredPriority(
            PreviousStrategicPriority);
        LastPriorityChangeTime = CampaignTime.Now;
        ClearTemporaryPriority();
        return true;
    }

    public void SetEvaluation(float satisfaction, float reliability, CampaignTime evaluationTime)
    {
        Satisfaction = Clamp(satisfaction);
        Reliability = Clamp(reliability);
        LastEvaluationTime = evaluationTime;
    }

    public void AdjustEvaluation(float satisfactionDelta, float reliabilityDelta)
    {
        Satisfaction = MercenaryValuePolicy.AdjustPercentage(Satisfaction, satisfactionDelta);
        Reliability = MercenaryValuePolicy.AdjustPercentage(Reliability, reliabilityDelta);
        LastEvaluationTime = CampaignTime.Now;
    }

    public void SetEmployerSatisfaction(float value)
    {
        EmployerSatisfaction = Clamp(value);
    }

    public void AddContribution(int points)
    {
        ContributionPoints = MercenaryValuePolicy.AccumulateNonNegative(
            ContributionPoints, points);
    }

    public void RecordContribution(int points, string activity)
    {
        AddContribution(points);
        LastActivity = activity ?? string.Empty;
        LastActivityTime = CampaignTime.Now;
    }

    public bool TryRecordDailyContribution(int points, string activity)
    {
        float elapsed = LastDailyContributionTime == CampaignTime.Zero
            ? -1f
            : LastDailyContributionTime.ElapsedDaysUntilNow;
        if (points <= 0 || !Services.MercenaryContributionPolicy.CanRecordDaily(elapsed))
        {
            return false;
        }

        RecordContribution(points, activity);
        LastDailyContributionTime = CampaignTime.Now;
        return true;
    }

    public void RecordEarnings(int earnings)
    {
        if (EarningsPeriodStart == CampaignTime.Zero
            || EarningsPeriodStart.ElapsedDaysUntilNow >= 7f)
        {
            RecentEarnings = 0;
            EarningsPeriodStart = CampaignTime.Now;
        }
        RecentEarnings = MercenaryValuePolicy.AccumulateNonNegative(
            RecentEarnings, earnings);
    }

    public void RecordRevisionRequest()
    {
        LastRevisionRequestTime = CampaignTime.Now;
    }

    public void RecordSuccessfulRevision()
    {
        LastSuccessfulRevisionTime = CampaignTime.Now;
        ContributionPointsAtLastSuccessfulRevision = ContributionPoints;
    }

    public bool TryConsumeContributionMilestone()
    {
        int milestones = ContributionPoints / 100;
        if (milestones <= RewardedContributionMilestones) return false;
        RewardedContributionMilestones++;
        return true;
    }

    public void DecayContribution()
    {
        if (LastContributionDecayTime == CampaignTime.Zero)
        {
            LastContributionDecayTime = CampaignTime.Now;
            return;
        }
        if (LastContributionDecayTime.ElapsedDaysUntilNow < 7f) return;
        ContributionPoints = System.Math.Max(0, ContributionPoints - 10);
        LastContributionDecayTime = CampaignTime.Now;
    }

    public void EnsureEvaluationDefaults()
    {
        if (LastEvaluationTime == CampaignTime.Zero)
        {
            if (Satisfaction <= 0f && Reliability <= 0f)
            {
                Satisfaction = 50f;
                Reliability = 50f;
            }
            EmployerSatisfaction = 50f;
            LastEvaluationTime = CampaignTime.Now;
        }
    }

    public void MigrateDuration()
    {
        if (EndTime == CampaignTime.Zero || EndTime == CampaignTime.Never)
        {
            Duration = MercenaryContractDuration.Indefinite;
            return;
        }
        double totalDays = System.Math.Max(0d, EndTime.ToDays - ServiceStartTime.ToDays);
        Duration = totalDays <= 45d ? MercenaryContractDuration.Days30
            : totalDays <= 90d ? MercenaryContractDuration.Days60
            : MercenaryContractDuration.Days120;
    }

    public void EnsureStrategicPriority()
    {
        switch (StrategicPriority)
        {
            case MercenaryStrategicPriority.IndependentOperations:
            case MercenaryStrategicPriority.JoinArmies:
            case MercenaryStrategicPriority.EconomicWarfare:
            case MercenaryStrategicPriority.TerritorialDefense:
            case MercenaryStrategicPriority.Recovery:
                EnsureTemporaryPriority();
                return;
            default:
                StrategicPriority = MercenaryStrategicPriority.IndependentOperations;
                ClearTemporaryPriority();
                return;
        }
    }

    private void EnsureTemporaryPriority()
    {
        if (StrategicPriority != MercenaryStrategicPriority.Recovery)
        {
            ClearTemporaryPriority();
            return;
        }

        PreviousStrategicPriority = MercenaryRecoveryPolicy.GetRestoredPriority(
            PreviousStrategicPriority);
        RecoveryTargetPartySizeRatio = MercenaryRecoveryPolicy
            .NormalizeTargetPartySizeRatio(RecoveryTargetPartySizeRatio);
        if (TemporaryPriorityEndTime == CampaignTime.Zero)
        {
            TemporaryPriorityEndTime = CampaignTime.DaysFromNow(
                MercenaryRecoveryPolicy.DefaultDurationDays);
        }
    }

    private void ClearTemporaryPriority()
    {
        PreviousStrategicPriority = MercenaryStrategicPriority.IndependentOperations;
        TemporaryPriorityEndTime = CampaignTime.Zero;
        RecoveryTargetPartySizeRatio = 0f;
    }

    public void Complete(CampaignTime completionTime)
    {
        if (!IsActive)
        {
            return;
        }

        EndTime = completionTime;
        State = MercenaryContractState.Completed;
    }

    public void EnsureServiceStartTime()
    {
        if (ServiceStartTime == CampaignTime.Zero)
        {
            ServiceStartTime = CampaignTime.Now;
        }
    }

    private static float Clamp(float value)
    {
        return MercenaryValuePolicy.NormalizePercentage(value);
    }

    private static MercenaryContractDuration GetDuration(CampaignTime endTime)
    {
        if (endTime == CampaignTime.Zero || endTime == CampaignTime.Never)
            return MercenaryContractDuration.Indefinite;
        float days = endTime.RemainingDaysFromNow;
        if (days <= 45f) return MercenaryContractDuration.Days30;
        if (days <= 90f) return MercenaryContractDuration.Days60;
        return MercenaryContractDuration.Days120;
    }
}
