<#
.SYNOPSIS
  Runs the runtime loopback tests against the packed WebRTCme package - tier 3 of
  doc/TestingPlan.md.

.DESCRIPTION
  Tiers 1 and 2 are managed-only: neither loads libwebrtc.aar, WebRTC.framework or
  WebRtcInterop.dll, so a package whose native payload is missing or built for the wrong
  architecture passes both. This runs a real negotiation - two RTCPeerConnections in one process,
  offer and answer and ICE handed between them in code - and is the tier that finds that out.

  It runs on the machine it tests. Windows runs the net10.0-windows slice; the Mac mini runs
  net10.0-maccatalyst. Android and iOS need a runner app on the device and are phase 4.

  As with tier 2, fetch the package from a CI run rather than packing locally - a locally packed
  one carries this machine's Apple slices, which on Windows are flat and unpublishable:

      gh run download <ci-run-id> -n nupkg -D artifacts
      ./Tests/Test-Device.ps1 -Version 26.9.9

.PARAMETER Version
  Package version to test. Read from the .nupkg file names if omitted, and refused if the folder
  holds more than one.

.PARAMETER PackageSource
  Folder holding the .nupkg files. Defaults to artifacts/ at the repository root.

.PARAMETER Framework
  Target framework to run. Defaults to this machine's: net10.0-windows on Windows,
  net10.0-maccatalyst on macOS.
#>
[CmdletBinding()]
param(
    [string] $Version,
    [string] $PackageSource,
    [string] $Framework
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repoRoot = Split-Path -Parent $PSScriptRoot
$project  = Join-Path $PSScriptRoot 'WebRTCme.DeviceTests/WebRTCme.DeviceTests.csproj'

if (-not $PackageSource) { $PackageSource = Join-Path $repoRoot 'artifacts' }
$PackageSource = [System.IO.Path]::GetFullPath($PackageSource)

if (-not (Test-Path $PackageSource)) {
    throw "No package source at $PackageSource. Download a CI artifact first: gh run download <run-id> -n nupkg -D artifacts"
}

if (-not $Framework) {
    $Framework = if ($IsWindows) { 'net10.0-windows10.0.22621.0' }
                 elseif ($IsMacOS) { 'net10.0-maccatalyst' }
                 else { throw "Tier 3 runs on Windows or macOS. Android and iOS are phase 4." }
}

if (-not $Version) {
    $found = @(Get-ChildItem $PackageSource -Filter 'WebRTCme.*.nupkg' |
               Where-Object { $_.Name -match '^WebRTCme\.\d' } |
               ForEach-Object { $_.BaseName -replace '^WebRTCme\.', '' } |
               Sort-Object -Unique)
    if ($found.Count -eq 0) { throw "No WebRTCme package in $PackageSource." }
    if ($found.Count -gt 1) { throw "More than one version in ${PackageSource}: $($found -join ', '). Pass -Version." }
    $Version = $found[0]
    Write-Host "Version not given; using the only one present: $Version"
}

# Same eviction as tier 2, and for the same reason: NuGet caches by id and version, two CI runs of
# one commit both produce the same version with different bytes, and a cached copy means testing
# the package from an hour ago while believing otherwise.
$cached = Join-Path $env:USERPROFILE ".nuget/packages/webrtcme/$Version"
if (Test-Path $cached) {
    Write-Host "Evicting cached WebRTCme $Version"
    Remove-Item $cached -Recurse -Force
}
$objDir = Join-Path $PSScriptRoot 'WebRTCme.DeviceTests/obj'
if (Test-Path $objDir) { Remove-Item $objDir -Recurse -Force }

Write-Host ""
Write-Host "Package source : $PackageSource"
Write-Host "Version        : $Version"
Write-Host "Framework      : $Framework"
Write-Host ""

& dotnet run --project $project -c Release -f $Framework `
    "-p:WebRTCmePackageVersion=$Version" `
    "-p:RestoreAdditionalProjectSources=$PackageSource"
$exit = $LASTEXITCODE

Write-Host ""
if ($exit -ne 0) {
    Write-Host "Device test FAILED for $Version on $Framework." -ForegroundColor Red
    Write-Host "A missing native payload reads as DllNotFoundException on the first P/Invoke;" -ForegroundColor Red
    Write-Host "one built for the wrong architecture reads as BadImageFormatException." -ForegroundColor Red
    exit $exit
}

Write-Host "Device test passed: $Version negotiates end to end on $Framework." -ForegroundColor Green
