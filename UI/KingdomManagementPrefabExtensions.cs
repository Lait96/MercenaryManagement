using System.Collections.Generic;
using System.Xml;
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

[PrefabExtension(
    "KingdomManagement",
    "descendant::KingdomTabControlListPanel/Children/ButtonWidget[@Id='DiplomacyTabButton']")]
internal sealed class KingdomMercenariesTabExtension : PrefabExtensionInsertPatch
{
    private readonly XmlDocument _document = new XmlDocument();

    public KingdomMercenariesTabExtension()
    {
        _document.LoadXml(
            """
            <ButtonWidget Id="MercenariesTabButton"
                          DoNotPassEventsToChildren="true"
                          WidthSizePolicy="Fixed"
                          HeightSizePolicy="Fixed"
                          SuggestedWidth="!Header.Tab.Center.Width.Scaled"
                          SuggestedHeight="!Header.Tab.Center.Height.Scaled"
                          VerticalAlignment="Center"
                          PositionYOffset="2"
                          Brush="Header.Tab.Center"
                          Command.Click="ExecuteShowMercenaries"
                          IsSelected="@IsMercenariesSelected"
                          UpdateChildrenStates="true">
              <Children>
                <TextWidget WidthSizePolicy="CoverChildren"
                            HeightSizePolicy="CoverChildren"
                            HorizontalAlignment="Center"
                            VerticalAlignment="Center"
                            Brush="Clan.TabControl.Text"
                            Text="@MercenariesText" />
              </Children>
            </ButtonWidget>
            """);
    }

    public override InsertType Type => InsertType.Prepend;

    [PrefabExtensionXmlNode(false)]
    public XmlNode GetPrefabExtension() => _document;
}

[PrefabExtension(
    "KingdomManagement",
    "descendant::Window/Widget/Children/Standard.DialogCloseButtons")]
internal sealed class KingdomMercenariesPanelExtension : PrefabExtensionInsertPatch
{
    private readonly XmlDocument _document = new XmlDocument();

    public KingdomMercenariesPanelExtension()
    {
        _document.LoadXml(
            """
            <Widget Id="MercenariesPanel"
                    WidthSizePolicy="StretchToParent"
                    HeightSizePolicy="StretchToParent"
                    MarginTop="188"
                    MarginBottom="75"
                    IsVisible="@IsMercenariesSelected">
              <Children>
                <TextWidget WidthSizePolicy="CoverChildren"
                            HeightSizePolicy="CoverChildren"
                            HorizontalAlignment="Center"
                            VerticalAlignment="Center"
                            Brush="Kingdom.NameTitle.Text"
                            Text="@MercenariesText" />
              </Children>
            </Widget>
            """);
    }

    public override InsertType Type => InsertType.Prepend;

    [PrefabExtensionXmlNode(false)]
    public XmlNode GetPrefabExtension() => _document;
}
