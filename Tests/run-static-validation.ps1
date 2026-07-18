$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot

function Assert-True([bool]$condition, [string]$message) {
    if (-not $condition) { throw "FAIL: $message" }
    Write-Host "PASS: $message"
}

foreach ($language in @('EN', 'RU')) {
    $path = Join-Path $root "_Module/ModuleData/Languages/$language/strings.xml"
    [xml]$document = Get-Content -Raw $path
    Assert-True ($document.base.strings.string.Count -gt 0) "$language localization XML parses"
    $ids = @($document.base.strings.string | ForEach-Object { $_.id })
    Assert-True (($ids | Group-Object | Where-Object Count -gt 1).Count -eq 0) "$language localization IDs are unique"
}

$prefabSource = Get-Content -Raw (Join-Path $root 'UI/ClanMercenaryContractExtensions.cs')
$prefabMatches = [regex]::Matches($prefabSource, '(?s)"""\s*(<.*?>)\s*"""')
Assert-True ($prefabMatches.Count -eq 2) 'expected embedded Clan prefab documents are present'
foreach ($match in $prefabMatches) { [xml]$match.Groups[1].Value | Out-Null }
Assert-True $true 'embedded UIExtenderEx prefab XML parses'

[xml]$clanPanelDocument = $prefabMatches[1].Groups[1].Value
$historyButton = $clanPanelDocument.SelectSingleNode("/Widget/Children/ButtonWidget[@Id='ContractHistoryButton']")
Assert-True ($null -ne $historyButton) 'contract history button is a top-level panel child'
Assert-True ($historyButton.DataSource -eq '{MercenaryContract}') 'contract history button has an explicit nested ViewModel data source'
Assert-True ($historyButton.'Command.Click' -eq 'ExecuteShowHistory') 'contract history button binds its click command'
Assert-True ($historyButton.DoNotPassEventsToChildren -eq 'true' -and
    $historyButton.UpdateChildrenStates -eq 'true') 'contract history button uses the working project button event pattern'
Assert-True ($historyButton.HorizontalAlignment -eq 'Right' -and
    $historyButton.VerticalAlignment -eq 'Top' -and
    [int]$historyButton.MarginTop -le 60 -and
    [int]$historyButton.MarginRight -le 15) 'contract history button is aligned with the panel top and right finance column'
$contentHistoryButton = $clanPanelDocument.SelectSingleNode("/Widget/Children/Widget[@DataSource='{MercenaryContract}']//ButtonWidget[@Id='ContractHistoryButton']")
Assert-True ($null -eq $contentHistoryButton) 'contract history button is outside scrollable and state-specific content'
$rootChildren = @($clanPanelDocument.Widget.Children.ChildNodes | Where-Object NodeType -eq Element)
Assert-True ($rootChildren[-1].Id -eq 'ContractHistoryButton') 'contract history button is layered after full-size content for hit testing'
$playerContractVm = Get-Content -Raw (Join-Path $root 'UI/PlayerMercenaryContractVM.cs')
Assert-True ($playerContractVm.Contains('public void ExecuteShowHistory()')) 'contract history ViewModel exposes the open command'
Assert-True ($playerContractVm.Contains('IsShowingHistory = !IsShowingHistory')) 'contract history button toggles the history panel on repeated clicks'
$historyScrollContainer = $clanPanelDocument.SelectSingleNode("//Widget[@Id='HistoryScrollContainer']")
Assert-True ($null -ne $historyScrollContainer) 'contract history owns a shared scroll container'
Assert-True ($null -ne $historyScrollContainer.SelectSingleNode("Children/ScrollablePanel") -and
    $null -ne $historyScrollContainer.SelectSingleNode("Children/Standard.VerticalScrollbar[@Id='HistoryScrollbar']")) 'contract history content and scrollbar share one overlay parent'
Assert-True ($historyScrollContainer.SelectSingleNode("Children/ScrollablePanel").MarginRight -eq '20') 'contract history content reserves width for its overlaid scrollbar'
$historyItemTemplate = $historyScrollContainer.SelectSingleNode(".//ListPanel[@Id='HistoryList']/ItemTemplate/BrushListPanel")
Assert-True ($historyItemTemplate.SuggestedHeight -ge 120) 'contract history rows reserve height for wrapped labeled values'
Assert-True (@($historyItemTemplate.SelectNodes("Children/Widget[@Sprite='GradientDivider_9']")).Count -eq 4) 'contract history rows visually separate all five columns'
$historyItemVm = Get-Content -Raw (Join-Path $root 'UI/MercenaryContractHistoryItemVM.cs')
foreach ($summary in @('EmployerSummary', 'ResultsSummary', 'RelationsSummary')) {
    Assert-True ($historyItemVm.Contains($summary)) "contract history item exposes wrapped $summary text"
}

$contractSource = Get-Content -Raw (Join-Path $root 'Contracts/MercenaryContract.cs')
$offerSource = Get-Content -Raw (Join-Path $root 'Contracts/MercenaryOffer.cs')
$historySource = Get-Content -Raw (Join-Path $root 'Contracts/MercenaryContractHistoryEntry.cs')
foreach ($entry in @(@('MercenaryContract', $contractSource, 30),
        @('MercenaryOffer', $offerSource, 9),
        @('MercenaryContractHistoryEntry', $historySource, 13))) {
    $ids = [regex]::Matches($entry[1], 'SaveableProperty\((\d+)\)') | ForEach-Object { [int]$_.Groups[1].Value }
    Assert-True (($ids | Group-Object | Where-Object Count -gt 1).Count -eq 0) "$($entry[0]) saveable property IDs are unique"
    Assert-True (($ids | Measure-Object -Maximum).Maximum -eq $entry[2]) "$($entry[0]) expected save schema is registered"
}

$definer = Get-Content -Raw (Join-Path $root 'SaveSystem/MercenarySaveableTypeDefiner.cs')
foreach ($required in @('MercenaryContract', 'MercenaryOffer', 'MercenaryContractState',
        'MercenaryContractHistoryEntry', 'MercenaryContractEndReason',
        'MercenaryStrategicPriority', 'MercenaryContractDuration',
        'Dictionary<string, MercenaryContract>', 'Dictionary<string, MercenaryOffer>',
        'Dictionary<string, CampaignTime>',
        'Dictionary<string, List<MercenaryContractHistoryEntry>>')) {
    Assert-True ($definer.Contains($required)) "save definer contains $required"
}

$subModule = Get-Content -Raw (Join-Path $root 'SubModule.cs')
$patchSource = (Get-Content -Raw (Join-Path $root 'Patches/MercenaryPaymentPatch.cs')) +
    (Get-Content -Raw (Join-Path $root 'Patches/MercenaryPriorityAiPatches.cs')) +
    (Get-Content -Raw (Join-Path $root 'Patches/ClanManagementSelectionPatch.cs'))
foreach ($patch in @('ClanManagementSelectionPatch', 'MercenaryPaymentPatch',
        'MercenaryPriorityArmyCallDistancePatch', 'MercenaryPriorityArmyCandidateWeightPatch',
        'MercenaryPriorityBehaviorScorePatch', 'MercenaryRecoveryTrainingPatch')) {
    Assert-True ($subModule.Contains("typeof($patch)")) "$patch is registered by SubModule"
    Assert-True ($patchSource.Contains("class $patch")) "$patch implementation exists"
}
foreach ($target in @('AddMercenaryIncome', 'CanLordCreateArmy', 'AiHourlyTick',
        'GetEffectiveDailyExperience', 'SetSelectedCategory')) {
    Assert-True ($patchSource.Contains($target)) "Harmony target reference $target exists"
}

$offerBehavior = Get-Content -Raw (Join-Path $root 'Behaviors/MercenaryOfferBehavior.cs')
$offerVm = Get-Content -Raw (Join-Path $root 'UI/PlayerMercenaryContractVM.cs')
$offerPrefab = Get-Content -Raw (Join-Path $root 'UI/ClanMercenaryContractExtensions.cs')
foreach ($token in @('GetServiceRequestKingdoms', 'RequestOffer', 'GetSavedOffers', 'RejectOffer', 'TryAccept')) {
    Assert-True ($offerBehavior.Contains($token)) "Offer Services behavior exposes $token"
}
Assert-True ($offerBehavior.Contains('_offers.TryGetValue(GetKey(clan, kingdom)')) 'Offer Services excludes kingdoms with an active saved offer'
foreach ($token in @('ExecuteShowOfferServices', 'ExecuteSubmitServiceRequest', 'RequestOffer', 'Refresh')) {
    Assert-True ($offerVm.Contains($token)) "Offer Services ViewModel chain contains $token"
}
foreach ($token in @('ExecuteShowOfferServices', 'ExecuteSubmitServiceRequest', 'ServiceKingdoms')) {
    Assert-True ($offerPrefab.Contains($token)) "Offer Services prefab binds $token"
}
foreach ($token in @('MouseScrollAxis="Vertical"', 'ServiceKingdomClipRect', 'ServiceKingdomScrollbar')) {
    Assert-True ($offerPrefab.Contains($token)) "Offer Services scrolling contains $token"
}
$clanTabWidths = Get-Content -Raw (Join-Path $root 'UI/ClanManagementTabWidthExtensions.cs')
foreach ($constant in @('Header.Tab.Left.Width.Scaled', 'Header.Tab.Center.Width.Scaled', 'Header.Tab.Right.Width.Scaled')) {
    Assert-True ($clanTabWidths.Contains($constant)) "Clan tab width adjusts $constant"
}

$kingdomAiBehavior = Get-Content -Raw (Join-Path $root 'Behaviors/MercenaryKingdomAiBehavior.cs')
$priorityBehavior = Get-Content -Raw (Join-Path $root 'Behaviors/MercenaryPriorityBehavior.cs')
Assert-True ($kingdomAiBehavior.Contains('MercenaryArmyLifecyclePolicy.OnArmyCreated')) 'army creation uses tested lifecycle policy'
Assert-True ($kingdomAiBehavior.Contains('MercenaryArmyLifecyclePolicy.OnArmyDispersed')) 'army dispersal uses tested lifecycle policy'
Assert-True ($priorityBehavior.Contains('MercenaryArmyLifecyclePolicy.CanEvaluatePartyExit')) 'party daily tick uses tested lifecycle policy'
Assert-True ($kingdomAiBehavior.Contains('private static void OnArmyCreated(Army? army)')) 'army creation accepts missing runtime army'
Assert-True ($kingdomAiBehavior.Contains('Army? army,')) 'army dispersal accepts missing runtime army'
Assert-True ($kingdomAiBehavior.Contains('Kingdom? kingdom = army?.Kingdom;')) 'army handlers read cleared kingdom safely'
Assert-True ($kingdomAiBehavior.Contains('Clan? playerClan = Clan.PlayerClan;')) 'army handlers tolerate missing player clan'
Assert-True ($priorityBehavior.Contains('MobileParty? armyLeaderParty = army?.LeaderParty;')) 'party tick tolerates cleared army leader'

$contractSource = Get-Content -Raw (Join-Path $root 'Contracts/MercenaryContract.cs')
Assert-True ($contractSource.Contains('MercenaryValuePolicy.NormalizePercentage')) 'saveable evaluation values use finite percentage normalization'
Assert-True ($contractSource.Contains('MercenaryValuePolicy.AdjustPercentage')) 'saveable evaluation deltas reject non-finite values'
Assert-True ($contractSource.Contains('MercenaryValuePolicy.AccumulateNonNegative')) 'persistent counters use overflow-safe accumulation'

$recoveryCommand = Get-Content -Raw (Join-Path $root 'Commands/MercenaryRecoveryTestCommands.cs')
foreach ($token in @('mm_recovery_priority_test', 'TrySetRecoveryPriority',
        'WarPartyComponents', 'PartySizeRatio', 'RunDiagnosticRecoveryCheck', 'SUMMARY')) {
    Assert-True ($recoveryCommand.Contains($token)) "recovery runtime scenario contains $token"
}

$runtimeTests = Get-Content -Raw (Join-Path $root 'Commands/MercenaryRuntimeTestCommands.cs')
Assert-True ($runtimeTests.Contains('CommandLineArgumentFunction("mm_runtime_test", "campaign")')) 'runtime test command is registered'
foreach ($role in @('free', 'vassal', 'mercenary', 'ruler')) {
    Assert-True ($runtimeTests.Contains("case `"$role`"")) "runtime test supports $role save"
}
Assert-True ($runtimeTests.Contains('RunDiagnosticNullArmyCallbacks')) 'runtime test executes null army callbacks'
Assert-True ($runtimeTests.Contains('Harmony.GetAllPatchedMethods')) 'runtime test inspects applied Harmony patches'
Assert-True ($runtimeTests.Contains('RunDiagnosticDailyEvaluation')) 'runtime test executes contract-only daily evaluation'

$historyCommands = Get-Content -Raw (Join-Path $root 'Commands/MercenaryHistoryTestCommands.cs')
foreach ($command in @('mm_contract_history', 'mm_contract_reputation', 'mm_history_test')) {
    Assert-True ($historyCommands.Contains($command)) "contract history command $command is registered"
}
foreach ($token in @('MercenaryDismissalService.EndService', 'MercenaryContractEndReason.MercenaryLeft',
        'CompleteContract(clan', 'OnClanChangedKingdom event', 'GetReputation(clan, employer)',
        'entry.FinalRelation', 'entry.ServiceEndTime')) {
    Assert-True ($historyCommands.Contains($token)) "contract history runtime scenario contains $token"
}

$requiredLocalization = @('MercenaryManagement_OfferServices',
    'MercenaryManagement_OfferServicesTitle', 'MercenaryManagement_OfferServicesHint',
    'MercenaryManagement_SubmitServiceRequest', 'MercenaryManagement_BackToOffers',
    'MercenaryManagement_ServiceRequestUnavailable',
    'MercenaryManagement_ServiceRequestDeclined',
    'MercenaryManagement_ServiceRequestCounteroffer',
    'MercenaryManagement_ContractHistory',
    'MercenaryManagement_HistoryEmployerValue',
    'MercenaryManagement_HistoryPeriodValue',
    'MercenaryManagement_HistoryResultsValue',
    'MercenaryManagement_HistoryRelationsValue',
    'MercenaryManagement_HistoryReasonExpired',
    'MercenaryManagement_HistoryReasonDismissed')
foreach ($language in @('EN', 'RU')) {
    [xml]$document = Get-Content -Raw (Join-Path $root "_Module/ModuleData/Languages/$language/strings.xml")
    $ids = @($document.base.strings.string | ForEach-Object { $_.id })
    foreach ($id in $requiredLocalization) { Assert-True ($ids -contains $id) "$language contains $id" }
}

Write-Host 'PASS: all source, XML, save schema, Harmony wiring, and Offer Services checks completed.'
