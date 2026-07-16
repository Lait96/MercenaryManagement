using System.Xml;
using Bannerlord.UIExtenderEx.Attributes;
using Bannerlord.UIExtenderEx.Prefabs2;

namespace MercenaryManagement.UI;

[PrefabExtension("ClanScreen", "descendant::ButtonWidget[@CommandParameter.Click='3']")]
internal sealed class ClanMercenaryContractTabExtension : PrefabExtensionInsertPatch
{
    private readonly XmlDocument _document = new XmlDocument();

    public ClanMercenaryContractTabExtension()
    {
        _document.LoadXml(
            """
            <ButtonWidget Id="MercenaryContractTabButton"
                          DoNotPassEventsToChildren="true"
                          WidthSizePolicy="Fixed"
                          HeightSizePolicy="Fixed"
                          SuggestedWidth="!Header.Tab.Center.Width.Scaled"
                          SuggestedHeight="!Header.Tab.Center.Height.Scaled"
                          PositionYOffset="6"
                          Brush="Header.Tab.Center"
                          Command.Click="ExecuteShowMercenaryContract"
                          IsSelected="@IsMercenaryContractSelected"
                          IsVisible="@IsMercenaryContractTabVisible"
                          UpdateChildrenStates="true">
              <Children>
                <TextWidget WidthSizePolicy="StretchToParent" HeightSizePolicy="StretchToParent" Brush="Clan.TabControl.Text" Text="@MercenaryContractText" />
              </Children>
            </ButtonWidget>
            """);
    }

    public override InsertType Type => InsertType.Prepend;

    [PrefabExtensionXmlNode(false)]
    public XmlNode GetPrefabExtension() => _document;
}

[PrefabExtension("ClanScreen", "descendant::Standard.DialogCloseButtons")]
internal sealed class ClanMercenaryContractPanelExtension : PrefabExtensionInsertPatch
{
    private readonly XmlDocument _document = new XmlDocument();

    public ClanMercenaryContractPanelExtension()
    {
        _document.LoadXml(
            """
            <Widget Id="MercenaryContractPanel" WidthSizePolicy="StretchToParent" HeightSizePolicy="StretchToParent" MarginTop="188" MarginBottom="75" IsVisible="@IsMercenaryContractSelected">
              <Children>
                <Widget DataSource="{MercenaryContract}" WidthSizePolicy="StretchToParent" HeightSizePolicy="StretchToParent" MarginLeft="250" MarginRight="380" MarginTop="45" MarginBottom="45">
                  <Children>
                    <TextWidget WidthSizePolicy="StretchToParent" HeightSizePolicy="StretchToParent" HorizontalAlignment="Center" VerticalAlignment="Center" Brush="Kingdom.TitleMedium.Text" Brush.FontSize="30" Text="@UnavailableReason" IsVisible="@IsUnavailable" />
                    <TextWidget WidthSizePolicy="StretchToParent" HeightSizePolicy="StretchToParent" HorizontalAlignment="Center" VerticalAlignment="Center" Brush="Kingdom.TitleMedium.Text" Brush.FontSize="30" Text="@NoActiveContractText" IsVisible="@HasNoActiveContract" />
                    <ScrollablePanel WidthSizePolicy="StretchToParent" HeightSizePolicy="StretchToParent" AutoHideScrollBars="true" MouseScrollAxis="Vertical" ClipRect="ContractClipRect" InnerPanel="ContractClipRect\ContractContent" VerticalScrollbar="..\ContractScrollbar\Scrollbar" IsVisible="@HasActiveContract">
                      <Children>
                        <Widget Id="ContractClipRect" WidthSizePolicy="StretchToParent" HeightSizePolicy="StretchToParent" ClipContents="true">
                          <Children>
                            <ListPanel Id="ContractContent" WidthSizePolicy="StretchToParent" HeightSizePolicy="CoverChildren" StackLayout.LayoutMethod="VerticalTopToBottom">
                              <Children>
                                <TextWidget WidthSizePolicy="StretchToParent" HeightSizePolicy="Fixed" SuggestedHeight="58" Brush="Kingdom.TitleMedium.Text" Text="@EmployerTitleText" />
                                <ListPanel WidthSizePolicy="StretchToParent" HeightSizePolicy="Fixed" SuggestedHeight="48"><Children><TextWidget WidthSizePolicy="StretchToParent" HeightSizePolicy="StretchToParent" Brush="Kingdom.ParagraphSmall.Text" Text="@KingdomText" /><TextWidget WidthSizePolicy="Fixed" HeightSizePolicy="StretchToParent" SuggestedWidth="520" HorizontalAlignment="Right" Brush="ArmyManagement.Army.Tuple.Name" Text="@EmployerKingdom" /></Children></ListPanel>
                                <ListPanel WidthSizePolicy="StretchToParent" HeightSizePolicy="Fixed" SuggestedHeight="48"><Children><TextWidget WidthSizePolicy="StretchToParent" HeightSizePolicy="StretchToParent" Brush="Kingdom.ParagraphSmall.Text" Text="@RulerText" /><TextWidget WidthSizePolicy="Fixed" HeightSizePolicy="StretchToParent" SuggestedWidth="520" HorizontalAlignment="Right" Brush="ArmyManagement.Army.Tuple.Name" Text="@EmployerRuler" /></Children></ListPanel>
                                <TextWidget WidthSizePolicy="StretchToParent" HeightSizePolicy="Fixed" SuggestedHeight="58" MarginTop="24" Brush="Kingdom.TitleMedium.Text" Text="@ContractTitleText" />
                                <ListPanel WidthSizePolicy="StretchToParent" HeightSizePolicy="Fixed" SuggestedHeight="48"><Children><TextWidget WidthSizePolicy="StretchToParent" HeightSizePolicy="StretchToParent" Brush="Kingdom.ParagraphSmall.Text" Text="@ContractStartText" /><TextWidget WidthSizePolicy="Fixed" HeightSizePolicy="StretchToParent" SuggestedWidth="520" HorizontalAlignment="Right" Brush="ArmyManagement.Army.Tuple.Name" Text="@ContractStart" /></Children></ListPanel>
                                <ListPanel WidthSizePolicy="StretchToParent" HeightSizePolicy="Fixed" SuggestedHeight="48"><Children><TextWidget WidthSizePolicy="StretchToParent" HeightSizePolicy="StretchToParent" Brush="Kingdom.ParagraphSmall.Text" Text="@ContractEndText" /><TextWidget WidthSizePolicy="Fixed" HeightSizePolicy="StretchToParent" SuggestedWidth="520" HorizontalAlignment="Right" Brush="ArmyManagement.Army.Tuple.Name" Text="@ContractEnd" /></Children></ListPanel>
                                <ListPanel WidthSizePolicy="StretchToParent" HeightSizePolicy="Fixed" SuggestedHeight="48"><Children><TextWidget WidthSizePolicy="StretchToParent" HeightSizePolicy="StretchToParent" Brush="Kingdom.ParagraphSmall.Text" Text="@ContractAgeText" /><TextWidget WidthSizePolicy="Fixed" HeightSizePolicy="StretchToParent" SuggestedWidth="520" HorizontalAlignment="Right" Brush="ArmyManagement.Army.Tuple.Name" Text="@ContractAge" /></Children></ListPanel>
                                <ListPanel WidthSizePolicy="StretchToParent" HeightSizePolicy="Fixed" SuggestedHeight="48"><Children><TextWidget WidthSizePolicy="StretchToParent" HeightSizePolicy="StretchToParent" Brush="Kingdom.ParagraphSmall.Text" Text="@InfluencePriceText" /><TextWidget WidthSizePolicy="Fixed" HeightSizePolicy="StretchToParent" SuggestedWidth="520" HorizontalAlignment="Right" Brush="ArmyManagement.Army.Tuple.Name" IntText="@InfluencePrice" /></Children></ListPanel>
                                <TextWidget WidthSizePolicy="StretchToParent" HeightSizePolicy="Fixed" SuggestedHeight="58" MarginTop="24" Brush="Kingdom.TitleMedium.Text" Text="@StrategicPriorityTitleText" />
                                <TextWidget WidthSizePolicy="StretchToParent" HeightSizePolicy="Fixed" SuggestedHeight="48" Brush="ArmyManagement.Army.Tuple.Name" Text="@StrategicPriority" />
                                <RichTextWidget WidthSizePolicy="StretchToParent" HeightSizePolicy="CoverChildren" MinHeight="80" MarginTop="8" MarginBottom="30" Brush="Kingdom.ParagraphSmall.Text" Brush.FontSize="21" Text="@StrategicPriorityDescription" />
                              </Children>
                            </ListPanel>
                          </Children>
                        </Widget>
                      </Children>
                    </ScrollablePanel>
                    <Standard.VerticalScrollbar Id="ContractScrollbar" HeightSizePolicy="StretchToParent" HorizontalAlignment="Right" IsVisible="@HasActiveContract" />
                  </Children>
                </Widget>
              </Children>
            </Widget>
            """);
    }

    public override InsertType Type => InsertType.Prepend;

    [PrefabExtensionXmlNode(false)]
    public XmlNode GetPrefabExtension() => _document;
}
