using System;
using System.Collections.Generic;
using System.Linq;
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
        IEnumerable<CodeInstruction> instructions,
        MethodBase __originalMethod)
    {
        var instructionList = instructions.ToList();
        MethodInfo nativeMaximumDistanceGetter = AccessTools.PropertyGetter(
            typeof(ArmyManagementCalculationModel),
            nameof(ArmyManagementCalculationModel.MaximumDistanceToCallToArmy));
        MethodInfo priorityMaximumDistanceGetter = AccessTools.Method(
            typeof(MercenaryPriorityService),
            nameof(MercenaryPriorityService.GetMaximumDistanceToCallToArmy));
        MethodBody? methodBody = __originalMethod.GetMethodBody();
        if (methodBody is null)
        {
            throw new InvalidOperationException(
                $"MercenaryManagement could not inspect {__originalMethod.Name}.");
        }

        int getterIndex = instructionList.FindIndex(
            instruction => instruction.Calls(nativeMaximumDistanceGetter));
        if (getterIndex < 0
            || instructionList.FindIndex(
                getterIndex + 1,
                instruction => instruction.Calls(nativeMaximumDistanceGetter)) >= 0)
        {
            throw new InvalidOperationException(
                "MercenaryManagement expected one native army call distance calculation.");
        }

        int mobilePartyLocalIndex = FindNearestStoredMobilePartyLocal(
            instructionList,
            getterIndex,
            methodBody.LocalVariables);

        for (int i = 0; i < instructionList.Count; i++)
        {
            CodeInstruction instruction = instructionList[i];
            if (i != getterIndex)
            {
                yield return instruction;
                continue;
            }

            yield return CodeInstruction.LoadLocal(mobilePartyLocalIndex)
                .MoveLabelsFrom(instruction)
                .MoveBlocksFrom(instruction);
            yield return new CodeInstruction(OpCodes.Call, priorityMaximumDistanceGetter);
        }
    }

    private static int FindNearestStoredMobilePartyLocal(
        IReadOnlyList<CodeInstruction> instructions,
        int beforeIndex,
        IList<LocalVariableInfo> locals)
    {
        for (int i = beforeIndex - 1; i >= 0; i--)
        {
            CodeInstruction instruction = instructions[i];
            if (!IsStoreLocal(instruction.opcode))
            {
                continue;
            }

            int localIndex = instruction.LocalIndex();
            if (localIndex >= 0
                && localIndex < locals.Count
                && locals[localIndex].LocalType == typeof(MobileParty))
            {
                return localIndex;
            }
        }

        throw new InvalidOperationException(
            "MercenaryManagement could not identify the army candidate MobileParty local.");
    }

    private static bool IsStoreLocal(OpCode opcode)
    {
        return opcode == OpCodes.Stloc
               || opcode == OpCodes.Stloc_S
               || opcode == OpCodes.Stloc_0
               || opcode == OpCodes.Stloc_1
               || opcode == OpCodes.Stloc_2
               || opcode == OpCodes.Stloc_3;
    }
}

[HarmonyPatch]
internal static class MercenaryPriorityArmyCandidateWeightPatch
{
    private static MethodBase TargetMethod()
    {
        Type? compilerGeneratedType = typeof(DefaultArmyManagementCalculationModel)
            .GetNestedTypes(BindingFlags.NonPublic)
            .FirstOrDefault(type => type.Name.StartsWith("<>c", StringComparison.Ordinal));
        MethodInfo? targetMethod = compilerGeneratedType?
            .GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
            .SingleOrDefault(method =>
                method.ReturnType == typeof(float)
                && method.GetParameters().Length == 1
                && method.GetParameters()[0].ParameterType
                    == typeof(ValueTuple<MobileParty, float, int>));

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
        yield return AccessTools.Method(typeof(AiVisitSettlementBehavior), "AiHourlyTick");
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

        if (behaviorType == typeof(AiVisitSettlementBehavior))
        {
            return MercenaryAiBehaviorSource.VisitSettlement;
        }

        return MercenaryAiBehaviorSource.Military;
    }
}

[HarmonyPatch(typeof(DefaultPartyTrainingModel), nameof(DefaultPartyTrainingModel.GetEffectiveDailyExperience))]
internal static class MercenaryRecoveryTrainingPatch
{
    private static void Postfix(MobileParty mobileParty, ref ExplainedNumber __result)
    {
        float factor = MercenaryPriorityService.GetTrainingFactor(mobileParty);
        if (factor > 1f)
        {
            __result.AddFactor(
                factor - 1f,
                new TaleWorlds.Localization.TextObject(
                    "{=MercenaryManagement_PriorityRecovery}Recovery"));
        }
    }
}
