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
                                   IsVisible="@HasAnyClans">
                          <Children>
                            <ListPanel WidthSizePolicy="Fixed"
                                       HeightSizePolicy="StretchToParent"
                                       SuggestedWidth="955">
                              <Children>
                                <ScrollablePanel WidthSizePolicy="StretchToParent"
                                                 HeightSizePolicy="StretchToParent"
                                                 AutoHideScrollBars="true"
                                                 MouseScrollAxis="Vertical"
                                                 ClipRect="MercenarySectionsClipRect"
                                                 InnerPanel="MercenarySectionsClipRect\MercenarySections"
                                                 VerticalScrollbar="..\MercenarySectionsScrollbar\Scrollbar">
                                  <Children>
                                    <Widget Id="MercenarySectionsClipRect"
                                            WidthSizePolicy="StretchToParent"
                                            HeightSizePolicy="StretchToParent"
                                            ClipContents="true">
                                      <Children>
                                        <ListPanel Id="MercenarySections"
                                                   WidthSizePolicy="Fixed"
                                                   HeightSizePolicy="CoverChildren"
                                                   SuggestedWidth="930"
                                                   StackLayout.LayoutMethod="VerticalTopToBottom">
                                          <Children>
                                <BrushListPanel WidthSizePolicy="Fixed"
                                                HeightSizePolicy="CoverChildren"
                                                SuggestedWidth="930"
                                                MinHeight="610"
                                                MarginBottom="12"
                                                Brush="Frame1Brush"
                                                StackLayout.LayoutMethod="VerticalTopToBottom">
                                  <Children>
                                    <TextWidget WidthSizePolicy="StretchToParent" HeightSizePolicy="Fixed" SuggestedHeight="42" HorizontalAlignment="Center" Brush="Kingdom.TitleMedium.Text" Brush.FontSize="25" Text="@HiredClansTitleText" />
                                    <ListPanel WidthSizePolicy="Fixed" HeightSizePolicy="Fixed" SuggestedWidth="900" SuggestedHeight="38" MarginLeft="10" StackLayout.LayoutMethod="HorizontalLeftToRight">
                                      <Children>
                                        <Widget WidthSizePolicy="Fixed" HeightSizePolicy="StretchToParent" SuggestedWidth="65" />
                                        <TextWidget WidthSizePolicy="Fixed" HeightSizePolicy="StretchToParent" SuggestedWidth="155" Brush="Kingdom.ParagraphSmall.Text" Brush.FontSize="18" Text="@ClanText" />
                                        <TextWidget WidthSizePolicy="Fixed" HeightSizePolicy="StretchToParent" SuggestedWidth="175" Brush="Kingdom.ParagraphSmall.Text" Brush.FontSize="18" Text="@LeaderText" />
                                        <TextWidget WidthSizePolicy="Fixed" HeightSizePolicy="StretchToParent" SuggestedWidth="85" HorizontalAlignment="Center" Brush="Kingdom.ParagraphSmall.Text" Brush.FontSize="18" Text="@ClanLevelColumnText" />
                                        <TextWidget WidthSizePolicy="Fixed" HeightSizePolicy="StretchToParent" SuggestedWidth="105" HorizontalAlignment="Center" Brush="Kingdom.ParagraphSmall.Text" Brush.FontSize="18" Text="@StrengthText" />
                                        <TextWidget WidthSizePolicy="Fixed" HeightSizePolicy="StretchToParent" SuggestedWidth="150" HorizontalAlignment="Center" Brush="Kingdom.ParagraphSmall.Text" Brush.FontSize="18" Text="@ActivePartiesColumnText" />
                                        <TextWidget WidthSizePolicy="Fixed" HeightSizePolicy="StretchToParent" SuggestedWidth="165" HorizontalAlignment="Center" Brush="Kingdom.ParagraphSmall.Text" Brush.FontSize="18" Text="@RelationColumnText" />
                                      </Children>
                                    </ListPanel>
                                    <ListPanel Id="HiredClanList" DataSource="{Clans}" WidthSizePolicy="StretchToParent" HeightSizePolicy="CoverChildren" MarginLeft="10" MarginBottom="8" StackLayout.LayoutMethod="VerticalTopToBottom" IsVisible="@HasClans">
                                      <ItemTemplate>
                                                    <ButtonWidget WidthSizePolicy="Fixed" HeightSizePolicy="Fixed" SuggestedWidth="900" SuggestedHeight="66" MarginBottom="3" Brush="Kingdom.Item.Tuple" Command.Click="ExecuteSelect" IsSelected="@IsSelected" DoNotPassEventsToChildren="true">
                                                      <Children>
                                                        <ListPanel WidthSizePolicy="StretchToParent" HeightSizePolicy="StretchToParent" StackLayout.LayoutMethod="HorizontalLeftToRight" DoNotAcceptEvents="true">
                                                          <Children>
                                                            <Widget WidthSizePolicy="Fixed" HeightSizePolicy="StretchToParent" SuggestedWidth="65">
                                                              <Children>
                                                                <MaskedTextureWidget DataSource="{Banner}" WidthSizePolicy="Fixed" HeightSizePolicy="Fixed" SuggestedWidth="46" SuggestedHeight="58" HorizontalAlignment="Center" VerticalAlignment="Center" Brush="Flat.Tuple.Banner.Small" AdditionalArgs="@AdditionalArgs" ImageId="@Id" TextureProviderName="@TextureProviderName" IsDisabled="true" />
                                                              </Children>
                                                            </Widget>
                                                            <TextWidget WidthSizePolicy="Fixed" HeightSizePolicy="StretchToParent" SuggestedWidth="155" Brush="ArmyManagement.Army.Tuple.Name" Brush.FontSize="20" Text="@ClanName" />
                                                            <TextWidget WidthSizePolicy="Fixed" HeightSizePolicy="StretchToParent" SuggestedWidth="175" Brush="ArmyManagement.Army.Tuple.Name" Brush.FontSize="20" Text="@LeaderName" />
                                                            <TextWidget WidthSizePolicy="Fixed" HeightSizePolicy="StretchToParent" SuggestedWidth="85" HorizontalAlignment="Center" Brush="ArmyManagement.Army.Tuple.Name" Brush.FontSize="20" IntText="@ClanLevel" />
                                                            <TextWidget WidthSizePolicy="Fixed" HeightSizePolicy="StretchToParent" SuggestedWidth="105" HorizontalAlignment="Center" Brush="ArmyManagement.Army.Tuple.Name" Brush.FontSize="20" IntText="@Strength" />
                                                            <TextWidget WidthSizePolicy="Fixed" HeightSizePolicy="StretchToParent" SuggestedWidth="150" HorizontalAlignment="Center" Brush="ArmyManagement.Army.Tuple.Name" Brush.FontSize="20" IntText="@ActiveParties" />
                                                            <TextWidget WidthSizePolicy="Fixed" HeightSizePolicy="StretchToParent" SuggestedWidth="165" HorizontalAlignment="Center" Brush="ArmyManagement.Army.Tuple.Name" Brush.FontSize="20" IntText="@RelationToRuler" />
                                                          </Children>
                                                        </ListPanel>
                                                      </Children>
                                                    </ButtonWidget>
                                      </ItemTemplate>
                                    </ListPanel>
                                    <TextWidget WidthSizePolicy="StretchToParent" HeightSizePolicy="Fixed" SuggestedHeight="530" HorizontalAlignment="Center" VerticalAlignment="Center" Brush="Kingdom.ParagraphSmall.Text" Brush.FontSize="22" Text="@NoHiredClansText" IsVisible="@AreHiredClansEmpty" />
                                  </Children>
                                </BrushListPanel>

                                <BrushListPanel WidthSizePolicy="Fixed"
                                                HeightSizePolicy="CoverChildren"
                                                SuggestedWidth="930"
                                                MinHeight="610"
                                                MarginBottom="12"
                                                Brush="Frame1Brush"
                                                StackLayout.LayoutMethod="VerticalTopToBottom">
                                  <Children>
                                    <TextWidget WidthSizePolicy="StretchToParent" HeightSizePolicy="Fixed" SuggestedHeight="42" HorizontalAlignment="Center" Brush="Kingdom.TitleMedium.Text" Brush.FontSize="25" Text="@AvailableClansTitleText" />
                                    <ListPanel WidthSizePolicy="Fixed" HeightSizePolicy="Fixed" SuggestedWidth="900" SuggestedHeight="38" MarginLeft="10" StackLayout.LayoutMethod="HorizontalLeftToRight">
                                      <Children>
                                        <Widget WidthSizePolicy="Fixed" HeightSizePolicy="StretchToParent" SuggestedWidth="65" />
                                        <TextWidget WidthSizePolicy="Fixed" HeightSizePolicy="StretchToParent" SuggestedWidth="155" Brush="Kingdom.ParagraphSmall.Text" Brush.FontSize="18" Text="@ClanText" />
                                        <TextWidget WidthSizePolicy="Fixed" HeightSizePolicy="StretchToParent" SuggestedWidth="175" Brush="Kingdom.ParagraphSmall.Text" Brush.FontSize="18" Text="@LeaderText" />
                                        <TextWidget WidthSizePolicy="Fixed" HeightSizePolicy="StretchToParent" SuggestedWidth="85" HorizontalAlignment="Center" Brush="Kingdom.ParagraphSmall.Text" Brush.FontSize="18" Text="@ClanLevelColumnText" />
                                        <TextWidget WidthSizePolicy="Fixed" HeightSizePolicy="StretchToParent" SuggestedWidth="105" HorizontalAlignment="Center" Brush="Kingdom.ParagraphSmall.Text" Brush.FontSize="18" Text="@StrengthText" />
                                        <TextWidget WidthSizePolicy="Fixed" HeightSizePolicy="StretchToParent" SuggestedWidth="150" HorizontalAlignment="Center" Brush="Kingdom.ParagraphSmall.Text" Brush.FontSize="18" Text="@ActivePartiesColumnText" />
                                        <TextWidget WidthSizePolicy="Fixed" HeightSizePolicy="StretchToParent" SuggestedWidth="165" HorizontalAlignment="Center" Brush="Kingdom.ParagraphSmall.Text" Brush.FontSize="18" Text="@RelationColumnText" />
                                      </Children>
                                    </ListPanel>
                                    <ListPanel Id="AvailableClanList" DataSource="{AvailableClans}" WidthSizePolicy="StretchToParent" HeightSizePolicy="CoverChildren" MarginLeft="10" MarginBottom="8" StackLayout.LayoutMethod="VerticalTopToBottom" IsVisible="@HasAvailableClans">
                                      <ItemTemplate>
                                                    <ButtonWidget WidthSizePolicy="Fixed" HeightSizePolicy="Fixed" SuggestedWidth="900" SuggestedHeight="66" MarginBottom="3" Brush="Kingdom.Item.Tuple" Command.Click="ExecuteSelect" IsSelected="@IsSelected" DoNotPassEventsToChildren="true">
                                                      <Children>
                                                        <ListPanel WidthSizePolicy="StretchToParent" HeightSizePolicy="StretchToParent" StackLayout.LayoutMethod="HorizontalLeftToRight" DoNotAcceptEvents="true">
                                                          <Children>
                                                            <Widget WidthSizePolicy="Fixed" HeightSizePolicy="StretchToParent" SuggestedWidth="65">
                                                              <Children>
                                                                <MaskedTextureWidget DataSource="{Banner}" WidthSizePolicy="Fixed" HeightSizePolicy="Fixed" SuggestedWidth="46" SuggestedHeight="58" HorizontalAlignment="Center" VerticalAlignment="Center" Brush="Flat.Tuple.Banner.Small" AdditionalArgs="@AdditionalArgs" ImageId="@Id" TextureProviderName="@TextureProviderName" IsDisabled="true" />
                                                              </Children>
                                                            </Widget>
                                                            <TextWidget WidthSizePolicy="Fixed" HeightSizePolicy="StretchToParent" SuggestedWidth="155" Brush="ArmyManagement.Army.Tuple.Name" Brush.FontSize="20" Text="@ClanName" />
                                                            <TextWidget WidthSizePolicy="Fixed" HeightSizePolicy="StretchToParent" SuggestedWidth="175" Brush="ArmyManagement.Army.Tuple.Name" Brush.FontSize="20" Text="@LeaderName" />
                                                            <TextWidget WidthSizePolicy="Fixed" HeightSizePolicy="StretchToParent" SuggestedWidth="85" HorizontalAlignment="Center" Brush="ArmyManagement.Army.Tuple.Name" Brush.FontSize="20" IntText="@ClanLevel" />
                                                            <TextWidget WidthSizePolicy="Fixed" HeightSizePolicy="StretchToParent" SuggestedWidth="105" HorizontalAlignment="Center" Brush="ArmyManagement.Army.Tuple.Name" Brush.FontSize="20" IntText="@Strength" />
                                                            <TextWidget WidthSizePolicy="Fixed" HeightSizePolicy="StretchToParent" SuggestedWidth="150" HorizontalAlignment="Center" Brush="ArmyManagement.Army.Tuple.Name" Brush.FontSize="20" IntText="@ActiveParties" />
                                                            <TextWidget WidthSizePolicy="Fixed" HeightSizePolicy="StretchToParent" SuggestedWidth="165" HorizontalAlignment="Center" Brush="ArmyManagement.Army.Tuple.Name" Brush.FontSize="20" IntText="@RelationToRuler" />
                                                          </Children>
                                                        </ListPanel>
                                                      </Children>
                                                    </ButtonWidget>
                                      </ItemTemplate>
                                    </ListPanel>
                                    <TextWidget WidthSizePolicy="StretchToParent" HeightSizePolicy="Fixed" SuggestedHeight="530" HorizontalAlignment="Center" VerticalAlignment="Center" Brush="Kingdom.ParagraphSmall.Text" Brush.FontSize="22" Text="@NoAvailableClansText" IsVisible="@AreAvailableClansEmpty" />
                                  </Children>
                                </BrushListPanel>

                                <BrushListPanel WidthSizePolicy="Fixed"
                                                HeightSizePolicy="CoverChildren"
                                                SuggestedWidth="930"
                                                MinHeight="610"
                                                Brush="Frame1Brush"
                                                StackLayout.LayoutMethod="VerticalTopToBottom">
                                  <Children>
                                    <TextWidget WidthSizePolicy="StretchToParent" HeightSizePolicy="Fixed" SuggestedHeight="42" HorizontalAlignment="Center" Brush="Kingdom.TitleMedium.Text" Brush.FontSize="25" Text="@OtherKingdomClansTitleText" />
                                    <ListPanel WidthSizePolicy="Fixed" HeightSizePolicy="Fixed" SuggestedWidth="900" SuggestedHeight="38" MarginLeft="10" StackLayout.LayoutMethod="HorizontalLeftToRight">
                                      <Children>
                                        <Widget WidthSizePolicy="Fixed" HeightSizePolicy="StretchToParent" SuggestedWidth="65" />
                                        <TextWidget WidthSizePolicy="Fixed" HeightSizePolicy="StretchToParent" SuggestedWidth="155" Brush="Kingdom.ParagraphSmall.Text" Brush.FontSize="18" Text="@ClanText" />
                                        <TextWidget WidthSizePolicy="Fixed" HeightSizePolicy="StretchToParent" SuggestedWidth="175" Brush="Kingdom.ParagraphSmall.Text" Brush.FontSize="18" Text="@LeaderText" />
                                        <TextWidget WidthSizePolicy="Fixed" HeightSizePolicy="StretchToParent" SuggestedWidth="85" HorizontalAlignment="Center" Brush="Kingdom.ParagraphSmall.Text" Brush.FontSize="18" Text="@ClanLevelColumnText" />
                                        <TextWidget WidthSizePolicy="Fixed" HeightSizePolicy="StretchToParent" SuggestedWidth="105" HorizontalAlignment="Center" Brush="Kingdom.ParagraphSmall.Text" Brush.FontSize="18" Text="@StrengthText" />
                                        <TextWidget WidthSizePolicy="Fixed" HeightSizePolicy="StretchToParent" SuggestedWidth="150" HorizontalAlignment="Center" Brush="Kingdom.ParagraphSmall.Text" Brush.FontSize="18" Text="@ActivePartiesColumnText" />
                                        <TextWidget WidthSizePolicy="Fixed" HeightSizePolicy="StretchToParent" SuggestedWidth="165" HorizontalAlignment="Center" Brush="Kingdom.ParagraphSmall.Text" Brush.FontSize="18" Text="@RelationColumnText" />
                                      </Children>
                                    </ListPanel>
                                    <ListPanel Id="OtherKingdomClanList" DataSource="{OtherKingdomClans}" WidthSizePolicy="StretchToParent" HeightSizePolicy="CoverChildren" MarginLeft="10" MarginBottom="8" StackLayout.LayoutMethod="VerticalTopToBottom" IsVisible="@HasOtherKingdomClans">
                                      <ItemTemplate>
                                                    <ButtonWidget WidthSizePolicy="Fixed" HeightSizePolicy="Fixed" SuggestedWidth="900" SuggestedHeight="66" MarginBottom="3" Brush="Kingdom.Item.Tuple" Command.Click="ExecuteSelect" IsSelected="@IsSelected" DoNotPassEventsToChildren="true">
                                                      <Children>
                                                        <ListPanel WidthSizePolicy="StretchToParent" HeightSizePolicy="StretchToParent" StackLayout.LayoutMethod="HorizontalLeftToRight" DoNotAcceptEvents="true">
                                                          <Children>
                                                            <Widget WidthSizePolicy="Fixed" HeightSizePolicy="StretchToParent" SuggestedWidth="65">
                                                              <Children>
                                                                <MaskedTextureWidget DataSource="{Banner}" WidthSizePolicy="Fixed" HeightSizePolicy="Fixed" SuggestedWidth="46" SuggestedHeight="58" HorizontalAlignment="Center" VerticalAlignment="Center" Brush="Flat.Tuple.Banner.Small" AdditionalArgs="@AdditionalArgs" ImageId="@Id" TextureProviderName="@TextureProviderName" IsDisabled="true" />
                                                              </Children>
                                                            </Widget>
                                                            <TextWidget WidthSizePolicy="Fixed" HeightSizePolicy="StretchToParent" SuggestedWidth="155" Brush="ArmyManagement.Army.Tuple.Name" Brush.FontSize="20" Text="@ClanName" />
                                                            <TextWidget WidthSizePolicy="Fixed" HeightSizePolicy="StretchToParent" SuggestedWidth="175" Brush="ArmyManagement.Army.Tuple.Name" Brush.FontSize="20" Text="@LeaderName" />
                                                            <TextWidget WidthSizePolicy="Fixed" HeightSizePolicy="StretchToParent" SuggestedWidth="85" HorizontalAlignment="Center" Brush="ArmyManagement.Army.Tuple.Name" Brush.FontSize="20" IntText="@ClanLevel" />
                                                            <TextWidget WidthSizePolicy="Fixed" HeightSizePolicy="StretchToParent" SuggestedWidth="105" HorizontalAlignment="Center" Brush="ArmyManagement.Army.Tuple.Name" Brush.FontSize="20" IntText="@Strength" />
                                                            <TextWidget WidthSizePolicy="Fixed" HeightSizePolicy="StretchToParent" SuggestedWidth="150" HorizontalAlignment="Center" Brush="ArmyManagement.Army.Tuple.Name" Brush.FontSize="20" IntText="@ActiveParties" />
                                                            <TextWidget WidthSizePolicy="Fixed" HeightSizePolicy="StretchToParent" SuggestedWidth="165" HorizontalAlignment="Center" Brush="ArmyManagement.Army.Tuple.Name" Brush.FontSize="20" IntText="@RelationToRuler" />
                                                          </Children>
                                                        </ListPanel>
                                                      </Children>
                                                    </ButtonWidget>
                                      </ItemTemplate>
                                    </ListPanel>
                                    <TextWidget WidthSizePolicy="StretchToParent" HeightSizePolicy="Fixed" SuggestedHeight="530" HorizontalAlignment="Center" VerticalAlignment="Center" Brush="Kingdom.ParagraphSmall.Text" Brush.FontSize="22" Text="@NoOtherKingdomClansText" IsVisible="@AreOtherKingdomClansEmpty" />
                                  </Children>
                                </BrushListPanel>
                                          </Children>
                                        </ListPanel>
                                      </Children>
                                    </Widget>
                                  </Children>
                                </ScrollablePanel>
                                <Standard.VerticalScrollbar Id="MercenarySectionsScrollbar"
                                                            HeightSizePolicy="StretchToParent"
                                                            HorizontalAlignment="Right"
                                                            MarginLeft="4"
                                                            MarginRight="4" />
                              </Children>
                            </ListPanel>

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

                                <ListPanel WidthSizePolicy="StretchToParent" HeightSizePolicy="Fixed" SuggestedHeight="44" IsVisible="@IsSelectedClanServingPlayerKingdom">
                                  <Children>
                                    <TextWidget WidthSizePolicy="StretchToParent" HeightSizePolicy="StretchToParent" Brush="Kingdom.ParagraphSmall.Text" Brush.FontSize="25" Text="@InfluencePriceText" />
                                    <TextWidget DataSource="{SelectedClan}" WidthSizePolicy="StretchToParent" HeightSizePolicy="StretchToParent" HorizontalAlignment="Right" Brush="ArmyManagement.Army.Tuple.Name" IntText="@InfluencePrice" />
                                  </Children>
                                </ListPanel>
                                <Widget WidthSizePolicy="StretchToParent" HeightSizePolicy="Fixed" SuggestedHeight="2" Sprite="GradientDivider_9" AlphaFactor="0.4" IsVisible="@IsSelectedClanServingPlayerKingdom" />

                                <ListPanel WidthSizePolicy="StretchToParent" HeightSizePolicy="Fixed" SuggestedHeight="44" IsVisible="@IsSelectedClanServingPlayerKingdom">
                                  <Children>
                                    <TextWidget WidthSizePolicy="StretchToParent" HeightSizePolicy="StretchToParent" Brush="Kingdom.ParagraphSmall.Text" Brush.FontSize="25" Text="@AccumulatedInfluenceText" />
                                    <TextWidget DataSource="{SelectedClan}" WidthSizePolicy="StretchToParent" HeightSizePolicy="StretchToParent" HorizontalAlignment="Right" Brush="ArmyManagement.Army.Tuple.Name" Text="@AccumulatedInfluence" />
                                  </Children>
                                </ListPanel>
                                <Widget WidthSizePolicy="StretchToParent" HeightSizePolicy="Fixed" SuggestedHeight="2" Sprite="GradientDivider_9" AlphaFactor="0.4" IsVisible="@IsSelectedClanServingPlayerKingdom" />

                                <ListPanel WidthSizePolicy="StretchToParent" HeightSizePolicy="Fixed" SuggestedHeight="44" IsVisible="@IsSelectedClanServingPlayerKingdom">
                                  <Children>
                                    <TextWidget WidthSizePolicy="StretchToParent" HeightSizePolicy="StretchToParent" Brush="Kingdom.ParagraphSmall.Text" Brush.FontSize="25" Text="@ExpectedPaymentText" />
                                    <TextWidget DataSource="{SelectedClan}" WidthSizePolicy="StretchToParent" HeightSizePolicy="StretchToParent" HorizontalAlignment="Right" Brush="ArmyManagement.Army.Tuple.Name" IntText="@ExpectedPayment" />
                                  </Children>
                                </ListPanel>
                                <Widget WidthSizePolicy="StretchToParent" HeightSizePolicy="Fixed" SuggestedHeight="2" Sprite="GradientDivider_9" AlphaFactor="0.4" IsVisible="@IsSelectedClanServingPlayerKingdom" />

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
