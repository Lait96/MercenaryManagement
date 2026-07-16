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
                <Widget DataSource="{MercenaryContract}" WidthSizePolicy="StretchToParent" HeightSizePolicy="StretchToParent" MarginLeft="80" MarginRight="380" MarginTop="45" MarginBottom="45">
                  <Children>
                    <TextWidget WidthSizePolicy="StretchToParent" HeightSizePolicy="StretchToParent" HorizontalAlignment="Center" VerticalAlignment="Center" Brush="Kingdom.TitleMedium.Text" Brush.FontSize="30" Text="@UnavailableReason" IsVisible="@IsUnavailable" />
                    <TextWidget WidthSizePolicy="StretchToParent" HeightSizePolicy="StretchToParent" HorizontalAlignment="Center" VerticalAlignment="Center" Brush="Kingdom.TitleMedium.Text" Brush.FontSize="30" Text="@NoAvailableOffersText" IsVisible="@HasNoAvailableOffers" />
                    <ListPanel WidthSizePolicy="StretchToParent" HeightSizePolicy="StretchToParent" StackLayout.LayoutMethod="HorizontalLeftToRight" IsVisible="@HasAvailableOffers">
                      <Children>
                        <BrushListPanel WidthSizePolicy="Fixed" HeightSizePolicy="StretchToParent" SuggestedWidth="950" MarginRight="18" Brush="Frame1Brush" StackLayout.LayoutMethod="VerticalTopToBottom">
                          <Children>
                            <TextWidget WidthSizePolicy="StretchToParent" HeightSizePolicy="Fixed" SuggestedHeight="44" HorizontalAlignment="Center" Brush="Kingdom.TitleMedium.Text" Brush.FontSize="25" Text="@AvailableOffersTitleText" />
                            <TextWidget WidthSizePolicy="StretchToParent" HeightSizePolicy="Fixed" SuggestedHeight="32" HorizontalAlignment="Center" Brush="Kingdom.ParagraphSmall.Text" Brush.FontSize="17" Text="@AvailableOffersHintText" />
                            <ListPanel WidthSizePolicy="StretchToParent" HeightSizePolicy="Fixed" SuggestedHeight="36" MarginLeft="8" MarginRight="28" StackLayout.LayoutMethod="HorizontalLeftToRight">
                              <Children>
                                <Widget WidthSizePolicy="Fixed" HeightSizePolicy="StretchToParent" SuggestedWidth="58" />
                                <TextWidget WidthSizePolicy="Fixed" HeightSizePolicy="StretchToParent" SuggestedWidth="185" Brush="Kingdom.ParagraphSmall.Text" Brush.FontSize="16" Text="@KingdomText" />
                                <TextWidget WidthSizePolicy="Fixed" HeightSizePolicy="StretchToParent" SuggestedWidth="115" Brush="Kingdom.ParagraphSmall.Text" Brush.FontSize="16" Text="@RulerText" />
                                <TextWidget WidthSizePolicy="Fixed" HeightSizePolicy="StretchToParent" SuggestedWidth="150" Brush="Kingdom.ParagraphSmall.Text" Brush.FontSize="16" Text="@CurrentWarsText" />
                                <TextWidget WidthSizePolicy="Fixed" HeightSizePolicy="StretchToParent" SuggestedWidth="190" Brush="Kingdom.ParagraphSmall.Text" Brush.FontSize="16" Text="@StrategicPriorityTitleText" />
                                <TextWidget WidthSizePolicy="Fixed" HeightSizePolicy="StretchToParent" SuggestedWidth="115" HorizontalAlignment="Center" Brush="Kingdom.ParagraphSmall.Text" Brush.FontSize="16" Text="@OfferRateText" />
                                <TextWidget WidthSizePolicy="Fixed" HeightSizePolicy="StretchToParent" SuggestedWidth="100" HorizontalAlignment="Center" Brush="Kingdom.ParagraphSmall.Text" Brush.FontSize="16" Text="@RelationToRulerText" />
                              </Children>
                            </ListPanel>
                            <Widget WidthSizePolicy="StretchToParent" HeightSizePolicy="StretchToParent">
                              <Children>
                                <ScrollablePanel WidthSizePolicy="StretchToParent" HeightSizePolicy="StretchToParent" MarginRight="20" AutoHideScrollBars="true" MouseScrollAxis="Vertical" ClipRect="OfferClipRect" InnerPanel="OfferClipRect\OfferList" VerticalScrollbar="..\OfferScrollbar\Scrollbar">
                                  <Children>
                                    <Widget Id="OfferClipRect" WidthSizePolicy="StretchToParent" HeightSizePolicy="StretchToParent" ClipContents="true">
                                      <Children>
                                        <ListPanel Id="OfferList" DataSource="{AvailableOffers}" WidthSizePolicy="StretchToParent" HeightSizePolicy="CoverChildren" MarginLeft="8" MarginRight="8" StackLayout.LayoutMethod="VerticalTopToBottom">
                                          <ItemTemplate>
                                            <ButtonWidget WidthSizePolicy="StretchToParent" HeightSizePolicy="Fixed" SuggestedHeight="78" MarginBottom="5" Brush="Clan.Item.Tuple" Command.Click="ExecuteSelect" IsSelected="@IsSelected" UpdateChildrenStates="true" DoNotPassEventsToChildren="true">
                                              <Children>
                                                <Widget WidthSizePolicy="StretchToParent" HeightSizePolicy="StretchToParent" DoNotAcceptEvents="true">
                                                  <Children>
                                                    <MaskedTextureWidget DataSource="{Banner}" WidthSizePolicy="Fixed" HeightSizePolicy="Fixed" SuggestedWidth="48" SuggestedHeight="62" MarginLeft="5" MarginTop="8" HorizontalAlignment="Left" VerticalAlignment="Top" Brush="Flat.Tuple.Banner.Small" AdditionalArgs="@AdditionalArgs" ImageId="@Id" TextureProviderName="@TextureProviderName" IsDisabled="true" />
                                                    <ListPanel WidthSizePolicy="StretchToParent" HeightSizePolicy="StretchToParent" MarginLeft="58" StackLayout.LayoutMethod="HorizontalLeftToRight">
                                                      <Children>
                                                        <TextWidget WidthSizePolicy="Fixed" HeightSizePolicy="StretchToParent" SuggestedWidth="185" Brush="ArmyManagement.Army.Tuple.Name" Brush.FontSize="18" Text="@KingdomName" />
                                                        <TextWidget WidthSizePolicy="Fixed" HeightSizePolicy="StretchToParent" SuggestedWidth="115" Brush="ArmyManagement.Army.Tuple.Name" Brush.FontSize="17" Text="@RulerName" />
                                                        <TextWidget WidthSizePolicy="Fixed" HeightSizePolicy="StretchToParent" SuggestedWidth="150" Brush="Kingdom.ParagraphSmall.Text" Brush.FontSize="16" Text="@CurrentWars" />
                                                        <TextWidget WidthSizePolicy="Fixed" HeightSizePolicy="StretchToParent" SuggestedWidth="190" Brush="ArmyManagement.Army.Tuple.Name" Brush.FontSize="17" Text="@StrategicPriority" />
                                                        <TextWidget WidthSizePolicy="Fixed" HeightSizePolicy="StretchToParent" SuggestedWidth="115" HorizontalAlignment="Center" Brush="ArmyManagement.Army.Tuple.Name" Brush.FontSize="19" IntText="@InfluencePrice" />
                                                        <TextWidget WidthSizePolicy="Fixed" HeightSizePolicy="StretchToParent" SuggestedWidth="100" HorizontalAlignment="Center" Brush="ArmyManagement.Army.Tuple.Name" Brush.FontSize="19" IntText="@RelationToRuler" />
                                                      </Children>
                                                    </ListPanel>
                                                  </Children>
                                                </Widget>
                                              </Children>
                                            </ButtonWidget>
                                          </ItemTemplate>
                                        </ListPanel>
                                      </Children>
                                    </Widget>
                                  </Children>
                                </ScrollablePanel>
                                <Standard.VerticalScrollbar Id="OfferScrollbar" HeightSizePolicy="StretchToParent" HorizontalAlignment="Right" MarginTop="4" MarginBottom="4" />
                              </Children>
                            </Widget>
                          </Children>
                        </BrushListPanel>
                        <Widget WidthSizePolicy="StretchToParent" HeightSizePolicy="StretchToParent" IsVisible="@HasSelectedOffer">
                          <Children>
                          <BrushListPanel DataSource="{SelectedOffer}" WidthSizePolicy="StretchToParent" HeightSizePolicy="StretchToParent" Brush="Frame1Brush" StackLayout.LayoutMethod="VerticalTopToBottom">
                          <Children>
                            <ListPanel WidthSizePolicy="StretchToParent" HeightSizePolicy="Fixed" SuggestedHeight="184" StackLayout.LayoutMethod="VerticalTopToBottom"><Children><Widget WidthSizePolicy="StretchToParent" HeightSizePolicy="Fixed" SuggestedHeight="94"><Children><MaskedTextureWidget DataSource="{Banner}" WidthSizePolicy="Fixed" HeightSizePolicy="Fixed" SuggestedWidth="70" SuggestedHeight="88" HorizontalAlignment="Center" VerticalAlignment="Center" Brush="Flat.Tuple.Banner.Small" AdditionalArgs="@AdditionalArgs" ImageId="@Id" TextureProviderName="@TextureProviderName" IsDisabled="true" /></Children></Widget><TextWidget WidthSizePolicy="StretchToParent" HeightSizePolicy="Fixed" SuggestedHeight="52" HorizontalAlignment="Center" Brush="Kingdom.TitleMedium.Text" Brush.FontSize="30" Text="@KingdomName" /><TextWidget WidthSizePolicy="StretchToParent" HeightSizePolicy="Fixed" SuggestedHeight="34" HorizontalAlignment="Center" Brush="Kingdom.ParagraphSmall.Text" Brush.FontSize="19" Text="@RulerName" /></Children></ListPanel>
                            <TextWidget WidthSizePolicy="StretchToParent" HeightSizePolicy="Fixed" SuggestedHeight="44" MarginLeft="20" MarginRight="20" Brush="Kingdom.TitleMedium.Text" Brush.FontSize="23" Text="@CurrentWarsText" />
                            <RichTextWidget WidthSizePolicy="StretchToParent" HeightSizePolicy="CoverChildren" MinHeight="65" MarginLeft="20" MarginRight="20" Brush="Kingdom.ParagraphSmall.Text" Brush.FontSize="19" Text="@CurrentWars" />
                            <ListPanel WidthSizePolicy="StretchToParent" HeightSizePolicy="Fixed" SuggestedHeight="50" MarginLeft="20" MarginRight="20"><Children><TextWidget WidthSizePolicy="StretchToParent" HeightSizePolicy="StretchToParent" Brush="Kingdom.ParagraphSmall.Text" Text="@InfluencePriceText" /><TextWidget WidthSizePolicy="Fixed" HeightSizePolicy="StretchToParent" SuggestedWidth="180" HorizontalAlignment="Right" Brush="ArmyManagement.Army.Tuple.Name" IntText="@InfluencePrice" /></Children></ListPanel>
                            <ListPanel WidthSizePolicy="StretchToParent" HeightSizePolicy="Fixed" SuggestedHeight="50" MarginLeft="20" MarginRight="20"><Children><TextWidget WidthSizePolicy="StretchToParent" HeightSizePolicy="StretchToParent" Brush="Kingdom.ParagraphSmall.Text" Text="@RelationToRulerText" /><TextWidget WidthSizePolicy="Fixed" HeightSizePolicy="StretchToParent" SuggestedWidth="180" HorizontalAlignment="Right" Brush="ArmyManagement.Army.Tuple.Name" IntText="@RelationToRuler" /></Children></ListPanel>
                            <TextWidget WidthSizePolicy="StretchToParent" HeightSizePolicy="Fixed" SuggestedHeight="52" MarginLeft="20" MarginRight="20" MarginTop="12" Brush="Kingdom.TitleMedium.Text" Brush.FontSize="23" Text="@StrategicPriority" />
                            <RichTextWidget WidthSizePolicy="StretchToParent" HeightSizePolicy="CoverChildren" MinHeight="90" MarginLeft="20" MarginRight="20" Brush="Kingdom.ParagraphSmall.Text" Brush.FontSize="19" Text="@StrategicPriorityDescription" />
                          </Children>
                        </BrushListPanel>
                        </Children>
                        </Widget>
                      </Children>
                    </ListPanel>
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
