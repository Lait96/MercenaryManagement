using System.Xml;
using Bannerlord.UIExtenderEx.Attributes;
using Bannerlord.UIExtenderEx.Prefabs2;

namespace MercenaryManagement.UI;

[PrefabExtension(
    "KingdomManagement",
    "descendant::KingdomTabControlListPanel/Children")]
internal sealed class KingdomMercenariesTabExtension : PrefabExtensionInsertPatch
{
    private readonly XmlDocument _document = new XmlDocument();

    public KingdomMercenariesTabExtension()
    {
        _document.LoadXml(
            """
            <ButtonWidget Id="MercenariesTabButton"
                          DoNotPassEventsToChildren="true"
                          WidthSizePolicy="CoverChildren"
                          HeightSizePolicy="Fixed"
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
                            MarginLeft="24"
                            MarginRight="24"
                            HorizontalAlignment="Center"
                            VerticalAlignment="Center"
                            Brush="Clan.TabControl.Text"
                            Text="@MercenariesText" />
              </Children>
            </ButtonWidget>
            """);
    }

    public override InsertType Type => InsertType.Child;

    [PrefabExtensionXmlNode(false)]
    public XmlNode GetPrefabExtension() => _document;
}
