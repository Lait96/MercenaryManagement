using System.Reflection;
using Bannerlord.UIExtenderEx;
using HarmonyLib;
using MercenaryManagement.Behaviors;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace MercenaryManagement;

public sealed class SubModule : MBSubModuleBase
{
    private const string HarmonyId = "MercenaryManagement";

    private UIExtender? _uiExtender;
    private Harmony? _harmony;

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

        _harmony?.UnpatchAll(HarmonyId);
        _harmony = null;

        base.OnSubModuleUnloaded();
    }

    protected override void OnGameStart(Game game, IGameStarter gameStarter)
    {
        base.OnGameStart(game, gameStarter);

        if (gameStarter is CampaignGameStarter campaignGameStarter)
        {
            campaignGameStarter.AddBehavior(new MercenaryContractBehavior());

            if (_harmony is null)
            {
                _harmony = new Harmony(HarmonyId);
                _harmony.PatchAll(Assembly.GetExecutingAssembly());
            }
        }
    }
}
