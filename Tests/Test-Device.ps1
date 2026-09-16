<#
.SYNOPSIS
  Runs the runtime loopback tests against the packed WebRTCme package - tier 3 of
  doc/TestingPlan.md.

.DESCRIPTION
  Tiers 1 and 2 are managed-only: neither loads libwebrtc.aar, WebRTC.framework or
  WebRtcInterop.dll, so a package whose native payload is missing or built for the wrong
  architecture passes both. This runs a real negotiation - two RTCPeerConnections in one process,
  offer and answer and ICE handed between them in code - and is the tier that finds that out.

  It runs on the machine it tests, and that machine is Windows. Mac Catalyst turned out to belong
  with Android and iOS rather than with Windows: it builds an .app rather than an executable, xUnit
  v3 will not build a test project without an app host, and there is no app host for
  maccatalyst-x64. All three need a device runner - phase 4 of doc/TestingPlan.md.

  As with tier 2, fetch the package from a CI run rather than packing locally - a locally packed
  one carries this machine's Apple slices, which on Windows are flat and unpublishable:

      gh run download <ci-run-id> -n nupkg -D artifacts
      ./Tests/Test-Device.ps1 -Version 26.9.9

.PARAMETER Version
  Package version to test. Read from the .nupkg file names if omitted, and refused if the folder
  holds more than one.

.PARAMETER PackageSource
  Folder holding the .nupkg files. Defaults to artifacts/ at the repository root.

.NOTES
  There is no framework parameter. The project sets its own, because passing -f or
  -p:TargetFramework does not survive the implicit restore: restore resolves the project's declared
  framework while the build uses the one given, and the build fails with NETSDK1005 saying the
  assets file has no target for it.
#>
[CmdletBinding()]
param(
    [string] $Version,
    [string] $PackageSource
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

if (-not $IsWindows) {
    throw @"
Tier 3 runs on Windows only.

Mac Catalyst cannot run as a plain test process - it builds an .app, xUnit v3 requires an app host,
and there is no app host for maccatalyst-x64 (NETSDK1084). It needs the device runner that Android
and iOS need, which is phase 4 of doc/TestingPlan.md.
"@
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
# $HOME rather than $env:USERPROFILE, which is empty on macOS - where this would have silently
# evicted nothing and tested whatever was already cached.
$cached = Join-Path $HOME ".nuget/packages/webrtcme/$Version"
if (Test-Path $cached) {
    Write-Host "Evicting cached WebRTCme $Version"
    Remove-Item $cached -Recurse -Force
}
$objDir = Join-Path $PSScriptRoot 'WebRTCme.DeviceTests/obj'
if (Test-Path $objDir) { Remove-Item $objDir -Recurse -Force }

Write-Host ""
Write-Host "Package source : $PackageSource"
Write-Host "Version        : $Version"
Write-Host ""

# Restore, build and run as three announced steps rather than one `dotnet run`.
#
# Because a silent hang needs to be localised before it can be explained. On a hosted Windows
# runner this tier printed its header and then produced nothing at all - for six hours the first
# time, until the job limit stopped it - and a single `dotnet run` cannot say whether it died in
# restore, in the build, or in the tests. Three steps and three messages can.
Write-Host "Restoring..."
& dotnet restore $project `
    "-p:WebRTCmePackageVersion=$Version" `
    "-p:RestoreAdditionalProjectSources=$PackageSource"
if ($LASTEXITCODE -ne 0) { Write-Host "restore failed" -ForegroundColor Red; exit 1 }

Write-Host "Building..."
& dotnet build $project -c Release --no-restore `
    "-p:WebRTCmePackageVersion=$Version" `
    "-p:RestoreAdditionalProjectSources=$PackageSource"
if ($LASTEXITCODE -ne 0) { Write-Host "build failed" -ForegroundColor Red; exit 1 }

Write-Host "Running the scenarios..."
& dotnet run --project $project -c Release --no-build `
    "-p:WebRTCmePackageVersion=$Version" `
    "-p:RestoreAdditionalProjectSources=$PackageSource"
$exit = $LASTEXITCODE

# The device-enumeration regression again, on its own, because it cannot prove anything in a
# process where a call has already happened.
#
# The damage it looks for is process-global: once a negotiation has reached DTLS the audio device
# module stops reporting microphones for the life of the process. A test that runs after the
# negotiation test takes an already-broken baseline - zero microphones - and then finds it
# unchanged, which is true and worthless. Written without this second pass, the test passed against
# the very build it was written to catch.
#
# xUnit gives no ordering contract within a class, so the fix is a second process rather than an
# ordering attribute: here nothing has negotiated yet, and the baseline is real.
if ($exit -eq 0) {
    Write-Host ""
    Write-Host "Re-running the device-enumeration regression in a process where no call has happened..."

    & dotnet run --project $project -c Release --no-build `
        "-p:WebRTCmePackageVersion=$Version" `
        "-p:RestoreAdditionalProjectSources=$PackageSource" `
        -- -filterVSTest "FullyQualifiedName~A_completed_call_does_not_change_what_devices_exist"
    $exit = $LASTEXITCODE
}

Write-Host ""
if ($exit -ne 0) {
    Write-Host "Device test FAILED for $Version." -ForegroundColor Red
    Write-Host "A missing native payload reads as DllNotFoundException on the first P/Invoke;" -ForegroundColor Red
    Write-Host "one built for the wrong architecture reads as BadImageFormatException." -ForegroundColor Red
    exit $exit
}

Write-Host "Device test passed: $Version negotiates end to end on this host." -ForegroundColor Green
