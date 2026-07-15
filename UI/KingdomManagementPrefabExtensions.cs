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

                        <ListPanel WidthSizePolicy="Fixed"
                                   HeightSizePolicy="Fixed"
                                   SuggestedWidth="1200"
                                   SuggestedHeight="55"
                                   HorizontalAlignment="Center"
                                   StackLayout.LayoutMethod="HorizontalLeftToRight">
                          <Children>
                            <Widget WidthSizePolicy="Fixed" HeightSizePolicy="StretchToParent" SuggestedWidth="110" />
                            <TextWidget WidthSizePolicy="Fixed" HeightSizePolicy="StretchToParent" SuggestedWidth="300" Brush="Kingdom.ParagraphSmall.Text" Brush.FontSize="24" Text="@ClanText" />
                            <TextWidget WidthSizePolicy="Fixed" HeightSizePolicy="StretchToParent" SuggestedWidth="340" Brush="Kingdom.ParagraphSmall.Text" Brush.FontSize="24" Text="@LeaderText" />
                            <TextWidget WidthSizePolicy="Fixed" HeightSizePolicy="StretchToParent" SuggestedWidth="200" HorizontalAlignment="Center" Brush="Kingdom.ParagraphSmall.Text" Brush.FontSize="24" Text="@StrengthText" />
                            <TextWidget WidthSizePolicy="Fixed" HeightSizePolicy="StretchToParent" SuggestedWidth="250" HorizontalAlignment="Center" Brush="Kingdom.ParagraphSmall.Text" Brush.FontSize="24" Text="@ActivePartiesText" />
                          </Children>
                        </ListPanel>

                        <ListPanel WidthSizePolicy="Fixed"
                                   HeightSizePolicy="StretchToParent"
                                   SuggestedWidth="1240"
                                   HorizontalAlignment="Center"
                                   IsVisible="@HasClans">
                          <Children>
                            <ScrollablePanel WidthSizePolicy="StretchToParent"
                                             HeightSizePolicy="StretchToParent"
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
                                        <Widget WidthSizePolicy="Fixed"
                                                HeightSizePolicy="Fixed"
                                                SuggestedWidth="1200"
                                                SuggestedHeight="90"
                                                HorizontalAlignment="Center"
                                                MarginBottom="4">
                                          <Children>
                                            <BrushWidget WidthSizePolicy="StretchToParent"
                                                         HeightSizePolicy="StretchToParent"
                                                         Brush="Kingdom.Item.Tuple"
                                                         IsEnabled="false" />
                                            <ListPanel WidthSizePolicy="StretchToParent"
                                                       HeightSizePolicy="StretchToParent"
                                                       StackLayout.LayoutMethod="HorizontalLeftToRight"
                                                       DoNotAcceptEvents="true">
                                              <Children>
                                                <Widget WidthSizePolicy="Fixed" HeightSizePolicy="StretchToParent" SuggestedWidth="110">
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
                                                <TextWidget WidthSizePolicy="Fixed" HeightSizePolicy="StretchToParent" SuggestedWidth="300" Brush="ArmyManagement.Army.Tuple.Name" Text="@ClanName" />
                                                <TextWidget WidthSizePolicy="Fixed" HeightSizePolicy="StretchToParent" SuggestedWidth="340" Brush="ArmyManagement.Army.Tuple.Name" Text="@LeaderName" />
                                                <TextWidget WidthSizePolicy="Fixed" HeightSizePolicy="StretchToParent" SuggestedWidth="200" HorizontalAlignment="Center" Brush="ArmyManagement.Army.Tuple.Name" IntText="@Strength" />
                                                <TextWidget WidthSizePolicy="Fixed" HeightSizePolicy="StretchToParent" SuggestedWidth="250" HorizontalAlignment="Center" Brush="ArmyManagement.Army.Tuple.Name" IntText="@ActiveParties" />
                                              </Children>
                                            </ListPanel>
                                          </Children>
                                        </Widget>
                                      </ItemTemplate>
                                    </ListPanel>
                                  </Children>
                                </Widget>
                              </Children>
                            </ScrollablePanel>
                            <Standard.VerticalScrollbar Id="MercenaryClanListScrollbar"
                                                        HeightSizePolicy="StretchToParent"
                                                        HorizontalAlignment="Right"
                                                        MarginLeft="4" />
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
