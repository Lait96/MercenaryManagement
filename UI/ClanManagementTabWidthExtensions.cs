using System.Collections.Generic;
using Bannerlord.UIExtenderEx.Attributes;
using Bannerlord.UIExtenderEx.Prefabs2;

namespace MercenaryManagement.UI;

[PrefabExtension("ClanScreen", "descendant::Constant[@Name='Header.Tab.Left.Width.Scaled']")]
internal sealed class ClanLeftTabWidthExtension : PrefabExtensionSetAttributePatch
{
    public override List<Attribute> Attributes => [new Attribute("MultiplyResult", "0.8")];
}

[PrefabExtension("ClanScreen", "descendant::Constant[@Name='Header.Tab.Center.Width.Scaled']")]
internal sealed class ClanCenterTabWidthExtension : PrefabExtensionSetAttributePatch
{
    public override List<Attribute> Attributes => [new Attribute("MultiplyResult", "0.8")];
}

[PrefabExtension("ClanScreen", "descendant::Constant[@Name='Header.Tab.Right.Width.Scaled']")]
internal sealed class ClanRightTabWidthExtension : PrefabExtensionSetAttributePatch
{
    public override List<Attribute> Attributes => [new Attribute("MultiplyResult", "0.8")];
}
