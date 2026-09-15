<#
.SYNOPSIS
  Runs the loopback scenarios in a browser. Phase 5 of doc/TestingPlan.md.

.DESCRIPTION
  The Blazor binding is JSInterop over the browser's own WebRTC implementation, so unlike the other
  four it cannot be reached from a test process - there is no browser in one. The scenarios
  therefore run inside a page: Tests/WebRTCme.BlazorTestHost is a Blazor WebAssembly app that runs
  them and writes each result into the DOM, and Tests/WebRTCme.BlazorTests drives it with Playwright
  and asserts on what it finds.

  This script is what joins the two. It builds the host against the package under test, serves it,
  waits until it answers, and runs the tests against that URL.

  Blazor is the one platform in the suite needing no hardware: Chromium is launched with its
  fake-media-device switches, so there is a synthetic camera and microphone and nothing competes for
  the machine's real one. That also means this is the tier that can run while the Windows app has
  the camera.

.PARAMETER Version
  Package version to test. Read from the .nupkg file names if omitted.

.PARAMETER PackageSource
  Folder holding the .nupkg files. Defaults to artifacts/ at the repository root.

.PARAMETER Port
  Port to serve the host on. Defaults to 5177.

.PARAMETER SkipBrowserInstall
  Skip `playwright install chromium`. It is a no-op once the browser is present, so this is only
  worth setting on a machine with no network.
#>
[CmdletBinding()]
param(
    [string] $Version,
    [string] $PackageSource,
    [int]    $Port = 5177,
    [switch] $SkipBrowserInstall
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repoRoot = Split-Path -Parent $PSScriptRoot
$host_    = Join-Path $PSScriptRoot 'WebRTCme.BlazorTestHost/WebRTCme.BlazorTestHost.csproj'
$tests    = Join-Path $PSScriptRoot 'WebRTCme.BlazorTests/WebRTCme.BlazorTests.csproj'
$url      = "http://127.0.0.1:$Port"

if (-not $PackageSource) { $PackageSource = Join-Path $repoRoot 'artifacts' }
$PackageSource = [System.IO.Path]::GetFullPath($PackageSource)
if (-not (Test-Path $PackageSource)) {
    throw "No package source at $PackageSource. Download a CI artifact first: gh run download <run-id> -n nupkg -D artifacts"
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

Write-Host ""
Write-Host "Package source : $PackageSource"
Write-Host "Version        : $Version"
Write-Host "Serving on     : $url"
Write-Host ""

# The same cache eviction the other tiers do. A version number is not enough on its own: a package
# already in the global cache is used in preference to the folder feed, so a rebuilt .nupkg with the
# same id and version would be ignored and the run would silently test the old bytes.
$cached = Join-Path $env:USERPROFILE ".nuget/packages/webrtcme/$Version"
if (Test-Path $cached) {
    Write-Host "Evicting cached WebRTCme $Version"
    Remove-Item -Recurse -Force $cached
}

$server = $null
try {
    Write-Host "Building the host..."
    & dotnet build $host_ -c Release `
        "-p:WebRTCmePackageVersion=$Version" "-p:RestoreAdditionalProjectSources=$PackageSource" --nologo |
        Select-String -Pattern 'error|Build succeeded' | Select-Object -First 5
    if ($LASTEXITCODE -ne 0) { throw "the Blazor host failed to build against $Version" }

    if (-not $SkipBrowserInstall) {
        Write-Host "Ensuring Chromium is installed for Playwright..."
        & dotnet build $tests -c Release --nologo | Out-Null
        if ($LASTEXITCODE -ne 0) { throw "the Blazor tests failed to build" }

        $installer = Get-ChildItem (Join-Path $PSScriptRoot 'WebRTCme.BlazorTests/bin/Release') `
                        -Recurse -Filter 'playwright.ps1' -ErrorAction SilentlyContinue |
                     Select-Object -First 1
        if (-not $installer) { throw "playwright.ps1 not found - is Microsoft.Playwright referenced?" }
        & pwsh $installer.FullName install chromium
        if ($LASTEXITCODE -ne 0) { throw "playwright install chromium failed" }
    }

    Write-Host "Starting the host..."
    $server = Start-Process dotnet `
        -ArgumentList @('run', '--project', $host_, '-c', 'Release', '--no-build', '--urls', $url) `
        -PassThru -WindowStyle Hidden

    # Poll rather than sleep: a warm build answers in a second or two, a cold one takes longer, and
    # waiting the worst case every time is wasteful. A host that never answers must fail here with
    # something readable rather than let every test time out one by one in the browser.
    $deadline = (Get-Date).AddSeconds(90)
    $ready = $false
    while (-not $ready -and (Get-Date) -lt $deadline) {
        if ($server.HasExited) { throw "the host exited before it served anything (exit $($server.ExitCode))" }
        try {
            $probe = Invoke-WebRequest -Uri $url -UseBasicParsing -TimeoutSec 3
            $ready = $probe.StatusCode -eq 200
        } catch {
            Start-Sleep -Milliseconds 500
        }
    }
    if (-not $ready) { throw "the host did not answer on $url within 90s" }

    Write-Host "Running the scenarios in headless Chromium..."
    Write-Host ""
    $env:WEBRTCME_BLAZOR_URL = $url
    & dotnet run --project $tests -c Release --no-build
    $failed = $LASTEXITCODE -ne 0
}
finally {
    if ($server -and -not $server.HasExited) {
        Stop-Process -Id $server.Id -Force -ErrorAction SilentlyContinue
    }
    Remove-Item Env:\WEBRTCME_BLAZOR_URL -ErrorAction SilentlyContinue
}

Write-Host ""
if ($failed) {
    Write-Host "Blazor test FAILED for $Version." -ForegroundColor Red
    Write-Host "A JsInterop.js missing from the package reads as a failure to create anything at all;" -ForegroundColor Red
    Write-Host "the browser console output is included in the assertion message." -ForegroundColor Red
    exit 1
}

Write-Host "Blazor test passed: $Version negotiates end to end in the browser." -ForegroundColor Green
