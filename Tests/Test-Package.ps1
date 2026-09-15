<#
.SYNOPSIS
  Restores and compiles a consumer project against the packed WebRTCme packages - tier 2 of
  doc/TestingPlan.md.

.DESCRIPTION
  Everything else in this repository tests the source tree. This tests the package: it builds
  Tests/WebRTCme.PackageTests, which references WebRTCme and WebRTCme.Middleware as PackageReference
  rather than ProjectReference, restoring them from a folder of .nupkg files.

  The packages should come from a CI run, not from a local pack. A package built on this machine
  carries this machine's Apple slices, and on Windows those are flat and unsignable - so testing one
  would test something that will never be published. Fetch the real thing first:

      gh run download <ci-run-id> -n nupkg -D artifacts

  Then:

      ./Tests/Test-Package.ps1 -Version 26.9.9

  What passing means: every target framework the package claims resolved, and the consumer surface
  compiled against it. What it does not mean: that the native half loads. Nothing here runs, and no
  compiler can tell you whether libwebrtc.aar is inside the .aar-shaped hole. That is tier 3.

.PARAMETER Version
  The package version to test. No default on purpose - testing "whatever is in the folder" is how
  you certify a package built last week. Read from the .nupkg file names if omitted, and refused if
  the folder holds more than one version.

.PARAMETER PackageSource
  Folder holding the .nupkg files. Defaults to artifacts/ at the repository root.

.PARAMETER Framework
  Build one target framework instead of all of them. Useful when chasing a single slice.
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
$project  = Join-Path $PSScriptRoot 'WebRTCme.PackageTests/WebRTCme.PackageTests.csproj'

if (-not $PackageSource) { $PackageSource = Join-Path $repoRoot 'artifacts' }
$PackageSource = [System.IO.Path]::GetFullPath($PackageSource)

if (-not (Test-Path $PackageSource)) {
    throw "No package source at $PackageSource. Download a CI artifact first: gh run download <run-id> -n nupkg -D artifacts"
}

# The digit after the id is what stops "WebRTCme." also matching WebRTCme.Middleware.
$packages = Get-ChildItem $PackageSource -Filter '*.nupkg' |
            Where-Object { $_.Name -match '^WebRTCme(\.Middleware)?\.\d' }

if (-not $packages) {
    throw "No WebRTCme packages in $PackageSource. Found: $((Get-ChildItem $PackageSource).Name -join ', ')"
}

# Version resolution. Stated wins; otherwise read it back from the file names, and refuse to guess
# when the folder holds more than one - an old .nupkg left behind is exactly how you end up
# testing the wrong bytes and believing the result.
if (-not $Version) {
    # @(...) because Sort-Object -Unique hands back a bare string when there is one match, and a
    # bare string has no Count - so the guard below would throw instead of running.
    $found = @($packages |
               ForEach-Object { $_.BaseName -replace '^WebRTCme(\.Middleware)?\.', '' } |
               Sort-Object -Unique)

    if ($found.Count -gt 1) {
        throw "More than one version in ${PackageSource}: $($found -join ', '). Pass -Version to say which."
    }
    $Version = $found[0]
    Write-Host "Version not given; using the only one present: $Version"
}

foreach ($id in 'WebRTCme', 'WebRTCme.Middleware') {
    $expected = Join-Path $PackageSource "$id.$Version.nupkg"
    if (-not (Test-Path $expected)) {
        throw "$id $Version is not in $PackageSource. The two packages are always published together, so both must be present."
    }
}

Write-Host ""
Write-Host "Package source : $PackageSource"
Write-Host "Version        : $Version"
Write-Host ""

# The version is not enough to identify a package here, and that nearly produced a false pass.
#
# NuGet caches by id and version. Two CI runs of the same commit both produce 26.9.9, with
# different bytes - a dependency moved, a slice was rebuilt - and the second restore finds 26.9.9
# already extracted under ~/.nuget/packages and never looks at the file in the feed. The build then
# reports success against the package from an hour ago. Observed exactly that on 2026-09-14: the
# cache held 64,134,427 bytes while the artifact under test was 64,134,436, and the run passed
# while the new package would in fact have failed the restore.
#
# So the cached copy goes before every run. It costs a re-extract from a folder feed, which is
# cheap, and it is the difference between testing the package and testing the name of one.
foreach ($id in 'WebRTCme', 'WebRTCme.Middleware') {
    # $HOME rather than $env:USERPROFILE, which is empty on macOS and Linux - where this would have
    # silently resolved to the filesystem root and evicted nothing.
    $cached = Join-Path $HOME ".nuget/packages/$($id.ToLowerInvariant())/$Version"
    if (Test-Path $cached) {
        Write-Host "Evicting cached $id $Version"
        Remove-Item $cached -Recurse -Force
    }
}

# obj/ for the same reason: a project.assets.json from a previous run lets restore decide there is
# nothing to do, which puts the stale package straight back.
$objDir = Join-Path $PSScriptRoot 'WebRTCme.PackageTests/obj'
if (Test-Path $objDir) { Remove-Item $objDir -Recurse -Force }

# RestoreAdditionalProjectSources rather than a nuget.config, so the feed is a parameter of the run
# instead of a fact about the checkout - and so nothing this script does can change what an ordinary
# build of the repository restores.
$arguments = @(
    'build', $project
    '-c', 'Release'
    "-p:WebRTCmePackageVersion=$Version"
    "-p:RestoreAdditionalProjectSources=$PackageSource"
    '--nologo'
)
if ($Framework) { $arguments += @('-f', $Framework) }

# Restore is deliberately not separated out: a consumer types `dotnet build` and gets both, and a
# fault in the restore half is exactly the kind this is looking for.
& dotnet @arguments
$exit = $LASTEXITCODE

Write-Host ""
if ($exit -ne 0) {
    Write-Host "Package test FAILED for $Version." -ForegroundColor Red
    Write-Host "A missing slice reads as NU1202 (no compatible assets); a fold that did not make it" -ForegroundColor Red
    Write-Host "into the package reads as CS0246 on a type that exists perfectly well in the repo." -ForegroundColor Red
    exit $exit
}

# Compiling is not enough on its own, and this is the reason.
#
# NuGet falls back: a net10.0-android project with no net10.0-android slice in the package resolves
# lib/net10.0/ instead and compiles perfectly, because the common API is identical across slices.
# What it would not have is any Android binding - the whole platform half - and nothing says so
# until the app runs on a phone and finds no native implementation behind the interface.
#
# So the assets file is read back and each target framework has to have resolved its OWN slice.
$assetsFile = Join-Path $PSScriptRoot 'WebRTCme.PackageTests/obj/project.assets.json'
if (-not (Test-Path $assetsFile)) { throw "No assets file at $assetsFile; cannot check which slices were resolved." }

$assets = Get-Content $assetsFile -Raw | ConvertFrom-Json
$sliceFailures = New-Object System.Collections.Generic.List[string]
$checked = 0

foreach ($target in $assets.targets.PSObject.Properties) {
    $tfm = $target.Name

    # net10.0-windows10.0.22621.0 -> net10.0-windows; net10.0-android -> net10.0-android; net10.0 -> net10.0
    if ($tfm -notmatch '^(net\d+\.\d+(?:-[a-z]+)?)') { continue }
    $base = $Matches[1]

    foreach ($id in 'WebRTCme', 'WebRTCme.Middleware') {
        $entry = $target.Value.PSObject.Properties | Where-Object { $_.Name -eq "$id/$Version" }
        if (-not $entry) {
            $sliceFailures.Add("$tfm : $id did not resolve at all")
            continue
        }

        $compile = @()
        if ($entry.Value.PSObject.Properties.Name -contains 'compile') {
            $compile = $entry.Value.compile.PSObject.Properties.Name | Where-Object { $_ -like "*/$id.dll" }
        }
        if (-not $compile) {
            $sliceFailures.Add("$tfm : $id resolved no compile assembly")
            continue
        }

        # The plain TFM must match exactly, or "lib/net10.0-android36.0/" would satisfy "net10.0".
        $ok = if ($base -eq $tfm -and $tfm -notmatch '-') { $compile -like "lib/$base/*" }
              else { $compile -like "lib/$base*/*" }

        $checked++
        if (-not $ok) {
            $sliceFailures.Add("$tfm : $id fell back to $compile instead of a lib/$base* slice")
        }
        else {
            Write-Host ("  {0,-30} {1,-20} {2}" -f $tfm, $id, $compile)
        }
    }
}

Write-Host ""
if ($sliceFailures.Count -gt 0) {
    Write-Host "Package test FAILED for ${Version}: wrong slices resolved." -ForegroundColor Red
    $sliceFailures | ForEach-Object { Write-Host "  $_" -ForegroundColor Red }
    Write-Host "A fallback to lib/net10.0 compiles but ships no platform implementation." -ForegroundColor Red
    exit 1
}

Write-Host "Package test passed: $Version restores and compiles for every target framework on this host," -ForegroundColor Green
Write-Host "and all $checked package/framework pairs resolved their own platform slice." -ForegroundColor Green
Write-Host "Not proven here: that the native half loads. That needs a device - tier 3."
