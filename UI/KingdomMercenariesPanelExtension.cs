using System.Xml;
using Bannerlord.UIExtenderEx.Attributes;
using Bannerlord.UIExtenderEx.Prefabs2;

namespace MercenaryManagement.UI;

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
                                       HeightSizePolicy="StretchToParent"
                                       MarginLeft="55"
                                       MarginRight="35">
                              <Children>
                                <ScrollablePanel WidthSizePolicy="StretchToParent"
                                                 HeightSizePolicy="StretchToParent"
                                                 AutoHideScrollBars="true"
                                                 MouseScrollAxis="Vertical"
                                                 ClipRect="MercenaryDetailsClipRect"
                                                 InnerPanel="MercenaryDetailsClipRect\MercenaryDetails"
                                                 VerticalScrollbar="..\MercenaryDetailsScrollbar\Scrollbar">
                                  <Children>
                                    <Widget Id="MercenaryDetailsClipRect"
                                            WidthSizePolicy="StretchToParent"
                                            HeightSizePolicy="StretchToParent"
                                            ClipContents="true">
                                      <Children>
                                        <ListPanel Id="MercenaryDetails"
                                                   WidthSizePolicy="StretchToParent"
                                                   HeightSizePolicy="CoverChildren"
                                                   MarginRight="22"
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

                                <BrushListPanel WidthSizePolicy="StretchToParent"
                                                HeightSizePolicy="CoverChildren"
                                                MarginTop="8"
                                                MarginBottom="18"
                                                Brush="Frame1Brush"
                                                StackLayout.LayoutMethod="VerticalTopToBottom">
                                  <Children>
                                    <TextWidget WidthSizePolicy="StretchToParent"
                                                HeightSizePolicy="Fixed"
                                                SuggestedHeight="44"
                                                HorizontalAlignment="Center"
                                                Brush="Kingdom.TitleMedium.Text"
                                                Brush.FontSize="24"
                                                Text="@ClanInformationText" />
                                    <Widget WidthSizePolicy="StretchToParent"
                                            HeightSizePolicy="Fixed"
                                            SuggestedHeight="2"
                                            MarginLeft="12"
                                            MarginRight="12"
                                            Sprite="GradientDivider_9"
                                            AlphaFactor="0.4" />
                                    <ListPanel WidthSizePolicy="StretchToParent"
                                               HeightSizePolicy="CoverChildren"
                                               MarginLeft="18"
                                               MarginRight="18"
                                               MarginBottom="8"
                                               StackLayout.LayoutMethod="VerticalTopToBottom">
                                      <Children>
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

                                <ListPanel WidthSizePolicy="StretchToParent" HeightSizePolicy="Fixed" SuggestedHeight="44" IsVisible="@IsSelectedClanInfluencePriceVisible">
                                  <Children>
                                    <TextWidget WidthSizePolicy="StretchToParent" HeightSizePolicy="StretchToParent" Brush="Kingdom.ParagraphSmall.Text" Brush.FontSize="25" Text="@InfluencePriceText" />
                                    <TextWidget DataSource="{SelectedClan}" WidthSizePolicy="StretchToParent" HeightSizePolicy="StretchToParent" HorizontalAlignment="Right" Brush="ArmyManagement.Army.Tuple.Name" IntText="@InfluencePrice" />
                                  </Children>
                                </ListPanel>
                                <Widget WidthSizePolicy="StretchToParent" HeightSizePolicy="Fixed" SuggestedHeight="2" Sprite="GradientDivider_9" AlphaFactor="0.4" IsVisible="@IsSelectedClanInfluencePriceVisible" />

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

                                <ListPanel WidthSizePolicy="StretchToParent" HeightSizePolicy="Fixed" SuggestedHeight="44" IsVisible="@IsSelectedClanServingPlayerKingdom">
                                  <Children>
                                    <TextWidget WidthSizePolicy="StretchToParent" HeightSizePolicy="StretchToParent" Brush="Kingdom.ParagraphSmall.Text" Brush.FontSize="25" Text="@ContractStateText" />
                                    <TextWidget DataSource="{SelectedClan}" WidthSizePolicy="StretchToParent" HeightSizePolicy="StretchToParent" HorizontalAlignment="Right" Brush="ArmyManagement.Army.Tuple.Name" Text="@ContractState" />
                                  </Children>
                                </ListPanel>
                                <Widget WidthSizePolicy="StretchToParent" HeightSizePolicy="Fixed" SuggestedHeight="2" Sprite="GradientDivider_9" AlphaFactor="0.4" IsVisible="@IsSelectedClanServingPlayerKingdom" />

                                <ListPanel WidthSizePolicy="StretchToParent" HeightSizePolicy="Fixed" SuggestedHeight="44" IsVisible="@IsSelectedClanServingPlayerKingdom">
                                  <Children>
                                    <TextWidget WidthSizePolicy="StretchToParent" HeightSizePolicy="StretchToParent" Brush="Kingdom.ParagraphSmall.Text" Brush.FontSize="25" Text="@ContractStartText" />
                                    <TextWidget DataSource="{SelectedClan}" WidthSizePolicy="StretchToParent" HeightSizePolicy="StretchToParent" HorizontalAlignment="Right" Brush="ArmyManagement.Army.Tuple.Name" Text="@ContractStart" />
                                  </Children>
                                </ListPanel>
                                <Widget WidthSizePolicy="StretchToParent" HeightSizePolicy="Fixed" SuggestedHeight="2" Sprite="GradientDivider_9" AlphaFactor="0.4" IsVisible="@IsSelectedClanServingPlayerKingdom" />

                                <ListPanel WidthSizePolicy="StretchToParent" HeightSizePolicy="Fixed" SuggestedHeight="44" IsVisible="@IsSelectedClanServingPlayerKingdom">
                                  <Children>
                                    <TextWidget WidthSizePolicy="StretchToParent" HeightSizePolicy="StretchToParent" Brush="Kingdom.ParagraphSmall.Text" Brush.FontSize="25" Text="@ContractEndText" />
                                    <TextWidget DataSource="{SelectedClan}" WidthSizePolicy="StretchToParent" HeightSizePolicy="StretchToParent" HorizontalAlignment="Right" Brush="ArmyManagement.Army.Tuple.Name" Text="@ContractEnd" />
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
                                </BrushListPanel>

                                <BrushListPanel WidthSizePolicy="StretchToParent"
                                                HeightSizePolicy="CoverChildren"
                                                MinHeight="190"
                                                MarginBottom="18"
                                                Brush="Frame1Brush"
                                                StackLayout.LayoutMethod="VerticalTopToBottom"
                                                IsVisible="@IsSelectedClanSettingsVisible">
                                  <Children>
                                    <TextWidget WidthSizePolicy="StretchToParent"
                                                HeightSizePolicy="Fixed"
                                                SuggestedHeight="44"
                                                HorizontalAlignment="Center"
                                                Brush="Kingdom.TitleMedium.Text"
                                                Brush.FontSize="24"
                                                Text="@ContractSettingsText" />
                                    <Widget WidthSizePolicy="StretchToParent"
                                            HeightSizePolicy="Fixed"
                                            SuggestedHeight="2"
                                            MarginLeft="12"
                                            MarginRight="12"
                                            Sprite="GradientDivider_9"
                                            AlphaFactor="0.4" />
                                    <ListPanel WidthSizePolicy="StretchToParent"
                                               HeightSizePolicy="Fixed"
                                               SuggestedHeight="58"
                                               MarginLeft="18"
                                               MarginRight="18">
                                      <Children>
                                        <TextWidget WidthSizePolicy="StretchToParent"
                                                    HeightSizePolicy="StretchToParent"
                                                    Brush="Kingdom.ParagraphSmall.Text"
                                                    Brush.FontSize="23"
                                                    Text="@InfluencePriceText" />
                                        <TextWidget DataSource="{SelectedClan}"
                                                    WidthSizePolicy="Fixed"
                                                    HeightSizePolicy="Fixed"
                                                    SuggestedWidth="75"
                                                    SuggestedHeight="36"
                                                    MarginRight="8"
                                                    HorizontalAlignment="Right"
                                                    VerticalAlignment="Center"
                                                    Brush="CustomBattle.SliderValue.Text"
                                                    Brush.FontSize="22"
                                                    Text="@InfluencePricePercentageText" />
                                        <IntegerInputTextWidget DataSource="{SelectedClan}"
                                                                WidthSizePolicy="Fixed"
                                                                HeightSizePolicy="Fixed"
                                                                SuggestedWidth="130"
                                                                SuggestedHeight="36"
                                                                HorizontalAlignment="Right"
                                                                VerticalAlignment="Center"
                                                                Brush="CustomBattle.SliderValue.Text"
                                                                Brush.FontSize="22"
                                                                IntText="@InfluencePrice"
                                                                MinInt="@MinimumInfluencePrice"
                                                                MaxInt="@MaximumInfluencePrice"
                                                                MaxLength="10"
                                                                EnableClamp="true"
                                                                UpdateValueOnFocusLost="true"
                                                                IsDisabled="@IsInfluencePriceReadOnly"
                                                                UpdateChildrenStates="true"
                                                                DoNotPassEventsToChildren="true" />
                                      </Children>
                                    </ListPanel>
                                    <Widget DataSource="{SelectedClan}"
                                            WidthSizePolicy="StretchToParent"
                                            HeightSizePolicy="Fixed"
                                            SuggestedHeight="94"
                                            MarginLeft="24"
                                            MarginRight="24">
                                      <Children>
                                        <SliderWidget WidthSizePolicy="StretchToParent"
                                                      HeightSizePolicy="Fixed"
                                                      SuggestedHeight="42"
                                                      VerticalAlignment="Top"
                                                      Filler="Filler"
                                                      Handle="SliderHandle"
                                                      MinValueInt="@MinimumInfluencePrice"
                                                      MaxValueInt="@MaximumInfluencePrice"
                                                      ValueInt="@InfluencePrice"
                                                      IsDiscrete="true"
                                                      IsDisabled="@IsInfluencePriceReadOnly"
                                                      UpdateChildrenStates="true"
                                                      DoNotPassEventsToChildren="true"
                                                      DoNotUpdateHandleSize="true">
                                          <Children>
                                            <BrushWidget DoNotAcceptEvents="true"
                                                         WidthSizePolicy="StretchToParent"
                                                         HeightSizePolicy="Fixed"
                                                         SuggestedHeight="14"
                                                         VerticalAlignment="Center"
                                                         Sprite="General\Slider\slider_inner_canvas" />
                                            <BrushWidget Id="Filler"
                                                         DoNotAcceptEvents="true"
                                                         WidthSizePolicy="StretchToParent"
                                                         HeightSizePolicy="Fixed"
                                                         SuggestedHeight="21"
                                                         VerticalAlignment="Center"
                                                         PositionYOffset="2"
                                                         Sprite="General\Slider\slider_fill" />
                                            <BrushWidget DoNotAcceptEvents="true"
                                                         WidthSizePolicy="StretchToParent"
                                                         HeightSizePolicy="StretchToParent"
                                                         MarginTop="5"
                                                         MarginBottom="5"
                                                         Brush="Slider"
                                                         Brush.SaturationFactor="25"
                                                         Brush.ValueFactor="-15" />
                                            <ImageWidget Id="SliderHandle"
                                                         DoNotAcceptEvents="true"
                                                         WidthSizePolicy="Fixed"
                                                         HeightSizePolicy="Fixed"
                                                         SuggestedWidth="20"
                                                         SuggestedHeight="32"
                                                         HorizontalAlignment="Left"
                                                         VerticalAlignment="Center"
                                                         Brush="SliderHandle" />
                                          </Children>
                                        </SliderWidget>
                                        <SliderWidget WidthSizePolicy="StretchToParent"
                                                      HeightSizePolicy="Fixed"
                                                      SuggestedHeight="42"
                                                      VerticalAlignment="Top"
                                                      Handle="BaseInfluencePriceHandle"
                                                      MinValueInt="@MinimumInfluencePrice"
                                                      MaxValueInt="@MaximumInfluencePrice"
                                                      ValueInt="@BaseInfluencePrice"
                                                      IsDiscrete="true"
                                                      Locked="true"
                                                      DoNotAcceptEvents="true"
                                                      DoNotUpdateHandleSize="true">
                                          <Children>
                                            <Widget Id="BaseInfluencePriceHandle"
                                                    DoNotAcceptEvents="true"
                                                    WidthSizePolicy="Fixed"
                                                    HeightSizePolicy="Fixed"
                                                    SuggestedWidth="20"
                                                    SuggestedHeight="42"
                                                    HorizontalAlignment="Left"
                                                    VerticalAlignment="Center">
                                              <Children>
                                                <Widget WidthSizePolicy="Fixed"
                                                        HeightSizePolicy="Fixed"
                                                        SuggestedWidth="2"
                                                        SuggestedHeight="24"
                                                        HorizontalAlignment="Center"
                                                        VerticalAlignment="Center"
                                                        Sprite="BlankWhiteSquare_9"
                                                        AlphaFactor="0.75" />
                                                <TextWidget WidthSizePolicy="Fixed"
                                                            HeightSizePolicy="Fixed"
                                                            SuggestedWidth="120"
                                                            SuggestedHeight="22"
                                                            PositionXOffset="-50"
                                                            PositionYOffset="38"
                                                            Brush="Kingdom.ParagraphSmall.Text"
                                                            Brush.FontSize="16"
                                                            Text="@BaseInfluencePriceText" />
                                              </Children>
                                            </Widget>
                                          </Children>
                                        </SliderWidget>
                                        <Widget WidthSizePolicy="StretchToParent"
                                                HeightSizePolicy="Fixed"
                                                SuggestedHeight="24"
                                                MarginLeft="10"
                                                MarginRight="10"
                                                VerticalAlignment="Bottom">
                                          <Children>
                                            <TextWidget WidthSizePolicy="Fixed"
                                                        HeightSizePolicy="StretchToParent"
                                                        SuggestedWidth="60"
                                                        PositionXOffset="-30"
                                                        HorizontalAlignment="Left"
                                                        Brush="Kingdom.ParagraphSmall.Text"
                                                        Brush.FontSize="18"
                                                        IntText="@MinimumInfluencePrice" />
                                            <TextWidget WidthSizePolicy="Fixed"
                                                        HeightSizePolicy="StretchToParent"
                                                        SuggestedWidth="60"
                                                        PositionXOffset="30"
                                                        HorizontalAlignment="Right"
                                                        Brush="Kingdom.ParagraphSmall.Text"
                                                        Brush.FontSize="18"
                                                        IntText="@MaximumInfluencePrice" />
                                          </Children>
                                        </Widget>
                                      </Children>
                                    </Widget>
                                  </Children>
                                </BrushListPanel>

                                <ButtonWidget WidthSizePolicy="Fixed"
                                              HeightSizePolicy="Fixed"
                                              SuggestedWidth="227"
                                              SuggestedHeight="40"
                                              MarginTop="12"
                                              HorizontalAlignment="Center"
                                              Brush="ButtonBrush2"
                                              Command.Click="ExecuteHireSelectedClan"
                                              IsVisible="@CanHireSelectedClan"
                                              UpdateChildrenStates="true"
                                              DoNotPassEventsToChildren="true">
                                  <Children>
                                    <TextWidget WidthSizePolicy="StretchToParent"
                                                HeightSizePolicy="StretchToParent"
                                                Brush="Kingdom.GeneralButtons.Text"
                                                Text="@HireText" />
                                  </Children>
                                </ButtonWidget>

                                <ButtonWidget WidthSizePolicy="Fixed"
                                              HeightSizePolicy="Fixed"
                                              SuggestedWidth="227"
                                              SuggestedHeight="40"
                                              MarginTop="12"
                                              HorizontalAlignment="Center"
                                              Brush="ButtonBrush2"
                                              Command.Click="ExecutePoachSelectedClan"
                                              IsVisible="@CanPoachSelectedClan"
                                              UpdateChildrenStates="true"
                                              DoNotPassEventsToChildren="true">
                                  <Children>
                                    <TextWidget WidthSizePolicy="StretchToParent"
                                                HeightSizePolicy="StretchToParent"
                                                Brush="Kingdom.GeneralButtons.Text"
                                                Text="@PoachText" />
                                  </Children>
                                </ButtonWidget>

                                <ButtonWidget WidthSizePolicy="Fixed"
                                              HeightSizePolicy="Fixed"
                                              SuggestedWidth="227"
                                              SuggestedHeight="40"
                                              MarginTop="12"
                                              HorizontalAlignment="Center"
                                              Brush="ButtonBrush2"
                                              Command.Click="ExecuteDismissSelectedClan"
                                              IsVisible="@CanDismissSelectedClan"
                                              UpdateChildrenStates="true"
                                              DoNotPassEventsToChildren="true">
                                  <Children>
                                    <TextWidget WidthSizePolicy="StretchToParent"
                                                HeightSizePolicy="StretchToParent"
                                                Brush="Kingdom.GeneralButtons.Text"
                                                Text="@DismissText" />
                                  </Children>
                                </ButtonWidget>
                                          </Children>
                                        </ListPanel>
                                      </Children>
                                    </Widget>
                                  </Children>
                                </ScrollablePanel>
                                <Standard.VerticalScrollbar Id="MercenaryDetailsScrollbar"
                                                            HeightSizePolicy="StretchToParent"
                                                            HorizontalAlignment="Right"
                                                            MarginLeft="4"
                                                            MarginRight="4" />
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
