using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using MercenaryManagement.Services;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;

namespace MercenaryManagement.Patches;

[HarmonyPatch(typeof(DefaultClanFinanceModel), "AddMercenaryIncome")]
internal static class MercenaryPaymentPatch
{
    private static IEnumerable<CodeInstruction> Transpiler(
        IEnumerable<CodeInstruction> instructions)
    {
        int replacementCount = 0;
        var nativeInfluencePriceGetter = AccessTools.PropertyGetter(
            typeof(Clan),
            nameof(Clan.MercenaryAwardMultiplier));
        var individualInfluencePriceGetter = AccessTools.Method(
            typeof(MercenaryPaymentService),
            nameof(MercenaryPaymentService.GetInfluencePrice));

        foreach (CodeInstruction instruction in instructions)
        {
            if (instruction.Calls(nativeInfluencePriceGetter))
            {
                replacementCount++;
                yield return new CodeInstruction(OpCodes.Call, individualInfluencePriceGetter)
                    .MoveLabelsFrom(instruction)
                    .MoveBlocksFrom(instruction);
                continue;
            }

            yield return instruction;
        }

        if (replacementCount == 0)
        {
            throw new InvalidOperationException(
                "MercenaryManagement could not find the native mercenary influence price calculation.");
        }
    }
}
