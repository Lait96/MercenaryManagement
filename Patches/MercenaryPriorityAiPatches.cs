using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using MercenaryManagement.Services;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CampaignBehaviors.AiBehaviors;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Party;

namespace MercenaryManagement.Patches;

[HarmonyPatch(typeof(DefaultArmyManagementCalculationModel), nameof(DefaultArmyManagementCalculationModel.CanLordCreateArmy))]
internal static class MercenaryPriorityArmyCallDistancePatch
{
    private static IEnumerable<CodeInstruction> Transpiler(
        IEnumerable<CodeInstruction> instructions)
    {
        MethodInfo nativeMaximumDistanceGetter = AccessTools.PropertyGetter(
            typeof(ArmyManagementCalculationModel),
            nameof(ArmyManagementCalculationModel.MaximumDistanceToCallToArmy));
        MethodInfo priorityMaximumDistanceGetter = AccessTools.Method(
            typeof(MercenaryPriorityService),
            nameof(MercenaryPriorityService.GetMaximumDistanceToCallToArmy));
        int replacementCount = 0;

        foreach (CodeInstruction instruction in instructions)
        {
            if (!instruction.Calls(nativeMaximumDistanceGetter))
            {
                yield return instruction;
                continue;
            }

            replacementCount++;
            yield return CodeInstruction.LoadLocal(6)
                .MoveLabelsFrom(instruction)
                .MoveBlocksFrom(instruction);
            yield return new CodeInstruction(OpCodes.Call, priorityMaximumDistanceGetter);
        }

        if (replacementCount != 1)
        {
            throw new InvalidOperationException(
                "MercenaryManagement could not find the native army call distance calculation.");
        }
    }
}

[HarmonyPatch]
internal static class MercenaryPriorityArmyCandidateWeightPatch
{
    private static MethodBase TargetMethod()
    {
        Type? compilerGeneratedType = AccessTools.TypeByName(
            "TaleWorlds.CampaignSystem.GameComponents.DefaultArmyManagementCalculationModel+<>c");
        MethodInfo? targetMethod = compilerGeneratedType is null
            ? null
            : AccessTools.Method(compilerGeneratedType, "<CanLordCreateArmy>b__23_1");

        return targetMethod ?? throw new MissingMethodException(
            "MercenaryManagement could not find the native army candidate weight calculation.");
    }

    private static void Postfix(
        ValueTuple<MobileParty, float, int> __0,
        ref float __result)
    {
        __result = MercenaryPriorityService.GetArmyCandidateWeight(__0.Item1, __result);
    }
}

[HarmonyPatch]
internal static class MercenaryPriorityBehaviorScorePatch
{
    private static IEnumerable<MethodBase> TargetMethods()
    {
        yield return AccessTools.Method(typeof(AiArmyMemberBehavior), "AiHourlyTick");
        yield return AccessTools.Method(typeof(AiEngagePartyBehavior), "AiHourlyTick");
        yield return AccessTools.Method(typeof(AiMilitaryBehavior), "AiHourlyTick");
        yield return AccessTools.Method(typeof(AiPatrollingBehavior), "AiHourlyTick");
    }

    private static void Prefix(PartyThinkParams p, out int __state)
    {
        __state = p.AIBehaviorScores.Count;
    }

    private static void Postfix(
        MobileParty mobileParty,
        PartyThinkParams p,
        int __state,
        MethodBase __originalMethod)
    {
        MercenaryPriorityService.AdjustBehaviorScores(
            mobileParty,
            p,
            __state,
            GetBehaviorSource(__originalMethod));
    }

    private static MercenaryAiBehaviorSource GetBehaviorSource(
        MethodBase originalMethod)
    {
        Type? behaviorType = originalMethod.DeclaringType;
        if (behaviorType == typeof(AiArmyMemberBehavior))
        {
            return MercenaryAiBehaviorSource.ArmyMember;
        }

        if (behaviorType == typeof(AiEngagePartyBehavior))
        {
            return MercenaryAiBehaviorSource.EngageParty;
        }

        if (behaviorType == typeof(AiPatrollingBehavior))
        {
            return MercenaryAiBehaviorSource.Patrolling;
        }

        return MercenaryAiBehaviorSource.Military;
    }
}
