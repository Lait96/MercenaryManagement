using System;
using HarmonyLib;
using TaleWorlds.CampaignSystem.ViewModelCollection.ClanManagement;

namespace MercenaryManagement.Patches;

[HarmonyPatch(typeof(ClanManagementVM), nameof(ClanManagementVM.SetSelectedCategory))]
internal static class ClanManagementSelectionPatch
{
    public static event Action<ClanManagementVM, int>? SelectedCategoryChanged;

    [HarmonyPrefix]
    private static bool Prefix(ClanManagementVM __instance, int index)
    {
        if (index != 4)
        {
            return true;
        }

        SelectedCategoryChanged?.Invoke(__instance, index);
        return false;
    }

    [HarmonyPostfix]
    private static void Postfix(ClanManagementVM __instance, int index)
    {
        SelectedCategoryChanged?.Invoke(__instance, index);
    }
}
