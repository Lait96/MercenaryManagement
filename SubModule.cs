using System;
using System.Collections.Generic;
using System.Reflection;
using Bannerlord.UIExtenderEx;
using HarmonyLib;
using MercenaryManagement.Behaviors;
using MercenaryManagement.Patches;
using MercenaryManagement.Services;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace MercenaryManagement;

public sealed class SubModule : MBSubModuleBase
{
    private const string HarmonyId = "MercenaryManagement";

    private static readonly Type[] HarmonyPatchTypes =
    {
        typeof(ClanManagementSelectionPatch),
        typeof(MercenaryPaymentPatch),
        typeof(MercenaryPriorityArmyCallDistancePatch),
        typeof(MercenaryPriorityArmyCandidateWeightPatch),
        typeof(MercenaryPriorityBehaviorScorePatch)
    };

    private UIExtender? _uiExtender;
    private readonly List<Harmony> _harmonies = new List<Harmony>();

    protected override void OnSubModuleLoad()
    {
        base.OnSubModuleLoad();

        _uiExtender = UIExtender.Create(nameof(MercenaryManagement));
        _uiExtender.Register(Assembly.GetExecutingAssembly());
        _uiExtender.Enable();
    }

    protected override void OnSubModuleUnloaded()
    {
        _uiExtender?.Disable();
        _uiExtender = null;

        foreach (Harmony harmony in _harmonies)
        {
            harmony.UnpatchAll(harmony.Id);
        }

        _harmonies.Clear();

        base.OnSubModuleUnloaded();
    }

    protected override void OnGameStart(Game game, IGameStarter gameStarter)
    {
        base.OnGameStart(game, gameStarter);

        if (gameStarter is CampaignGameStarter campaignGameStarter)
        {
            MercenaryAiDiagnostics.Stop();
            campaignGameStarter.AddBehavior(new MercenaryContractBehavior());
            campaignGameStarter.AddBehavior(new MercenaryPriorityBehavior());

            if (_harmonies.Count == 0)
            {
                foreach (Type patchType in HarmonyPatchTypes)
                {
                    Harmony? harmony = TryApplyPatch(patchType);
                    if (harmony is not null)
                    {
                        _harmonies.Add(harmony);
                    }
                }
            }
        }
    }

    private static Harmony? TryApplyPatch(Type patchType)
    {
        var harmony = new Harmony($"{HarmonyId}.{patchType.Name}");
        try
        {
            harmony.CreateClassProcessor(patchType).Patch();
            return harmony;
        }
        catch (Exception exception)
        {
            harmony.UnpatchAll(harmony.Id);
            TaleWorlds.Library.Debug.Print(
                $"MercenaryManagement disabled Harmony patch {patchType.Name}: {exception}",
                0,
                TaleWorlds.Library.Debug.DebugColor.Red,
                0UL);
            return null;
        }
    }
}
