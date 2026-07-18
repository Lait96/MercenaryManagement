using System.Collections.Generic;
using MercenaryManagement.Contracts;
using MercenaryManagement.Services;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace MercenaryManagement.Behaviors;

internal sealed class MercenaryContractBehavior : CampaignBehaviorBase
{
    private const int CurrentDataVersion = 6;

    private Dictionary<string, MercenaryContract> _contracts =
        new Dictionary<string, MercenaryContract>();
    private Dictionary<string, int> _legacyInfluencePrices = new Dictionary<string, int>();
    private Dictionary<string, List<MercenaryContractHistoryEntry>> _contractHistory =
        new Dictionary<string, List<MercenaryContractHistoryEntry>>();
    private readonly Dictionary<string, MercenaryContractEndReason> _pendingEndReasons =
        new Dictionary<string, MercenaryContractEndReason>();
    private int _dataVersion;

    public override void RegisterEvents()
    {
        CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(
            this,
            OnSessionLaunched);
        CampaignEvents.OnClanChangedKingdomEvent.AddNonSerializedListener(
            this,
            OnClanChangedKingdom);
        CampaignEvents.DailyTickClanEvent.AddNonSerializedListener(this, OnDailyTickClan);
    }

    public override void SyncData(IDataStore dataStore)
    {
        dataStore.SyncData("MercenaryManagement_Contracts", ref _contracts);
        dataStore.SyncData("MercenaryManagement_DataVersion", ref _dataVersion);
        dataStore.SyncData("MercenaryManagement_ContractHistory", ref _contractHistory);

        // Keep reading the old key so campaigns saved before contracts were introduced
        // retain their individually configured prices.
        dataStore.SyncData(
            "MercenaryManagement_InfluencePrices",
            ref _legacyInfluencePrices);

        _contracts ??= new Dictionary<string, MercenaryContract>();
        _legacyInfluencePrices ??= new Dictionary<string, int>();
        _contractHistory ??= new Dictionary<string, List<MercenaryContractHistoryEntry>>();
    }

    public bool TryGetContract(Clan clan, out MercenaryContract contract)
    {
        if (clan.IsUnderMercenaryService
            && clan.Kingdom is not null
            && _contracts.TryGetValue(clan.StringId, out contract)
            && contract.IsActive
            && contract.Matches(clan.StringId, clan.Kingdom.StringId))
        {
            bool isLegacyContract = contract.Clan is null
                                    || contract.EmployerKingdom is null;
            contract.AttachParticipants(clan, clan.Kingdom);
            if (isLegacyContract && contract.EndTime == CampaignTime.Zero)
            {
                contract.SetEndTime(clan.ShouldStayInKingdomUntil);
            }

            contract.EnsureServiceStartTime();
            contract.EnsureStrategicPriority();
            contract.EnsureEvaluationDefaults();
            contract.SetInfluencePrice(NormalizeInfluencePrice(clan, contract.InfluencePrice));
            return true;
        }

        contract = null!;
        return false;
    }

    public float GetReliability(Clan clan)
    {
        if (_contracts.TryGetValue(clan.StringId, out MercenaryContract contract)
            && contract.IsActive)
            return contract.Reliability;
        return GetReputation(clan, null);
    }

    public IReadOnlyList<MercenaryContractHistoryEntry> GetHistory(Clan clan)
    {
        return _contractHistory.TryGetValue(clan.StringId,
            out List<MercenaryContractHistoryEntry> history)
            ? history
            : (IReadOnlyList<MercenaryContractHistoryEntry>)System.Array.Empty<MercenaryContractHistoryEntry>();
    }

    public float GetReputation(Clan clan, Kingdom? employer)
    {
        if (!_contractHistory.TryGetValue(clan.StringId,
                out List<MercenaryContractHistoryEntry> history)
            || history.Count == 0)
            return 50f;

        float total = 0f;
        float employerTotal = 0f;
        int employerCount = 0;
        foreach (MercenaryContractHistoryEntry entry in history)
        {
            float days = (float)System.Math.Max(0d,
                entry.ServiceEndTime.ToDays - entry.ServiceStartTime.ToDays);
            float score = MercenaryReputationModel.Calculate(new ContractReputationInputs(
                entry.Reliability, entry.EmployerSatisfaction, entry.ContributionPoints,
                entry.FinalRelation, days));
            total += score;
            if (employer is not null && entry.KingdomId == employer.StringId)
            {
                employerTotal += score;
                employerCount++;
            }
        }

        float general = total / history.Count;
        return MercenaryReputationModel.Combine(general,
            employerCount == 0 ? 50f : employerTotal / employerCount,
            employerCount > 0);
    }

    public bool GetOrCreateContract(Clan clan, out MercenaryContract contract)
    {
        if (TryGetContract(clan, out contract))
        {
            return true;
        }

        if (!clan.IsUnderMercenaryService || clan.Kingdom is null)
        {
            contract = null!;
            return false;
        }

        int influencePrice = clan.MercenaryAwardMultiplier;
        if (_legacyInfluencePrices.TryGetValue(clan.StringId, out int legacyInfluencePrice))
        {
            influencePrice = legacyInfluencePrice;
        }

        return StartContract(
            clan,
            clan.Kingdom,
            influencePrice,
            clan.ShouldStayInKingdomUntil,
            out contract);
    }

    public bool StartContract(
        Clan clan,
        Kingdom kingdom,
        int influencePrice,
        CampaignTime endTime,
        out MercenaryContract contract)
    {
        if (!clan.IsUnderMercenaryService || clan.Kingdom != kingdom)
        {
            contract = null!;
            return false;
        }

        influencePrice = NormalizeInfluencePrice(clan, influencePrice);

        if (TryGetContract(clan, out contract))
        {
            contract.SetInfluencePrice(influencePrice);
            contract.SetEndTime(endTime);
            return true;
        }

        float previousReliability = GetReputation(clan, kingdom);
        CompleteContract(clan, MercenaryContractEndReason.ChangedKingdom);
        contract = new MercenaryContract(clan, kingdom, influencePrice, endTime);
        contract.SetEvaluation(50f, previousReliability, CampaignTime.Now);
        _contracts[clan.StringId] = contract;
        _legacyInfluencePrices.Remove(clan.StringId);
        return true;
    }

    public void SetInfluencePrice(Clan clan, int influencePrice)
    {
        if (GetOrCreateContract(clan, out MercenaryContract contract))
        {
            int normalized = NormalizeInfluencePrice(clan, influencePrice);
            if (normalized > contract.InfluencePrice)
                contract.AdjustEvaluation(System.Math.Min(8f, normalized - contract.InfluencePrice), 0f);
            contract.SetInfluencePrice(normalized);
        }
    }

    public bool SetServiceStartTime(Clan clan, CampaignTime serviceStartTime)
    {
        if (serviceStartTime == CampaignTime.Zero || serviceStartTime.IsFuture)
        {
            return false;
        }

        if (!GetOrCreateContract(clan, out MercenaryContract contract))
        {
            return false;
        }

        contract.SetServiceStartTime(serviceStartTime);
        return true;
    }

    public bool SetContractEndTime(Clan clan, CampaignTime endTime)
    {
        if (!GetOrCreateContract(clan, out MercenaryContract contract))
        {
            return false;
        }

        contract.SetEndTime(endTime);
        return true;
    }

    public bool SetStrategicPriority(
        Clan clan,
        MercenaryStrategicPriority strategicPriority)
    {
        if (!IsDefined(strategicPriority))
        {
            return false;
        }

        if (!GetOrCreateContract(clan, out MercenaryContract contract))
        {
            return false;
        }

        MercenaryStrategicPriority previousPriority = contract.StrategicPriority;
        if (strategicPriority == MercenaryStrategicPriority.Recovery)
        {
            contract.SetRecoveryPriority(
                MercenaryRecoveryPolicy.DefaultDurationDays,
                MercenaryRecoveryPolicy.DefaultTargetPartySizeRatio);
        }
        else
        {
            contract.SetStrategicPriority(strategicPriority);
        }
        if (clan == Clan.PlayerClan && previousPriority != strategicPriority)
        {
            TextObject message = new TextObject(
                "{=MercenaryManagement_PlayerPriorityChanged}Your employer changed the clan's strategic priority to {PRIORITY}.");
            message.SetTextVariable("PRIORITY", GetPriorityName(strategicPriority));
            InformationManager.DisplayMessage(new InformationMessage(message.ToString()));
        }
        return true;
    }

    public bool SetRecoveryPriority(
        Clan clan,
        float durationDays,
        float targetPartySizeRatio)
    {
        if (!GetOrCreateContract(clan, out MercenaryContract contract))
            return false;

        MercenaryStrategicPriority previousPriority = contract.StrategicPriority;
        contract.SetRecoveryPriority(durationDays, targetPartySizeRatio);
        if (clan == Clan.PlayerClan && previousPriority != MercenaryStrategicPriority.Recovery)
        {
            TextObject message = new TextObject(
                "{=MercenaryManagement_PlayerPriorityChanged}Your employer changed the clan's strategic priority to {PRIORITY}.");
            message.SetTextVariable("PRIORITY", GetPriorityName(MercenaryStrategicPriority.Recovery));
            InformationManager.DisplayMessage(new InformationMessage(message.ToString()));
        }
        return true;
    }

    private static TextObject GetPriorityName(MercenaryStrategicPriority priority)
    {
        switch (priority)
        {
            case MercenaryStrategicPriority.JoinArmies:
                return new TextObject("{=MercenaryManagement_PriorityJoinArmies}Join armies");
            case MercenaryStrategicPriority.EconomicWarfare:
                return new TextObject("{=MercenaryManagement_PriorityEconomicWarfare}Economic warfare");
            case MercenaryStrategicPriority.TerritorialDefense:
                return new TextObject("{=MercenaryManagement_PriorityTerritorialDefense}Territorial defense");
            case MercenaryStrategicPriority.Recovery:
                return new TextObject("{=MercenaryManagement_PriorityRecovery}Recovery");
            default:
                return new TextObject("{=MercenaryManagement_PriorityIndependentOperations}Independent operations");
        }
    }

    public bool CompleteContract(Clan clan,
        MercenaryContractEndReason reason = MercenaryContractEndReason.Unknown)
    {
        _legacyInfluencePrices.Remove(clan.StringId);

        if (!_contracts.TryGetValue(clan.StringId, out MercenaryContract contract)
            || !contract.IsActive)
        {
            return false;
        }

        if (contract.Clan is null || contract.EmployerKingdom is null)
        {
            foreach (Kingdom kingdom in Kingdom.All)
            {
                if (kingdom.StringId == contract.KingdomId)
                {
                    contract.AttachParticipants(clan, kingdom);
                    break;
                }
            }
        }

        if (_pendingEndReasons.TryGetValue(clan.StringId,
                out MercenaryContractEndReason pendingReason))
            reason = pendingReason;
        _pendingEndReasons.Remove(clan.StringId);

        CampaignTime completionTime = CampaignTime.Now;
        int relation = contract.Clan?.Leader is not null
                       && contract.EmployerKingdom?.Leader is not null
            ? contract.Clan.Leader.GetRelation(contract.EmployerKingdom.Leader)
            : 0;
        if (!_contractHistory.TryGetValue(clan.StringId,
                out List<MercenaryContractHistoryEntry> history))
        {
            history = new List<MercenaryContractHistoryEntry>();
            _contractHistory[clan.StringId] = history;
        }
        history.Add(new MercenaryContractHistoryEntry(contract, reason, completionTime, relation));
        contract.Complete(completionTime);
        return true;
    }

    public void SetPendingEndReason(Clan clan, MercenaryContractEndReason reason)
    {
        _pendingEndReasons[clan.StringId] = reason;
    }

    public void ClearPendingEndReason(Clan clan)
    {
        _pendingEndReasons.Remove(clan.StringId);
    }

    public bool RunDiagnosticDailyEvaluation(Clan clan, int count)
    {
        if (count < 1 || !TryGetContract(clan, out MercenaryContract contract))
        {
            return false;
        }

        for (int index = 0; index < count; index++)
        {
            MercenaryEvaluationService.EvaluateDaily(contract);
        }
        return true;
    }

    public bool RunDiagnosticLifecycleCheck(Clan clan, bool forceWarEnded)
    {
        if (!TryGetContract(clan, out MercenaryContract contract)) return false;
        bool expired = contract.EndTime != CampaignTime.Zero
                       && contract.EndTime != CampaignTime.Never
                       && contract.EndTime.IsPast;
        bool warEnded = contract.Duration == MercenaryContractDuration.UntilCurrentWarEnds
                        && (forceWarEnded || !IsWarStillActive(contract));
        if (!expired && !warEnded) return false;
        MercenaryDismissalService.EndService(clan);
        return true;
    }

    private void OnSessionLaunched(CampaignGameStarter campaignGameStarter)
    {
        if (_dataVersion > CurrentDataVersion)
        {
            // Do not downgrade or rewrite data created by a newer mod version.
            return;
        }

        var knownClanIds = new HashSet<string>();
        foreach (Clan clan in Clan.All)
        {
            knownClanIds.Add(clan.StringId);
            if (clan.IsUnderMercenaryService && clan.Kingdom is not null)
            {
                GetOrCreateContract(clan, out _);
                continue;
            }

            CompleteContract(clan);
        }

        var orphanedContractIds = new List<string>();
        foreach (string clanId in _contracts.Keys)
        {
            if (!knownClanIds.Contains(clanId))
            {
                orphanedContractIds.Add(clanId);
            }
        }

        foreach (string clanId in orphanedContractIds)
        {
            _contracts.Remove(clanId);
        }

        _legacyInfluencePrices.Clear();

        if (_dataVersion < 3)
        {
            foreach (MercenaryContract contract in _contracts.Values)
                contract.MigrateDuration();
        }

        // Reconciliation above migrates legacy prices and supplies defaults for
        // contract fields added by later schema versions.
        _dataVersion = CurrentDataVersion;
    }

    private static int NormalizeInfluencePrice(Clan clan, int influencePrice)
    {
        return System.Math.Max(
            MercenaryPaymentService.CalculateMinimumInfluencePrice(clan),
            System.Math.Min(
                MercenaryPaymentService.CalculateMaximumInfluencePrice(clan),
                influencePrice));
    }

    private static bool IsDefined(MercenaryStrategicPriority strategicPriority)
    {
        switch (strategicPriority)
        {
            case MercenaryStrategicPriority.IndependentOperations:
            case MercenaryStrategicPriority.JoinArmies:
            case MercenaryStrategicPriority.EconomicWarfare:
            case MercenaryStrategicPriority.TerritorialDefense:
            case MercenaryStrategicPriority.Recovery:
                return true;
            default:
                return false;
        }
    }

    private void OnClanChangedKingdom(
        Clan clan,
        Kingdom oldKingdom,
        Kingdom newKingdom,
        ChangeKingdomAction.ChangeKingdomActionDetail detail,
        bool showNotification)
    {
        if (oldKingdom == newKingdom)
        {
            if (!clan.IsUnderMercenaryService)
            {
                CompleteContract(clan, MercenaryContractEndReason.PartyUnavailable);
            }

            return;
        }

        CompleteContract(clan, MercenaryContractEndReason.ChangedKingdom);

        if (newKingdom is not null && clan.IsUnderMercenaryService)
        {
            StartContract(
                clan,
                newKingdom,
                clan.MercenaryAwardMultiplier,
                clan.ShouldStayInKingdomUntil,
                out _);
        }
    }

    private void OnDailyTickClan(Clan clan)
    {
        if (!TryGetContract(clan, out MercenaryContract contract))
        {
            return;
        }

        if (clan.IsEliminated
            || contract.EmployerKingdom?.IsEliminated == true
            || contract.EmployerKingdom?.Leader is null
            || !contract.EmployerKingdom.Leader.IsAlive)
        {
            MercenaryDismissalService.EndService(clan, MercenaryContractEndReason.PartyUnavailable);
            return;
        }

        if (contract.EndTime != CampaignTime.Zero
            && contract.EndTime != CampaignTime.Never
            && contract.EndTime.IsPast)
        {
            MercenaryDismissalService.EndService(clan, MercenaryContractEndReason.TermExpired);
            return;
        }

        if (contract.Duration == MercenaryContractDuration.UntilCurrentWarEnds
            && !IsWarStillActive(contract))
        {
            MercenaryDismissalService.EndService(clan, MercenaryContractEndReason.WarEnded);
            return;
        }

        if (TryCompleteRecoveryPriority(clan, contract))
        {
            if (clan == Clan.PlayerClan)
            {
                TextObject message = new TextObject(
                    "{=MercenaryManagement_PlayerRecoveryCompleted}Recovery is complete. The previous strategic priority has been restored.");
                InformationManager.DisplayMessage(new InformationMessage(message.ToString()));
            }
        }

        MercenaryEvaluationService.EvaluateDaily(contract);
        TryVoluntaryDeparture(clan, contract);
    }

    internal bool RunDiagnosticRecoveryCheck(
        Clan clan,
        IReadOnlyList<float>? partySizeRatios = null,
        bool forceExpired = false)
    {
        return TryGetContract(clan, out MercenaryContract contract)
               && TryCompleteRecoveryPriority(clan, contract, partySizeRatios, forceExpired);
    }

    private static bool TryCompleteRecoveryPriority(
        Clan clan,
        MercenaryContract contract,
        IReadOnlyList<float>? diagnosticRatios = null,
        bool forceExpired = false)
    {
        if (!contract.HasTemporaryPriority)
            return false;

        var ratios = diagnosticRatios is null ? new List<float>() : null;
        if (ratios is not null)
        {
            foreach (var component in clan.WarPartyComponents)
            {
                if (component.MobileParty.IsActive && !component.MobileParty.IsDisbanding)
                    ratios.Add(component.MobileParty.PartySizeRatio);
            }
        }

        IReadOnlyList<float> activeRatios = diagnosticRatios ?? ratios!;
        bool expired = forceExpired || contract.TemporaryPriorityEndTime.IsPast;
        bool goalComplete = MercenaryRecoveryPolicy.IsGoalComplete(
            activeRatios,
            contract.RecoveryTargetPartySizeRatio);
        return (expired || goalComplete) && contract.RestorePreviousPriority();
    }

    private static void TryVoluntaryDeparture(Clan clan, MercenaryContract contract)
    {
        if (contract.ServiceStartTime.ElapsedDaysUntilNow < 30f || HasPartyInMapEvent(clan)) return;
        float risk = MercenaryEvaluationService.CalculateDepartureRisk(contract);
        if (risk <= 70f) return;
        float dailyChance = (risk - 70f) / 3000f;
        if (MBRandom.RandomFloat < dailyChance)
            MercenaryDismissalService.EndService(clan, MercenaryContractEndReason.MercenaryLeft);
    }

    private static bool HasPartyInMapEvent(Clan clan)
    {
        foreach (var component in clan.WarPartyComponents)
            if (component.MobileParty.MapEvent is not null) return true;
        return false;
    }

    private static bool IsWarStillActive(MercenaryContract contract)
    {
        Kingdom? employer = contract.EmployerKingdom;
        if (employer is null || string.IsNullOrEmpty(contract.WarTargetKingdomId)) return false;
        foreach (Kingdom kingdom in Kingdom.All)
        {
            if (kingdom.StringId == contract.WarTargetKingdomId)
                return !kingdom.IsEliminated && FactionManager.IsAtWarAgainstFaction(employer, kingdom);
        }
        return false;
    }
}
