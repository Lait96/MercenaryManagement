using System;
using HarmonyLib;
using TaleWorlds.CampaignSystem.ViewModelCollection.ClanManagement;

namespace MercenaryManagement.Patches;

[HarmonyPatch(typeof(ClanManagementVM), nameof(ClanManagementVM.SetSelectedCategory))]
internal static class ClanManagementSelectionPatch
{
    public static event Action<ClanManagementVM>? SelectedCategoryChanged;

    [HarmonyPostfix]
    private static void Postfix(ClanManagementVM __instance)
    {
        SelectedCategoryChanged?.Invoke(__instance);
    }
}
