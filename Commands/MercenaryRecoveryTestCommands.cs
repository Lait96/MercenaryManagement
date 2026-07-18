using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using MercenaryManagement.Behaviors;
using MercenaryManagement.Contracts;
using MercenaryManagement.Services;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;

namespace MercenaryManagement.Commands;

public static class MercenaryRecoveryTestCommands
{
    [CommandLineFunctionality.CommandLineArgumentFunction(
        "mm_recovery_priority_test",
        "campaign")]
    public static string Run(List<string> arguments)
    {
        const string usage = "Usage: campaign.mm_recovery_priority_test <ACTIVE_CLAN_ID> <DAYS|NaN> <TARGET_RATIO|NaN> confirm";
        if (!MercenaryDebugCommands.TryValidateCampaign(out string error))
            return error;
        if (arguments.Count != 4
            || arguments[3] != "confirm"
            || !float.TryParse(arguments[1], NumberStyles.Float,
                CultureInfo.InvariantCulture, out float durationDays)
            || !float.TryParse(arguments[2], NumberStyles.Float,
                CultureInfo.InvariantCulture, out float targetRatio))
            return usage;
        if (!MercenaryDebugCommands.TryFindClan(arguments[0], out Clan clan, out error))
            return error;
        Kingdom? kingdom = clan.Kingdom;
        if (kingdom is null || !clan.IsUnderMercenaryService)
            return "FAIL | clan must have an active mercenary contract";

        MercenaryContractBehavior? behavior = Campaign.Current!
            .GetCampaignBehavior<MercenaryContractBehavior>();
        if (behavior?.GetOrCreateContract(clan, out MercenaryContract contract) != true)
            return "FAIL | active contract API is unavailable";

        MercenaryStrategicPriority previous = contract.StrategicPriority == MercenaryStrategicPriority.Recovery
            ? contract.PreviousStrategicPriority
            : contract.StrategicPriority;
        if (!MercenaryPriorityService.TrySetRecoveryPriority(
                clan, kingdom, durationDays, targetRatio))
            return "FAIL | recovery API rejected the request; run as the employer kingdom's ruler";

        bool normalized = contract.StrategicPriority == MercenaryStrategicPriority.Recovery
                          && contract.HasTemporaryPriority
                          && contract.RecoveryTargetPartySizeRatio
                          == MercenaryRecoveryPolicy.NormalizeTargetPartySizeRatio(targetRatio);
        float[] actualRatios = clan.WarPartyComponents
            .Where(component => component.MobileParty.IsActive
                                && !component.MobileParty.IsDisbanding)
            .Select(component => component.MobileParty.PartySizeRatio)
            .ToArray();
        bool actualGameApiReadable = actualRatios.All(ratio => !float.IsNaN(ratio)
                                                              && !float.IsInfinity(ratio));
        float target = contract.RecoveryTargetPartySizeRatio;
        bool incompleteGoalPreserved = !behavior.RunDiagnosticRecoveryCheck(
            clan,
            new[] { System.Math.Max(0f, target - 0.01f) });
        bool completed = behavior.RunDiagnosticRecoveryCheck(clan, new[] { target, 1f });
        bool restored = contract.StrategicPriority
                        == MercenaryRecoveryPolicy.GetRestoredPriority(previous);

        bool passed = normalized && actualGameApiReadable
                      && incompleteGoalPreserved && completed && restored;
        return $"SUMMARY | result={(passed ? "PASS" : "FAIL")}; "
               + $"input_days={durationDays.ToString(CultureInfo.InvariantCulture)}; "
               + $"normalized_days={MercenaryRecoveryPolicy.NormalizeDurationDays(durationDays).ToString(CultureInfo.InvariantCulture)}; "
               + $"input_target={targetRatio.ToString(CultureInfo.InvariantCulture)}; "
               + $"normalized_target={target.ToString(CultureInfo.InvariantCulture)}; "
               + $"game_parties={actualRatios.Length}; restored={contract.StrategicPriority}";
    }
}
