using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using MercenaryManagement.Services;
using MercenaryManagement.Behaviors;
using MercenaryManagement.Contracts;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;

namespace MercenaryManagement.Patches;

[HarmonyPatch(typeof(DefaultClanFinanceModel), "AddMercenaryIncome")]
internal static class MercenaryPaymentPatch
{
    private static void Prefix(ref ExplainedNumber goldChange, out float __state)
    {
        __state = goldChange.ResultNumber;
    }

    private static void Postfix(Clan clan, ref ExplainedNumber goldChange, bool applyWithdrawals, float __state)
    {
        if (!applyWithdrawals) return;
        int earnings = System.Math.Max(0, (int)System.Math.Round(goldChange.ResultNumber - __state));
        MercenaryContractBehavior? behavior = Campaign.Current?
            .GetCampaignBehavior<MercenaryContractBehavior>();
        if (earnings > 0 && behavior?.TryGetContract(clan, out MercenaryContract contract) == true)
            contract.RecordEarnings(earnings);
    }

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

        if (replacementCount != 1)
        {
            throw new InvalidOperationException(
                $"MercenaryManagement expected one native mercenary influence price calculation, but found {replacementCount}.");
        }
    }
}
