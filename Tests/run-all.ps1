$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot

Write-Host '== Debug compilation =='
dotnet build (Join-Path $root 'MercenaryManagement.csproj') --configuration Debug --no-restore
if ($LASTEXITCODE -ne 0) { throw 'Debug compilation failed.' }

Write-Host '== Deterministic domain scenarios =='
dotnet run --project (Join-Path $PSScriptRoot 'MercenaryManagement.StaticTests.csproj') --configuration Debug
if ($LASTEXITCODE -ne 0) { throw 'Static domain scenarios failed.' }

Write-Host '== Static source and asset validation =='
& (Join-Path $PSScriptRoot 'run-static-validation.ps1')

Write-Host 'ALL PRE-RUNTIME CHECKS PASSED.'
