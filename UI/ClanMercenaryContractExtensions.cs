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
                          Command.Click="SetSelectedCategory"
                          CommandParameter.Click="4"
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
                <Widget DataSource="{MercenaryContract}" WidthSizePolicy="StretchToParent" HeightSizePolicy="StretchToParent" MarginLeft="35" MarginRight="430" MarginTop="45" MarginBottom="45">
                  <Children>
                    <TextWidget WidthSizePolicy="StretchToParent" HeightSizePolicy="StretchToParent" HorizontalAlignment="Center" VerticalAlignment="Center" Brush="Kingdom.TitleMedium.Text" Brush.FontSize="30" Text="@UnavailableReason" IsVisible="@IsUnavailable" />
                    <TextWidget WidthSizePolicy="StretchToParent" HeightSizePolicy="StretchToParent" MarginBottom="90" HorizontalAlignment="Center" VerticalAlignment="Center" Brush="Kingdom.TitleMedium.Text" Brush.FontSize="30" Text="@NoAvailableOffersText" IsVisible="@ShouldShowNoAvailableOffers" />
                    <ButtonWidget WidthSizePolicy="Fixed" HeightSizePolicy="Fixed" SuggestedWidth="360" SuggestedHeight="48" MarginBottom="18" HorizontalAlignment="Center" VerticalAlignment="Bottom" Brush="ButtonBrush2" Command.Click="ExecuteShowOfferServices" IsVisible="@CanOfferServices"><Children><TextWidget DoNotAcceptEvents="true" WidthSizePolicy="StretchToParent" HeightSizePolicy="StretchToParent" HorizontalAlignment="Center" VerticalAlignment="Center" Brush="Kingdom.GeneralButtons.Text" Brush.FontSize="19" Text="@OfferServicesText" /></Children></ButtonWidget>
                    <ListPanel WidthSizePolicy="StretchToParent" HeightSizePolicy="StretchToParent" StackLayout.LayoutMethod="HorizontalLeftToRight" IsVisible="@ShouldShowAvailableOffers">
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
                            <ListPanel WidthSizePolicy="StretchToParent" HeightSizePolicy="Fixed" SuggestedHeight="126" StackLayout.LayoutMethod="VerticalTopToBottom"><Children><Widget WidthSizePolicy="StretchToParent" HeightSizePolicy="Fixed" SuggestedHeight="62"><Children><MaskedTextureWidget DataSource="{Banner}" WidthSizePolicy="Fixed" HeightSizePolicy="Fixed" SuggestedWidth="48" SuggestedHeight="60" HorizontalAlignment="Center" VerticalAlignment="Center" Brush="Flat.Tuple.Banner.Small" AdditionalArgs="@AdditionalArgs" ImageId="@Id" TextureProviderName="@TextureProviderName" IsDisabled="true" /></Children></Widget><TextWidget WidthSizePolicy="StretchToParent" HeightSizePolicy="Fixed" SuggestedHeight="38" HorizontalAlignment="Center" Brush="Kingdom.TitleMedium.Text" Brush.FontSize="27" Text="@KingdomName" /><TextWidget WidthSizePolicy="StretchToParent" HeightSizePolicy="Fixed" SuggestedHeight="26" HorizontalAlignment="Center" Brush="Kingdom.ParagraphSmall.Text" Brush.FontSize="18" Text="@RulerName" /></Children></ListPanel>
                            <TextWidget WidthSizePolicy="StretchToParent" HeightSizePolicy="Fixed" SuggestedHeight="32" MarginLeft="20" MarginRight="20" Brush="Kingdom.TitleMedium.Text" Brush.FontSize="21" Text="@CurrentWarsText" />
                            <RichTextWidget WidthSizePolicy="StretchToParent" HeightSizePolicy="CoverChildren" MinHeight="38" MarginLeft="20" MarginRight="20" Brush="Kingdom.ParagraphSmall.Text" Brush.FontSize="18" Text="@CurrentWars" />
                            <ListPanel WidthSizePolicy="StretchToParent" HeightSizePolicy="Fixed" SuggestedHeight="40" MarginLeft="20" MarginRight="20"><Children><TextWidget WidthSizePolicy="StretchToParent" HeightSizePolicy="StretchToParent" Brush="Kingdom.ParagraphSmall.Text" Text="@InfluencePriceText" /><TextWidget WidthSizePolicy="Fixed" HeightSizePolicy="StretchToParent" SuggestedWidth="180" HorizontalAlignment="Right" Brush="ArmyManagement.Army.Tuple.Name" IntText="@InfluencePrice" /></Children></ListPanel>
                            <ListPanel WidthSizePolicy="StretchToParent" HeightSizePolicy="Fixed" SuggestedHeight="40" MarginLeft="20" MarginRight="20"><Children><TextWidget WidthSizePolicy="StretchToParent" HeightSizePolicy="StretchToParent" Brush="Kingdom.ParagraphSmall.Text" Text="@RelationToRulerText" /><TextWidget WidthSizePolicy="Fixed" HeightSizePolicy="StretchToParent" SuggestedWidth="180" HorizontalAlignment="Right" Brush="ArmyManagement.Army.Tuple.Name" IntText="@RelationToRuler" /></Children></ListPanel>
                            <ListPanel WidthSizePolicy="StretchToParent" HeightSizePolicy="Fixed" SuggestedHeight="40" MarginLeft="20" MarginRight="20"><Children><TextWidget WidthSizePolicy="StretchToParent" HeightSizePolicy="StretchToParent" Brush="Kingdom.ParagraphSmall.Text" Text="@DurationText" /><TextWidget WidthSizePolicy="Fixed" HeightSizePolicy="StretchToParent" SuggestedWidth="220" HorizontalAlignment="Right" Brush="ArmyManagement.Army.Tuple.Name" Text="@ExpectedDuration" /></Children></ListPanel>
                            <ListPanel WidthSizePolicy="StretchToParent" HeightSizePolicy="Fixed" SuggestedHeight="40" MarginLeft="20" MarginRight="20"><Children><TextWidget WidthSizePolicy="StretchToParent" HeightSizePolicy="StretchToParent" Brush="Kingdom.ParagraphSmall.Text" Text="@DangerText" /><TextWidget WidthSizePolicy="Fixed" HeightSizePolicy="StretchToParent" SuggestedWidth="220" HorizontalAlignment="Right" Brush="ArmyManagement.Army.Tuple.Name" Text="@EstimatedDanger" /></Children></ListPanel>
                            <TextWidget WidthSizePolicy="StretchToParent" HeightSizePolicy="Fixed" SuggestedHeight="30" MarginLeft="20" MarginRight="20" Brush="Kingdom.TitleMedium.Text" Brush.FontSize="19" Text="@InterestText" />
                            <RichTextWidget WidthSizePolicy="StretchToParent" HeightSizePolicy="CoverChildren" MinHeight="44" MarginLeft="20" MarginRight="20" Brush="Kingdom.ParagraphSmall.Text" Brush.FontSize="17" Text="@InterestReason" />
                            <TextWidget WidthSizePolicy="StretchToParent" HeightSizePolicy="Fixed" SuggestedHeight="38" MarginLeft="20" MarginRight="20" MarginTop="6" Brush="Kingdom.TitleMedium.Text" Brush.FontSize="21" Text="@StrategicPriority" />
                            <RichTextWidget WidthSizePolicy="StretchToParent" HeightSizePolicy="CoverChildren" MinHeight="54" MarginLeft="20" MarginRight="20" Brush="Kingdom.ParagraphSmall.Text" Brush.FontSize="18" Text="@StrategicPriorityDescription" />
                            <ButtonWidget DataSource="{..}" DoNotPassEventsToChildren="true" UpdateChildrenStates="true" WidthSizePolicy="Fixed" HeightSizePolicy="Fixed" SuggestedWidth="360" SuggestedHeight="48" MarginTop="10" HorizontalAlignment="Center" Brush="ButtonBrush2" Command.Click="ExecuteAcceptOffer"><Children><TextWidget DoNotAcceptEvents="true" WidthSizePolicy="StretchToParent" HeightSizePolicy="StretchToParent" HorizontalAlignment="Center" VerticalAlignment="Center" Brush="Kingdom.GeneralButtons.Text" Brush.FontSize="19" Text="@AcceptOfferText" /></Children></ButtonWidget>
                            <ButtonWidget DataSource="{..}" DoNotPassEventsToChildren="true" UpdateChildrenStates="true" WidthSizePolicy="Fixed" HeightSizePolicy="Fixed" SuggestedWidth="360" SuggestedHeight="42" MarginTop="6" MarginBottom="12" HorizontalAlignment="Center" Brush="ButtonBrush2" Command.Click="ExecuteShowOfferServices" IsVisible="@CanOfferServices"><Children><TextWidget DoNotAcceptEvents="true" WidthSizePolicy="StretchToParent" HeightSizePolicy="StretchToParent" HorizontalAlignment="Center" VerticalAlignment="Center" Brush="Kingdom.GeneralButtons.Text" Brush.FontSize="18" Text="@OfferServicesText" /></Children></ButtonWidget>
                          </Children>
                        </BrushListPanel>
                        </Children>
                        </Widget>
                      </Children>
                    </ListPanel>
                    <ListPanel WidthSizePolicy="StretchToParent" HeightSizePolicy="StretchToParent" StackLayout.LayoutMethod="HorizontalLeftToRight" IsVisible="@IsOfferingServices">
                      <Children>
                        <BrushListPanel WidthSizePolicy="Fixed" HeightSizePolicy="StretchToParent" SuggestedWidth="720" MarginRight="18" Brush="Frame1Brush" StackLayout.LayoutMethod="VerticalTopToBottom">
                          <Children>
                            <TextWidget WidthSizePolicy="StretchToParent" HeightSizePolicy="Fixed" SuggestedHeight="52" HorizontalAlignment="Center" Brush="Kingdom.TitleMedium.Text" Brush.FontSize="26" Text="@OfferServicesTitleText" />
                            <RichTextWidget WidthSizePolicy="StretchToParent" HeightSizePolicy="CoverChildren" MinHeight="55" MarginLeft="20" MarginRight="20" Brush="Kingdom.ParagraphSmall.Text" Brush.FontSize="17" Text="@OfferServicesHintText" />
                            <Widget WidthSizePolicy="StretchToParent" HeightSizePolicy="StretchToParent">
                              <Children>
                                <ScrollablePanel WidthSizePolicy="StretchToParent" HeightSizePolicy="StretchToParent" MarginLeft="12" MarginRight="24" AutoHideScrollBars="true" MouseScrollAxis="Vertical" ClipRect="ServiceKingdomClipRect" InnerPanel="ServiceKingdomClipRect\ServiceKingdomList" VerticalScrollbar="..\ServiceKingdomScrollbar\Scrollbar">
                                  <Children>
                                    <Widget Id="ServiceKingdomClipRect" WidthSizePolicy="StretchToParent" HeightSizePolicy="StretchToParent" ClipContents="true">
                                      <Children>
                                        <ListPanel Id="ServiceKingdomList" DataSource="{ServiceKingdoms}" WidthSizePolicy="StretchToParent" HeightSizePolicy="CoverChildren" StackLayout.LayoutMethod="VerticalTopToBottom">
                                          <ItemTemplate>
                                            <ButtonWidget WidthSizePolicy="StretchToParent" HeightSizePolicy="Fixed" SuggestedHeight="70" MarginBottom="5" Brush="Clan.Item.Tuple" Command.Click="ExecuteSelect" IsSelected="@IsSelected" UpdateChildrenStates="true" DoNotPassEventsToChildren="true">
                                              <Children>
                                                <MaskedTextureWidget DataSource="{Banner}" WidthSizePolicy="Fixed" HeightSizePolicy="Fixed" SuggestedWidth="44" SuggestedHeight="56" MarginLeft="8" MarginTop="7" Brush="Flat.Tuple.Banner.Small" AdditionalArgs="@AdditionalArgs" ImageId="@Id" TextureProviderName="@TextureProviderName" IsDisabled="true" />
                                                <TextWidget WidthSizePolicy="StretchToParent" HeightSizePolicy="StretchToParent" MarginLeft="65" MarginRight="220" Brush="ArmyManagement.Army.Tuple.Name" Brush.FontSize="19" Text="@KingdomName" />
                                                <TextWidget WidthSizePolicy="Fixed" HeightSizePolicy="StretchToParent" SuggestedWidth="150" MarginRight="55" HorizontalAlignment="Right" Brush="Kingdom.ParagraphSmall.Text" Brush.FontSize="17" Text="@RulerName" />
                                                <TextWidget WidthSizePolicy="Fixed" HeightSizePolicy="StretchToParent" SuggestedWidth="45" MarginRight="8" HorizontalAlignment="Right" Brush="ArmyManagement.Army.Tuple.Name" Brush.FontSize="19" IntText="@RelationToRuler" />
                                              </Children>
                                            </ButtonWidget>
                                          </ItemTemplate>
                                        </ListPanel>
                                      </Children>
                                    </Widget>
                                  </Children>
                                </ScrollablePanel>
                                <Standard.VerticalScrollbar Id="ServiceKingdomScrollbar" HeightSizePolicy="StretchToParent" HorizontalAlignment="Right" MarginTop="4" MarginBottom="4" />
                              </Children>
                            </Widget>
                          </Children>
                        </BrushListPanel>
                        <BrushListPanel WidthSizePolicy="StretchToParent" HeightSizePolicy="StretchToParent" Brush="Frame1Brush" StackLayout.LayoutMethod="VerticalTopToBottom">
                          <Children>
                            <TextWidget DataSource="{SelectedServiceKingdom}" WidthSizePolicy="StretchToParent" HeightSizePolicy="Fixed" SuggestedHeight="90" MarginTop="80" HorizontalAlignment="Center" Brush="Kingdom.TitleMedium.Text" Brush.FontSize="30" Text="@KingdomName" />
                            <ButtonWidget WidthSizePolicy="Fixed" HeightSizePolicy="Fixed" SuggestedWidth="360" SuggestedHeight="48" MarginTop="20" HorizontalAlignment="Center" Brush="ButtonBrush2" Command.Click="ExecuteSubmitServiceRequest" IsVisible="@HasSelectedServiceKingdom"><Children><TextWidget DoNotAcceptEvents="true" WidthSizePolicy="StretchToParent" HeightSizePolicy="StretchToParent" HorizontalAlignment="Center" VerticalAlignment="Center" Brush="Kingdom.GeneralButtons.Text" Brush.FontSize="19" Text="@SubmitServiceRequestText" /></Children></ButtonWidget>
                            <ButtonWidget WidthSizePolicy="Fixed" HeightSizePolicy="Fixed" SuggestedWidth="360" SuggestedHeight="42" MarginTop="10" HorizontalAlignment="Center" Brush="ButtonBrush2" Command.Click="ExecuteBackToOffers"><Children><TextWidget DoNotAcceptEvents="true" WidthSizePolicy="StretchToParent" HeightSizePolicy="StretchToParent" HorizontalAlignment="Center" VerticalAlignment="Center" Brush="Kingdom.GeneralButtons.Text" Brush.FontSize="18" Text="@BackToOffersText" /></Children></ButtonWidget>
                          </Children>
                        </BrushListPanel>
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
                                <ListPanel WidthSizePolicy="StretchToParent" HeightSizePolicy="Fixed" SuggestedHeight="48"><Children><TextWidget WidthSizePolicy="StretchToParent" HeightSizePolicy="StretchToParent" Brush="Kingdom.ParagraphSmall.Text" Text="@ExpectedPaymentText" /><TextWidget WidthSizePolicy="Fixed" HeightSizePolicy="StretchToParent" SuggestedWidth="520" HorizontalAlignment="Right" Brush="ArmyManagement.Army.Tuple.Name" IntText="@ExpectedPayment" /></Children></ListPanel>
                                <ListPanel WidthSizePolicy="StretchToParent" HeightSizePolicy="Fixed" SuggestedHeight="48"><Children><TextWidget WidthSizePolicy="StretchToParent" HeightSizePolicy="StretchToParent" Brush="Kingdom.ParagraphSmall.Text" Text="@SatisfactionText" /><TextWidget WidthSizePolicy="Fixed" HeightSizePolicy="StretchToParent" SuggestedWidth="520" HorizontalAlignment="Right" Brush="ArmyManagement.Army.Tuple.Name" IntText="@Satisfaction" /></Children></ListPanel>
                                <ListPanel WidthSizePolicy="StretchToParent" HeightSizePolicy="Fixed" SuggestedHeight="48"><Children><TextWidget WidthSizePolicy="StretchToParent" HeightSizePolicy="StretchToParent" Brush="Kingdom.ParagraphSmall.Text" Text="@EmployerTerminationRiskText" /><TextWidget WidthSizePolicy="Fixed" HeightSizePolicy="StretchToParent" SuggestedWidth="520" HorizontalAlignment="Right" Brush="ArmyManagement.Army.Tuple.Name" IntText="@EmployerTerminationRisk" /></Children></ListPanel>
                                <ListPanel WidthSizePolicy="StretchToParent" HeightSizePolicy="Fixed" SuggestedHeight="48"><Children><TextWidget WidthSizePolicy="StretchToParent" HeightSizePolicy="StretchToParent" Brush="Kingdom.ParagraphSmall.Text" Text="@PerformanceText" /><TextWidget WidthSizePolicy="Fixed" HeightSizePolicy="StretchToParent" SuggestedWidth="520" HorizontalAlignment="Right" Brush="ArmyManagement.Army.Tuple.Name" Text="@Performance" /></Children></ListPanel>
                                <ListPanel WidthSizePolicy="StretchToParent" HeightSizePolicy="Fixed" SuggestedHeight="48"><Children><TextWidget WidthSizePolicy="StretchToParent" HeightSizePolicy="StretchToParent" Brush="Kingdom.ParagraphSmall.Text" Text="@RecentEarningsText" /><TextWidget WidthSizePolicy="Fixed" HeightSizePolicy="StretchToParent" SuggestedWidth="520" HorizontalAlignment="Right" Brush="ArmyManagement.Army.Tuple.Name" IntText="@RecentEarnings" /></Children></ListPanel>
                                <ListPanel WidthSizePolicy="StretchToParent" HeightSizePolicy="CoverChildren" MarginTop="24" MarginBottom="30" StackLayout.LayoutMethod="VerticalTopToBottom"><Children><ButtonWidget DoNotPassEventsToChildren="true" UpdateChildrenStates="true" WidthSizePolicy="Fixed" HeightSizePolicy="Fixed" SuggestedWidth="430" SuggestedHeight="48" HorizontalAlignment="Center" Brush="ButtonBrush2" Command.Click="ExecuteRequestRevision"><Children><TextWidget DoNotAcceptEvents="true" WidthSizePolicy="StretchToParent" HeightSizePolicy="StretchToParent" HorizontalAlignment="Center" VerticalAlignment="Center" Brush="Kingdom.GeneralButtons.Text" Brush.FontSize="19" Text="@RequestRevisionText" /></Children></ButtonWidget><ButtonWidget DoNotPassEventsToChildren="true" UpdateChildrenStates="true" WidthSizePolicy="Fixed" HeightSizePolicy="Fixed" SuggestedWidth="430" SuggestedHeight="48" MarginTop="12" HorizontalAlignment="Center" Brush="ButtonBrush2" Command.Click="ExecuteEndContract"><Children><TextWidget DoNotAcceptEvents="true" WidthSizePolicy="StretchToParent" HeightSizePolicy="StretchToParent" HorizontalAlignment="Center" VerticalAlignment="Center" Brush="Kingdom.GeneralButtons.Text" Brush.FontSize="19" Text="@EndContractText" /></Children></ButtonWidget></Children></ListPanel>
                              </Children>
                            </ListPanel>
                          </Children>
                        </Widget>
                      </Children>
                    </ScrollablePanel>
                    <Standard.VerticalScrollbar Id="ContractScrollbar" HeightSizePolicy="StretchToParent" HorizontalAlignment="Right" IsVisible="@HasActiveContract" />
                    <BrushListPanel WidthSizePolicy="StretchToParent" HeightSizePolicy="StretchToParent" Brush="Frame1Brush" StackLayout.LayoutMethod="VerticalTopToBottom" IsVisible="@IsShowingHistory">
                      <Children>
                        <TextWidget WidthSizePolicy="StretchToParent" HeightSizePolicy="Fixed" SuggestedHeight="54" HorizontalAlignment="Center" Brush="Kingdom.TitleMedium.Text" Brush.FontSize="27" Text="@HistoryText" />
                        <TextWidget WidthSizePolicy="StretchToParent" HeightSizePolicy="StretchToParent" HorizontalAlignment="Center" VerticalAlignment="Center" Brush="Kingdom.ParagraphSmall.Text" Brush.FontSize="22" Text="@NoHistoryText" IsVisible="@HasNoContractHistory" />
                        <ListPanel WidthSizePolicy="StretchToParent" HeightSizePolicy="Fixed" SuggestedHeight="44" MarginLeft="12" MarginRight="28" StackLayout.LayoutMethod="HorizontalLeftToRight" IsVisible="@HasContractHistory"><Children><TextWidget WidthSizePolicy="Fixed" HeightSizePolicy="StretchToParent" SuggestedWidth="220" MarginLeft="10" MarginRight="10" Brush="Kingdom.ParagraphSmall.Text" Text="@HistoryEmployerText" /><Widget WidthSizePolicy="Fixed" HeightSizePolicy="StretchToParent" SuggestedWidth="2" Sprite="GradientDivider_9" AlphaFactor="0.55" /><TextWidget WidthSizePolicy="Fixed" HeightSizePolicy="StretchToParent" SuggestedWidth="270" MarginLeft="10" MarginRight="10" Brush="Kingdom.ParagraphSmall.Text" Text="@HistoryPeriodText" /><Widget WidthSizePolicy="Fixed" HeightSizePolicy="StretchToParent" SuggestedWidth="2" Sprite="GradientDivider_9" AlphaFactor="0.55" /><TextWidget WidthSizePolicy="Fixed" HeightSizePolicy="StretchToParent" SuggestedWidth="220" MarginLeft="10" MarginRight="10" Brush="Kingdom.ParagraphSmall.Text" Text="@HistoryReasonText" /><Widget WidthSizePolicy="Fixed" HeightSizePolicy="StretchToParent" SuggestedWidth="2" Sprite="GradientDivider_9" AlphaFactor="0.55" /><TextWidget WidthSizePolicy="Fixed" HeightSizePolicy="StretchToParent" SuggestedWidth="220" MarginLeft="10" MarginRight="10" Brush="Kingdom.ParagraphSmall.Text" Text="@HistoryResultsText" /><Widget WidthSizePolicy="Fixed" HeightSizePolicy="StretchToParent" SuggestedWidth="2" Sprite="GradientDivider_9" AlphaFactor="0.55" /><TextWidget WidthSizePolicy="StretchToParent" HeightSizePolicy="StretchToParent" MarginLeft="10" MarginRight="10" Brush="Kingdom.ParagraphSmall.Text" Text="@HistoryRelationsText" /></Children></ListPanel>
                        <Widget WidthSizePolicy="StretchToParent" HeightSizePolicy="Fixed" SuggestedHeight="2" MarginLeft="12" MarginRight="28" Sprite="GradientDivider_9" AlphaFactor="0.7" IsVisible="@HasContractHistory" />
                        <Widget Id="HistoryScrollContainer" WidthSizePolicy="StretchToParent" HeightSizePolicy="StretchToParent" IsVisible="@HasContractHistory"><Children><ScrollablePanel WidthSizePolicy="StretchToParent" HeightSizePolicy="StretchToParent" MarginRight="20" AutoHideScrollBars="true" MouseScrollAxis="Vertical" ClipRect="HistoryClipRect" InnerPanel="HistoryClipRect\HistoryList" VerticalScrollbar="..\HistoryScrollbar\Scrollbar"><Children><Widget Id="HistoryClipRect" WidthSizePolicy="StretchToParent" HeightSizePolicy="StretchToParent" ClipContents="true"><Children><ListPanel Id="HistoryList" DataSource="{ContractHistory}" WidthSizePolicy="StretchToParent" HeightSizePolicy="CoverChildren" StackLayout.LayoutMethod="VerticalTopToBottom"><ItemTemplate><BrushListPanel WidthSizePolicy="StretchToParent" HeightSizePolicy="Fixed" SuggestedHeight="124" MarginLeft="8" MarginRight="8" MarginBottom="7" Brush="Clan.Item.Tuple" StackLayout.LayoutMethod="HorizontalLeftToRight"><Children><TextWidget WidthSizePolicy="Fixed" HeightSizePolicy="StretchToParent" SuggestedWidth="220" MarginLeft="10" MarginRight="10" Brush="ArmyManagement.Army.Tuple.Name" Brush.FontSize="17" Text="@EmployerSummary" /><Widget WidthSizePolicy="Fixed" HeightSizePolicy="StretchToParent" SuggestedWidth="2" Sprite="GradientDivider_9" AlphaFactor="0.45" /><TextWidget WidthSizePolicy="Fixed" HeightSizePolicy="StretchToParent" SuggestedWidth="270" MarginLeft="10" MarginRight="10" Brush="Kingdom.ParagraphSmall.Text" Brush.FontSize="16" Text="@ServicePeriod" /><Widget WidthSizePolicy="Fixed" HeightSizePolicy="StretchToParent" SuggestedWidth="2" Sprite="GradientDivider_9" AlphaFactor="0.45" /><TextWidget WidthSizePolicy="Fixed" HeightSizePolicy="StretchToParent" SuggestedWidth="220" MarginLeft="10" MarginRight="10" Brush="Kingdom.ParagraphSmall.Text" Brush.FontSize="16" Text="@EndReason" /><Widget WidthSizePolicy="Fixed" HeightSizePolicy="StretchToParent" SuggestedWidth="2" Sprite="GradientDivider_9" AlphaFactor="0.45" /><TextWidget WidthSizePolicy="Fixed" HeightSizePolicy="StretchToParent" SuggestedWidth="220" MarginLeft="10" MarginRight="10" Brush="Kingdom.ParagraphSmall.Text" Brush.FontSize="16" Text="@ResultsSummary" /><Widget WidthSizePolicy="Fixed" HeightSizePolicy="StretchToParent" SuggestedWidth="2" Sprite="GradientDivider_9" AlphaFactor="0.45" /><TextWidget WidthSizePolicy="StretchToParent" HeightSizePolicy="StretchToParent" MarginLeft="10" MarginRight="10" Brush="Kingdom.ParagraphSmall.Text" Brush.FontSize="16" Text="@RelationsSummary" /></Children></BrushListPanel></ItemTemplate></ListPanel></Children></Widget></Children></ScrollablePanel><Standard.VerticalScrollbar Id="HistoryScrollbar" HeightSizePolicy="StretchToParent" HorizontalAlignment="Right" MarginTop="4" MarginBottom="4" /></Children></Widget>
                      </Children>
                    </BrushListPanel>
                  </Children>
                </Widget>
                <ButtonWidget Id="ContractHistoryButton"
                              DataSource="{MercenaryContract}"
                              DoNotPassEventsToChildren="true"
                              UpdateChildrenStates="true"
                              WidthSizePolicy="Fixed"
                              HeightSizePolicy="Fixed"
                              SuggestedWidth="300"
                              SuggestedHeight="48"
                              MarginRight="10"
                              MarginTop="45"
                              HorizontalAlignment="Right"
                              VerticalAlignment="Top"
                              Brush="ButtonBrush2"
                              Command.Click="ExecuteShowHistory">
                  <Children>
                    <TextWidget DoNotAcceptEvents="true"
                                WidthSizePolicy="StretchToParent"
                                HeightSizePolicy="StretchToParent"
                                HorizontalAlignment="Center"
                                VerticalAlignment="Center"
                                Brush="Kingdom.GeneralButtons.Text"
                                Brush.FontSize="18"
                                Text="@HistoryText" />
                  </Children>
                </ButtonWidget>
              </Children>
            </Widget>
            """);
    }

    public override InsertType Type => InsertType.Prepend;

    [PrefabExtensionXmlNode(false)]
    public XmlNode GetPrefabExtension() => _document;
}
