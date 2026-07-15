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
                <Widget DataSource="{Mercenaries}"
                        WidthSizePolicy="StretchToParent"
                        HeightSizePolicy="StretchToParent"
                        MarginTop="35"
                        MarginBottom="35">
                  <Children>
                    <ListPanel WidthSizePolicy="StretchToParent"
                               HeightSizePolicy="StretchToParent"
                               StackLayout.LayoutMethod="VerticalTopToBottom">
                      <Children>
                        <TextWidget WidthSizePolicy="StretchToParent"
                                    HeightSizePolicy="Fixed"
                                    SuggestedHeight="60"
                                    HorizontalAlignment="Center"
                                    Brush="Kingdom.TitleMedium.Text"
                                    Text="@TitleText" />

                        <ListPanel WidthSizePolicy="StretchToParent"
                                   HeightSizePolicy="StretchToParent"
                                   MarginLeft="35"
                                   MarginRight="35"
                                   StackLayout.LayoutMethod="HorizontalLeftToRight"
                                   IsVisible="@HasClans">
                          <Children>
                            <BrushListPanel WidthSizePolicy="Fixed"
                                            HeightSizePolicy="StretchToParent"
                                            SuggestedWidth="930"
                                            Brush="Frame1Brush"
                                            StackLayout.LayoutMethod="VerticalTopToBottom">
                              <Children>
                                <ListPanel WidthSizePolicy="Fixed"
                                           HeightSizePolicy="Fixed"
                                           SuggestedWidth="880"
                                           SuggestedHeight="55"
                                           MarginLeft="15"
                                           StackLayout.LayoutMethod="HorizontalLeftToRight">
                                  <Children>
                                    <Widget WidthSizePolicy="Fixed" HeightSizePolicy="StretchToParent" SuggestedWidth="90" />
                                    <TextWidget WidthSizePolicy="Fixed" HeightSizePolicy="StretchToParent" SuggestedWidth="220" Brush="Kingdom.ParagraphSmall.Text" Brush.FontSize="22" Text="@ClanText" />
                                    <TextWidget WidthSizePolicy="Fixed" HeightSizePolicy="StretchToParent" SuggestedWidth="250" Brush="Kingdom.ParagraphSmall.Text" Brush.FontSize="22" Text="@LeaderText" />
                                    <TextWidget WidthSizePolicy="Fixed" HeightSizePolicy="StretchToParent" SuggestedWidth="130" HorizontalAlignment="Center" Brush="Kingdom.ParagraphSmall.Text" Brush.FontSize="22" Text="@StrengthText" />
                                    <TextWidget WidthSizePolicy="Fixed" HeightSizePolicy="StretchToParent" SuggestedWidth="190" HorizontalAlignment="Center" Brush="Kingdom.ParagraphSmall.Text" Brush.FontSize="22" Text="@ActivePartiesText" />
                                  </Children>
                                </ListPanel>

                                <ListPanel WidthSizePolicy="StretchToParent" HeightSizePolicy="StretchToParent">
                                  <Children>
                                    <ScrollablePanel WidthSizePolicy="StretchToParent"
                                                     HeightSizePolicy="StretchToParent"
                                                     MarginLeft="15"
                                                     MarginBottom="8"
                                                     AutoHideScrollBars="true"
                                                     ClipRect="MercenaryClanListClipRect"
                                                     InnerPanel="MercenaryClanListClipRect\MercenaryClanList"
                                                     VerticalScrollbar="..\MercenaryClanListScrollbar\Scrollbar">
                                      <Children>
                                        <Widget Id="MercenaryClanListClipRect"
                                                WidthSizePolicy="StretchToParent"
                                                HeightSizePolicy="StretchToParent"
                                                ClipContents="true">
                                          <Children>
                                            <ListPanel Id="MercenaryClanList"
                                                       DataSource="{Clans}"
                                                       WidthSizePolicy="StretchToParent"
                                                       HeightSizePolicy="CoverChildren"
                                                       StackLayout.LayoutMethod="VerticalTopToBottom">
                                              <ItemTemplate>
                                                <ButtonWidget WidthSizePolicy="Fixed"
                                                              HeightSizePolicy="Fixed"
                                                              SuggestedWidth="880"
                                                              SuggestedHeight="90"
                                                              MarginBottom="4"
                                                              Brush="Kingdom.Item.Tuple"
                                                              Command.Click="ExecuteSelect"
                                                              IsSelected="@IsSelected"
                                                              DoNotPassEventsToChildren="true">
                                                  <Children>
                                                    <ListPanel WidthSizePolicy="StretchToParent"
                                                               HeightSizePolicy="StretchToParent"
                                                               StackLayout.LayoutMethod="HorizontalLeftToRight"
                                                               DoNotAcceptEvents="true">
                                                      <Children>
                                                        <Widget WidthSizePolicy="Fixed" HeightSizePolicy="StretchToParent" SuggestedWidth="90">
                                                          <Children>
                                                            <MaskedTextureWidget DataSource="{Banner}"
                                                                                 WidthSizePolicy="Fixed"
                                                                                 HeightSizePolicy="Fixed"
                                                                                 SuggestedWidth="62"
                                                                                 SuggestedHeight="80"
                                                                                 HorizontalAlignment="Center"
                                                                                 VerticalAlignment="Center"
                                                                                 Brush="Flat.Tuple.Banner.Small"
                                                                                 AdditionalArgs="@AdditionalArgs"
                                                                                 ImageId="@Id"
                                                                                 TextureProviderName="@TextureProviderName"
                                                                                 IsDisabled="true" />
                                                          </Children>
                                                        </Widget>
                                                        <TextWidget WidthSizePolicy="Fixed" HeightSizePolicy="StretchToParent" SuggestedWidth="220" Brush="ArmyManagement.Army.Tuple.Name" Text="@ClanName" />
                                                        <TextWidget WidthSizePolicy="Fixed" HeightSizePolicy="StretchToParent" SuggestedWidth="250" Brush="ArmyManagement.Army.Tuple.Name" Text="@LeaderName" />
                                                        <TextWidget WidthSizePolicy="Fixed" HeightSizePolicy="StretchToParent" SuggestedWidth="130" HorizontalAlignment="Center" Brush="ArmyManagement.Army.Tuple.Name" IntText="@Strength" />
                                                        <TextWidget WidthSizePolicy="Fixed" HeightSizePolicy="StretchToParent" SuggestedWidth="190" HorizontalAlignment="Center" Brush="ArmyManagement.Army.Tuple.Name" IntText="@ActiveParties" />
                                                      </Children>
                                                    </ListPanel>
                                                  </Children>
                                                </ButtonWidget>
                                              </ItemTemplate>
                                            </ListPanel>
                                          </Children>
                                        </Widget>
                                      </Children>
                                    </ScrollablePanel>
                                    <Standard.VerticalScrollbar Id="MercenaryClanListScrollbar"
                                                                HeightSizePolicy="StretchToParent"
                                                                HorizontalAlignment="Right"
                                                                MarginLeft="4"
                                                                MarginRight="8"
                                                                MarginBottom="8" />
                                  </Children>
                                </ListPanel>
                              </Children>
                            </BrushListPanel>

                            <ListPanel WidthSizePolicy="StretchToParent"
                                       HeightSizePolicy="CoverChildren"
                                       MarginLeft="55"
                                       MarginRight="35"
                                       StackLayout.LayoutMethod="VerticalTopToBottom">
                              <Children>
                                <TextWidget DataSource="{SelectedClan}"
                                            WidthSizePolicy="StretchToParent"
                                            HeightSizePolicy="Fixed"
                                            SuggestedHeight="55"
                                            HorizontalAlignment="Center"
                                            Brush="Kingdom.TitleMedium.Text"
                                            Text="@ClanName" />

                                <MaskedTextureWidget DataSource="{SelectedClan\Banner}"
                                                     WidthSizePolicy="Fixed"
                                                     HeightSizePolicy="Fixed"
                                                     SuggestedWidth="90"
                                                     SuggestedHeight="105"
                                                     MarginTop="2"
                                                     MarginBottom="8"
                                                     HorizontalAlignment="Center"
                                                     Brush="Kingdom.TornBanner.Big"
                                                     AdditionalArgs="@AdditionalArgs"
                                                     ImageId="@Id"
                                                     TextureProviderName="@TextureProviderName" />

                                <ListPanel WidthSizePolicy="StretchToParent" HeightSizePolicy="Fixed" SuggestedHeight="44">
                                  <Children>
                                    <TextWidget WidthSizePolicy="StretchToParent" HeightSizePolicy="StretchToParent" Brush="Kingdom.ParagraphSmall.Text" Brush.FontSize="25" Text="@LeaderText" />
                                    <TextWidget DataSource="{SelectedClan}" WidthSizePolicy="StretchToParent" HeightSizePolicy="StretchToParent" HorizontalAlignment="Right" Brush="ArmyManagement.Army.Tuple.Name" Text="@LeaderName" />
                                  </Children>
                                </ListPanel>
                                <Widget WidthSizePolicy="StretchToParent" HeightSizePolicy="Fixed" SuggestedHeight="2" Sprite="GradientDivider_9" AlphaFactor="0.4" />

                                <ListPanel WidthSizePolicy="StretchToParent" HeightSizePolicy="Fixed" SuggestedHeight="44">
                                  <Children>
                                    <TextWidget WidthSizePolicy="StretchToParent" HeightSizePolicy="StretchToParent" Brush="Kingdom.ParagraphSmall.Text" Brush.FontSize="25" Text="@ClanLevelText" />
                                    <TextWidget DataSource="{SelectedClan}" WidthSizePolicy="StretchToParent" HeightSizePolicy="StretchToParent" HorizontalAlignment="Right" Brush="ArmyManagement.Army.Tuple.Name" IntText="@ClanLevel" />
                                  </Children>
                                </ListPanel>
                                <Widget WidthSizePolicy="StretchToParent" HeightSizePolicy="Fixed" SuggestedHeight="2" Sprite="GradientDivider_9" AlphaFactor="0.4" />

                                <ListPanel WidthSizePolicy="StretchToParent" HeightSizePolicy="Fixed" SuggestedHeight="44">
                                  <Children>
                                    <TextWidget WidthSizePolicy="StretchToParent" HeightSizePolicy="StretchToParent" Brush="Kingdom.ParagraphSmall.Text" Brush.FontSize="25" Text="@StrengthText" />
                                    <TextWidget DataSource="{SelectedClan}" WidthSizePolicy="StretchToParent" HeightSizePolicy="StretchToParent" HorizontalAlignment="Right" Brush="ArmyManagement.Army.Tuple.Name" IntText="@Strength" />
                                  </Children>
                                </ListPanel>
                                <Widget WidthSizePolicy="StretchToParent" HeightSizePolicy="Fixed" SuggestedHeight="2" Sprite="GradientDivider_9" AlphaFactor="0.4" />

                                <ListPanel WidthSizePolicy="StretchToParent" HeightSizePolicy="Fixed" SuggestedHeight="44">
                                  <Children>
                                    <TextWidget WidthSizePolicy="StretchToParent" HeightSizePolicy="StretchToParent" Brush="Kingdom.ParagraphSmall.Text" Brush.FontSize="25" Text="@ActivePartiesText" />
                                    <TextWidget DataSource="{SelectedClan}" WidthSizePolicy="StretchToParent" HeightSizePolicy="StretchToParent" HorizontalAlignment="Right" Brush="ArmyManagement.Army.Tuple.Name" IntText="@ActiveParties" />
                                  </Children>
                                </ListPanel>
                                <Widget WidthSizePolicy="StretchToParent" HeightSizePolicy="Fixed" SuggestedHeight="2" Sprite="GradientDivider_9" AlphaFactor="0.4" />

                                <ListPanel WidthSizePolicy="StretchToParent" HeightSizePolicy="Fixed" SuggestedHeight="44">
                                  <Children>
                                    <TextWidget WidthSizePolicy="StretchToParent" HeightSizePolicy="StretchToParent" Brush="Kingdom.ParagraphSmall.Text" Brush.FontSize="25" Text="@ClanMembersText" />
                                    <TextWidget DataSource="{SelectedClan}" WidthSizePolicy="StretchToParent" HeightSizePolicy="StretchToParent" HorizontalAlignment="Right" Brush="ArmyManagement.Army.Tuple.Name" IntText="@ClanMembers" />
                                  </Children>
                                </ListPanel>
                                <Widget WidthSizePolicy="StretchToParent" HeightSizePolicy="Fixed" SuggestedHeight="2" Sprite="GradientDivider_9" AlphaFactor="0.4" />

                                <ListPanel WidthSizePolicy="StretchToParent" HeightSizePolicy="Fixed" SuggestedHeight="44">
                                  <Children>
                                    <TextWidget WidthSizePolicy="StretchToParent" HeightSizePolicy="StretchToParent" Brush="Kingdom.ParagraphSmall.Text" Brush.FontSize="25" Text="@RelationToRulerText" />
                                    <TextWidget DataSource="{SelectedClan}" WidthSizePolicy="StretchToParent" HeightSizePolicy="StretchToParent" HorizontalAlignment="Right" Brush="ArmyManagement.Army.Tuple.Name" IntText="@RelationToRuler" />
                                  </Children>
                                </ListPanel>
                                <Widget WidthSizePolicy="StretchToParent" HeightSizePolicy="Fixed" SuggestedHeight="2" Sprite="GradientDivider_9" AlphaFactor="0.4" />

                                <ListPanel WidthSizePolicy="StretchToParent" HeightSizePolicy="Fixed" SuggestedHeight="44">
                                  <Children>
                                    <TextWidget WidthSizePolicy="StretchToParent" HeightSizePolicy="StretchToParent" Brush="Kingdom.ParagraphSmall.Text" Brush.FontSize="25" Text="@InfluencePriceText" />
                                    <TextWidget DataSource="{SelectedClan}" WidthSizePolicy="StretchToParent" HeightSizePolicy="StretchToParent" HorizontalAlignment="Right" Brush="ArmyManagement.Army.Tuple.Name" IntText="@InfluencePrice" />
                                  </Children>
                                </ListPanel>
                                <Widget WidthSizePolicy="StretchToParent" HeightSizePolicy="Fixed" SuggestedHeight="2" Sprite="GradientDivider_9" AlphaFactor="0.4" />

                                <ListPanel WidthSizePolicy="StretchToParent" HeightSizePolicy="Fixed" SuggestedHeight="44">
                                  <Children>
                                    <TextWidget WidthSizePolicy="StretchToParent" HeightSizePolicy="StretchToParent" Brush="Kingdom.ParagraphSmall.Text" Brush.FontSize="25" Text="@AccumulatedInfluenceText" />
                                    <TextWidget DataSource="{SelectedClan}" WidthSizePolicy="StretchToParent" HeightSizePolicy="StretchToParent" HorizontalAlignment="Right" Brush="ArmyManagement.Army.Tuple.Name" Text="@AccumulatedInfluence" />
                                  </Children>
                                </ListPanel>
                                <Widget WidthSizePolicy="StretchToParent" HeightSizePolicy="Fixed" SuggestedHeight="2" Sprite="GradientDivider_9" AlphaFactor="0.4" />

                                <ListPanel WidthSizePolicy="StretchToParent" HeightSizePolicy="Fixed" SuggestedHeight="44">
                                  <Children>
                                    <TextWidget WidthSizePolicy="StretchToParent" HeightSizePolicy="StretchToParent" Brush="Kingdom.ParagraphSmall.Text" Brush.FontSize="25" Text="@ExpectedPaymentText" />
                                    <TextWidget DataSource="{SelectedClan}" WidthSizePolicy="StretchToParent" HeightSizePolicy="StretchToParent" HorizontalAlignment="Right" Brush="ArmyManagement.Army.Tuple.Name" IntText="@ExpectedPayment" />
                                  </Children>
                                </ListPanel>
                                <Widget WidthSizePolicy="StretchToParent" HeightSizePolicy="Fixed" SuggestedHeight="2" Sprite="GradientDivider_9" AlphaFactor="0.4" />

                                <ListPanel WidthSizePolicy="StretchToParent" HeightSizePolicy="Fixed" SuggestedHeight="44">
                                  <Children>
                                    <TextWidget WidthSizePolicy="StretchToParent" HeightSizePolicy="StretchToParent" Brush="Kingdom.ParagraphSmall.Text" Brush.FontSize="25" Text="@ServiceStatusText" />
                                    <TextWidget DataSource="{SelectedClan}" WidthSizePolicy="StretchToParent" HeightSizePolicy="StretchToParent" HorizontalAlignment="Right" Brush="ArmyManagement.Army.Tuple.Name" Text="@ServiceStatus" />
                                  </Children>
                                </ListPanel>
                              </Children>
                            </ListPanel>
                          </Children>
                        </ListPanel>

                        <TextWidget WidthSizePolicy="StretchToParent"
                                    HeightSizePolicy="StretchToParent"
                                    HorizontalAlignment="Center"
                                    VerticalAlignment="Center"
                                    MarginLeft="80"
                                    MarginRight="80"
                                    Brush="Kingdom.TitleMedium.Text"
                                    Brush.FontSize="32"
                                    Text="@EmptyStateText"
                                    IsVisible="@IsEmpty" />
                      </Children>
                    </ListPanel>
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
