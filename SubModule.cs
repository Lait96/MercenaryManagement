using System.Reflection;
using Bannerlord.UIExtenderEx;
using TaleWorlds.MountAndBlade;

namespace MercenaryManagement;

public sealed class SubModule : MBSubModuleBase
{
    private UIExtender? _uiExtender;

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

        base.OnSubModuleUnloaded();
    }
}
