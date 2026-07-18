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

$contractSource = Get-Content -Raw (Join-Path $root 'Contracts/MercenaryContract.cs')
$offerSource = Get-Content -Raw (Join-Path $root 'Contracts/MercenaryOffer.cs')
foreach ($entry in @(@('MercenaryContract', $contractSource, 27), @('MercenaryOffer', $offerSource, 9))) {
    $ids = [regex]::Matches($entry[1], 'SaveableProperty\((\d+)\)') | ForEach-Object { [int]$_.Groups[1].Value }
    Assert-True (($ids | Group-Object | Where-Object Count -gt 1).Count -eq 0) "$($entry[0]) saveable property IDs are unique"
    Assert-True (($ids | Measure-Object -Maximum).Maximum -eq $entry[2]) "$($entry[0]) expected save schema is registered"
}

$definer = Get-Content -Raw (Join-Path $root 'SaveSystem/MercenarySaveableTypeDefiner.cs')
foreach ($required in @('MercenaryContract', 'MercenaryOffer', 'MercenaryContractState',
        'MercenaryStrategicPriority', 'MercenaryContractDuration',
        'Dictionary<string, MercenaryContract>', 'Dictionary<string, MercenaryOffer>',
        'Dictionary<string, CampaignTime>')) {
    Assert-True ($definer.Contains($required)) "save definer contains $required"
}

$subModule = Get-Content -Raw (Join-Path $root 'SubModule.cs')
$patchSource = (Get-Content -Raw (Join-Path $root 'Patches/MercenaryPaymentPatch.cs')) +
    (Get-Content -Raw (Join-Path $root 'Patches/MercenaryPriorityAiPatches.cs')) +
    (Get-Content -Raw (Join-Path $root 'Patches/ClanManagementSelectionPatch.cs'))
foreach ($patch in @('ClanManagementSelectionPatch', 'MercenaryPaymentPatch',
        'MercenaryPriorityArmyCallDistancePatch', 'MercenaryPriorityArmyCandidateWeightPatch',
        'MercenaryPriorityBehaviorScorePatch')) {
    Assert-True ($subModule.Contains("typeof($patch)")) "$patch is registered by SubModule"
    Assert-True ($patchSource.Contains("class $patch")) "$patch implementation exists"
}
foreach ($target in @('AddMercenaryIncome', 'CanLordCreateArmy', 'AiHourlyTick', 'SetSelectedCategory')) {
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

$runtimeTests = Get-Content -Raw (Join-Path $root 'Commands/MercenaryRuntimeTestCommands.cs')
Assert-True ($runtimeTests.Contains('CommandLineArgumentFunction("mm_runtime_test", "campaign")')) 'runtime test command is registered'
foreach ($role in @('free', 'vassal', 'mercenary', 'ruler')) {
    Assert-True ($runtimeTests.Contains("case `"$role`"")) "runtime test supports $role save"
}
Assert-True ($runtimeTests.Contains('RunDiagnosticNullArmyCallbacks')) 'runtime test executes null army callbacks'
Assert-True ($runtimeTests.Contains('Harmony.GetAllPatchedMethods')) 'runtime test inspects applied Harmony patches'
Assert-True ($runtimeTests.Contains('RunDiagnosticDailyEvaluation')) 'runtime test executes contract-only daily evaluation'

$requiredLocalization = @('MercenaryManagement_OfferServices',
    'MercenaryManagement_OfferServicesTitle', 'MercenaryManagement_OfferServicesHint',
    'MercenaryManagement_SubmitServiceRequest', 'MercenaryManagement_BackToOffers',
    'MercenaryManagement_ServiceRequestUnavailable',
    'MercenaryManagement_ServiceRequestDeclined',
    'MercenaryManagement_ServiceRequestCounteroffer')
foreach ($language in @('EN', 'RU')) {
    [xml]$document = Get-Content -Raw (Join-Path $root "_Module/ModuleData/Languages/$language/strings.xml")
    $ids = @($document.base.strings.string | ForEach-Object { $_.id })
    foreach ($id in $requiredLocalization) { Assert-True ($ids -contains $id) "$language contains $id" }
}

Write-Host 'PASS: all source, XML, save schema, Harmony wiring, and Offer Services checks completed.'
