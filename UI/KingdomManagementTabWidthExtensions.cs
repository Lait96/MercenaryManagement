using System.Collections.Generic;
using Bannerlord.UIExtenderEx.Attributes;
using Bannerlord.UIExtenderEx.Prefabs2;

namespace MercenaryManagement.UI;

[PrefabExtension(
    "KingdomManagement",
    "descendant::Constant[@Name='Header.Tab.Left.Width.Scaled']")]
internal sealed class KingdomLeftTabWidthExtension : PrefabExtensionSetAttributePatch
{
    public override List<Attribute> Attributes => [new Attribute("MultiplyResult", "0.75")];
}

[PrefabExtension(
    "KingdomManagement",
    "descendant::Constant[@Name='Header.Tab.Center.Width.Scaled']")]
internal sealed class KingdomCenterTabWidthExtension : PrefabExtensionSetAttributePatch
{
    public override List<Attribute> Attributes => [new Attribute("MultiplyResult", "0.75")];
}

[PrefabExtension(
    "KingdomManagement",
    "descendant::Constant[@Name='Header.Tab.Right.Width.Scaled']")]
internal sealed class KingdomRightTabWidthExtension : PrefabExtensionSetAttributePatch
{
    public override List<Attribute> Attributes => [new Attribute("MultiplyResult", "0.75")];
}
