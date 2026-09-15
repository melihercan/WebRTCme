<#
.SYNOPSIS
  Runs the loopback scenarios on a device - Android, iOS or Mac Catalyst. Phase 4 of
  doc/TestingPlan.md.

.DESCRIPTION
  Phase 3 runs the same scenarios as a plain Windows process. These three platforms cannot: they
  build an .app or an .apk rather than an executable, xUnit v3 will not build a test project without
  an app host, and there is no app host for maccatalyst-x64 or for a phone. So the scenarios run
  inside a small MAUI app - Tests/WebRTCme.DeviceTests.Runner - which prints one line per result and
  writes a report file.

  XHarness is the usual tool for this and is not used. The newest build on the dotnet-eng feed is
  from September 2023: it predates .NET 10, and it drives Apple devices through mlaunch, which on
  this SDK hangs with "Please connect the device" on a locked phone and does not parse :v2:udid=
  selectors. adb and devicectl work, and this drives them directly.

  Each platform is run twice, deliberately. The device-enumeration scenario needs a process in which
  no call has happened - the Windows audio device module used to stop counting microphones once a
  negotiation reached DTLS, so a baseline taken after a call is the broken one and "unchanged" holds
  trivially. So: everything once, then that scenario alone in a fresh process.

.PARAMETER Platform
  android, ios or maccatalyst.

.PARAMETER Version
  Package version to test. Read from the .nupkg file names if omitted.

.PARAMETER PackageSource
  Folder holding the .nupkg files. Defaults to artifacts/ at the repository root.

.PARAMETER Mac
  Host running the Mac, for ios and maccatalyst. Defaults to WEBRTCME_MAC_HOST or 192.168.1.33.

.PARAMETER DeviceId
  iOS only: the paired identifier from `xcrun devicectl list devices`. Defaults to
  WEBRTCME_IOS_DEVICE. Several iPhones can be paired and reported "available" whether or not they
  are plugged in, so this is worth stating rather than guessing.

.PARAMETER SkipBuild
  Use what is already installed rather than rebuilding and reinstalling.
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory)][ValidateSet('android', 'ios', 'maccatalyst')][string] $Platform,
    [string] $Version,
    [string] $PackageSource,
    [string] $Mac,
    [string] $DeviceId,
    [switch] $SkipBuild
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repoRoot  = Split-Path -Parent $PSScriptRoot
$runner    = Join-Path $PSScriptRoot 'WebRTCme.DeviceTests.Runner/WebRTCme.DeviceTests.Runner.csproj'
$appId     = 'com.melihercan.webrtcme.devicetests'

# The scenario that has to run on its own, and why - see the description above.
$isolated  = 'ACompletedCallDoesNotChangeWhatDevicesExist'

if (-not $PackageSource) { $PackageSource = Join-Path $repoRoot 'artifacts' }
$PackageSource = [System.IO.Path]::GetFullPath($PackageSource)
if (-not (Test-Path $PackageSource)) {
    throw "No package source at $PackageSource. Download a CI artifact first: gh run download <run-id> -n nupkg -D artifacts"
}

if (-not $Mac)      { $Mac = if ($env:WEBRTCME_MAC_HOST) { $env:WEBRTCME_MAC_HOST } else { '192.168.1.33' } }
if (-not $DeviceId) { $DeviceId = $env:WEBRTCME_IOS_DEVICE }

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

# A version number is not enough on its own. A package already in the global cache is preferred to
# the folder feed, so a rebuilt .nupkg carrying the same id and version is ignored and the run
# silently tests the old bytes. That is not hypothetical here: CI can pack the same date-based
# version more than once in a day, and an earlier 26.9.15 exists that predates two fixes.
$cached = Join-Path $env:USERPROFILE ".nuget/packages/webrtcme/$Version"
if (Test-Path $cached) {
    Write-Host "Evicting cached WebRTCme $Version"
    Remove-Item -Recurse -Force $cached
}
Write-Host ""
Write-Host "Platform       : $Platform"
Write-Host "Package source : $PackageSource"
Write-Host "Version        : $Version"
Write-Host ""

# ---------------------------------------------------------------- reading results
#
# The runner prints one line per scenario and a summary line. A run that produces no summary did not
# finish - the app died, or never started - and that must fail rather than look like zero failures.

function Read-Outcome {
    param([string[]] $Lines, [string] $What)

    # Deduplicated on the marker onwards, not on the whole line, because the log prefixes a
    # timestamp and a tag. The runner deliberately reports through both Console.WriteLine and
    # Debug.WriteLine - on Apple platforms stdout and the system log are different pipes and which
    # one a launch mechanism captures varies - and on Android the two are the same pipe, so every
    # line arrives twice. Each scenario runs once per invocation and names are unique, so identical
    # payloads are duplicates rather than distinct results.
    $scenarios = $Lines | Where-Object { $_ -match 'WEBRTCME-SCENARIO' } |
                 ForEach-Object { $_ -replace '^.*?(?=WEBRTCME-SCENARIO)', '' } |
                 Select-Object -Unique
    $summary   = $Lines | Where-Object { $_ -match 'WEBRTCME-SUMMARY' } |
                 ForEach-Object { $_ -replace '^.*?(?=WEBRTCME-SUMMARY)', '' } |
                 Select-Object -Last 1

    foreach ($line in $scenarios) {
        $colour = if ($line -match '\| FAILED ') { 'Red' } elseif ($line -match '\| SKIPPED') { 'Yellow' } else { 'Green' }
        Write-Host ("  " + ($line -replace '.*WEBRTCME-SCENARIO \| ', '')) -ForegroundColor $colour
    }

    if (-not $summary) {
        Write-Host "  no summary line - the runner did not finish ($What)" -ForegroundColor Red
        return $false
    }

    Write-Host ("  " + ($summary -replace '.*WEBRTCME-SUMMARY \| ', 'summary: ')) -ForegroundColor Cyan
    return ($summary -match 'failed:0')
}

# ---------------------------------------------------------------- android

function Invoke-Android {
    param([string] $Filter)

    $adb = Get-Command adb -ErrorAction SilentlyContinue
    if (-not $adb) {
        $adb = "${env:ProgramFiles(x86)}\Android\android-sdk\platform-tools\adb.exe"
        if (-not (Test-Path $adb)) { throw "adb not found on PATH or at $adb" }
    } else { $adb = $adb.Source }

    & $adb logcat -c
    & $adb shell am force-stop $appId | Out-Null

    # The activity is named explicitly in MainActivity's [Activity(Name = ...)] rather than left to
    # MAUI, which would generate a hash of the namespace. Resolved from the device anyway, so a
    # rename is a clear failure here instead of a silent "component does not exist" from am start.
    $resolved = (& $adb shell cmd package resolve-activity --brief -c android.intent.category.LAUNCHER $appId 2>$null |
                 Select-Object -Last 1).Trim()
    if (-not $resolved -or $resolved -notmatch "^$([regex]::Escape($appId))/") {
        throw "could not resolve the runner's launcher activity for $appId (got '$resolved'). Is it installed?"
    }

    $startArgs = @('shell', 'am', 'start', '-n', $resolved)
    if ($Filter) { $startArgs += @('--es', 'filter', $Filter) }
    $started = & $adb @startArgs 2>&1
    if ($started -match 'Error') { throw "could not start the runner: $started" }

    # Poll rather than sleep a fixed time: a passing run takes about a second, a hung negotiation
    # takes the scenario's own 30s timeout, and waiting the worst case every time is wasteful.
    $deadline = (Get-Date).AddSeconds(120)
    do {
        Start-Sleep -Seconds 3
        $log = & $adb logcat -d 2>$null | Where-Object { $_ -match 'WEBRTCME-' }
    } while (-not ($log | Where-Object { $_ -match 'WEBRTCME-SUMMARY' }) -and (Get-Date) -lt $deadline)

    & $adb shell am force-stop $appId | Out-Null
    return Read-Outcome -Lines $log -What 'android'
}

# ---------------------------------------------------------------- mac catalyst

function Invoke-MacCatalyst {
    param([string] $Filter)

    # The bundle is named from ApplicationTitle rather than the assembly - "WebRTCme Device
    # Tests.app" - so the path contains spaces and every use of it has to stay quoted.
    $app = '$HOME/Projects/WebRTCme/Tests/WebRTCme.DeviceTests.Runner/bin/Debug/net10.0-maccatalyst/maccatalyst-x64/WebRTCme Device Tests.app/Contents/MacOS/WebRTCme.DeviceTests.Runner'
    $filterArg = if ($Filter) { "--filter=$Filter" } else { '' }

    # A literal here-string, with the two variable parts substituted afterwards. Every $ below
    # belongs to the remote shell, and writing it in an interpolating string means escaping each one
    # - which is how the first version of this silently sent a script the shell could not run and
    # reported "the runner did not finish".
    #
    # The executable inside the bundle is run directly so stdout comes back over SSH; `open` would
    # detach it and send its output to the system log instead. No window server session is needed.
    #
    # Polled rather than slept: a passing run takes about three seconds, a hung negotiation takes the
    # scenario's own 30s timeout. And no `timeout` command - macOS has none, and its absence is
    # silent, because "command not found" leaves the exit code to whatever came next in the pipeline.
    $script = @'
APP="__APP__"
LOG=$(mktemp)
"$APP" __FILTER__ > "$LOG" 2>&1 &
APPPID=$!
for i in $(seq 1 60); do
  grep -q WEBRTCME-SUMMARY "$LOG" && break
  sleep 2
done
kill $APPPID 2>/dev/null
grep WEBRTCME- "$LOG"
rm -f "$LOG"
'@

    # Carriage returns stripped, and this is not cosmetic. This file has CRLF line endings, so the
    # here-string above carries them into the script sent over SSH - and zsh treats the CR as part of
    # the command, failing every line with "command not found: do^M" while the exit status and the
    # WEBRTCME- grep both come back empty. The harness then reports "the runner did not finish",
    # which is indistinguishable from an app that crashed on launch.
    $remote = $script.Replace('__APP__', $app).Replace('__FILTER__', $filterArg).Replace("`r", '')

    $log = ssh -o BatchMode=yes $Mac $remote 2>&1
    return Read-Outcome -Lines $log -What 'maccatalyst'
}

# ---------------------------------------------------------------- ios

function Invoke-Ios {
    param([string] $Filter)

    if (-not $DeviceId) {
        throw "iOS needs -DeviceId (or WEBRTCME_IOS_DEVICE): the paired identifier from " +
              "xcrun devicectl list devices. Several iPhones can be paired and reported " +
              "available whether or not they are plugged in, so this is not guessed."
    }

    # The filter travels as an environment variable, not an argument. devicectl documents trailing
    # <command-line-arguments> and they never reach the app: passed plainly or after a -- separator,
    # it still ran all five scenarios. devicectl does forward anything prefixed DEVICECTL_CHILD_,
    # so the app sees WEBRTCME_FILTER. See Platforms/iOS/Program.cs.
    $filterExport = if ($Filter) { "export DEVICECTL_CHILD_WEBRTCME_FILTER=$Filter" } else { '' }

    # --console keeps the app attached so its stdout comes back. It also terminates the app when the
    # session ends, which is wanted here and is a trap elsewhere: a detached launch reports "exit
    # code 0" on a perfectly healthy app.
    $script = @'
__FILTER_EXPORT__
LOG=$(mktemp)
xcrun devicectl device process launch --device __DEVICE__ --console --terminate-existing __APPID__ > "$LOG" 2>&1 &
LAUNCHPID=$!
for i in $(seq 1 60); do
  grep -q WEBRTCME-SUMMARY "$LOG" && break
  sleep 2
done
kill $LAUNCHPID 2>/dev/null
grep WEBRTCME- "$LOG"
rm -f "$LOG"
'@

    $remote = $script.Replace('__FILTER_EXPORT__', $filterExport).
                      Replace('__DEVICE__', $DeviceId).
                      Replace('__APPID__', $appId).
                      Replace("`r", '')

    $log = ssh -o BatchMode=yes $Mac $remote 2>&1
    return Read-Outcome -Lines $log -What 'ios'
}

# ---------------------------------------------------------------- build and install

if (-not $SkipBuild) {
    Write-Host "Building and installing the runner..."
    switch ($Platform) {
        'android' {
            & dotnet build $runner -f net10.0-android -c Debug -t:Install `
                "-p:WebRTCmePackageVersion=$Version" "-p:RestoreAdditionalProjectSources=$PackageSource" --nologo |
                Select-String -Pattern 'error|Build succeeded' | Select-Object -First 5
            if ($LASTEXITCODE -ne 0) { throw "android build/install failed" }
        }
        'maccatalyst' {
            # Built over plain SSH. No codesigning ceremony for this one: a Catalyst app run locally
            # from its build output needs no signing identity, so the Terminal.app osascript dance a
            # device deploy needs (see doc/KnownGaps.md) does not apply here.
            #
            # The Mac restores from its OWN copy of the artifact, not this machine's, and evicts the
            # cached copy first for the reason given at the top of this script.
            $remote = "cd ~/Projects/WebRTCme && git pull --ff-only >/dev/null 2>&1; " +
                      "rm -rf ~/.nuget/packages/webrtcme/$Version && " +
                      "dotnet build Tests/WebRTCme.DeviceTests.Runner/WebRTCme.DeviceTests.Runner.csproj " +
                      "-f net10.0-maccatalyst -c Debug -p:WebRTCmePackageVersion=$Version " +
                      "-p:RestoreAdditionalProjectSources=`$HOME/Projects/WebRTCme/artifacts --nologo"

            $built = ssh -o BatchMode=yes $Mac $remote 2>&1
            if ($LASTEXITCODE -ne 0) {
                $built | Select-String -Pattern 'error' | Select-Object -First 5 | ForEach-Object { Write-Host $_ }
                throw "maccatalyst build failed on $Mac. Is the artifact in ~/Projects/WebRTCme/artifacts there?"
            }
            $built | Select-String -Pattern 'Build succeeded' | Select-Object -First 1 | ForEach-Object { Write-Host $_ }
        }
        'ios' {
            # Driven through Terminal.app, and that is the whole reason this case exists rather than
            # a plain ssh build. Codesigning from an SSH session fails with
            #
            #   /usr/bin/codesign exited with code 1: ... WebRTC.framework: errSecInternalComponent
            #
            # because an SSH session's keychain is not the GUI session's and cannot reach the signing
            # identity. osascript asks the logged-in desktop to run the build instead, which can.
            # A completion file carries the exit code back, because osascript returns as soon as
            # Terminal has accepted the command and says nothing about how it ended.
            $buildScript = @'
#!/bin/zsh
cd ~/Projects/WebRTCme
rm -f /tmp/webrtcme-ios-build.done
git pull --ff-only > /dev/null 2>&1
rm -rf ~/.nuget/packages/webrtcme/__VERSION__
dotnet build Tests/WebRTCme.DeviceTests.Runner/WebRTCme.DeviceTests.Runner.csproj -f net10.0-ios -c Debug -p:RuntimeIdentifier=ios-arm64 -p:WebRTCmePackageVersion=__VERSION__ -p:RestoreAdditionalProjectSources=$HOME/Projects/WebRTCme/artifacts --nologo > /tmp/webrtcme-ios-build.log 2>&1
echo $? > /tmp/webrtcme-ios-build.done
'@

            $buildScript = $buildScript.Replace('__VERSION__', $Version).Replace("`r", '')

            # Heredoc with a quoted delimiter, so the remote shell expands nothing on the way in.
            $write = "cat > /tmp/webrtcme-ios-build.sh <<'WEBRTCME_EOF'`n$buildScript`nWEBRTCME_EOF`nchmod +x /tmp/webrtcme-ios-build.sh"
            ssh -o BatchMode=yes $Mac $write.Replace("`r", '') | Out-Null

            Write-Host "  building on the Mac desktop (codesigning needs the GUI session's keychain)..."
            # The launcher goes into a file as well. It contains double quotes for osascript, and a
            # PowerShell double-quoted string has no \" escape - only the backtick - so building this
            # inline ends the string at the first quote osascript needs.
            $runScript = @'
#!/bin/zsh
rm -f /tmp/webrtcme-ios-build.done
osascript -e 'tell application "Terminal" to do script "/tmp/webrtcme-ios-build.sh"' > /dev/null 2>&1
for i in $(seq 1 120); do
  [ -f /tmp/webrtcme-ios-build.done ] && break
  sleep 5
done
cat /tmp/webrtcme-ios-build.done 2>/dev/null || echo TIMEOUT
'@

            $writeRun = "cat > /tmp/webrtcme-ios-run.sh <<'WEBRTCME_EOF'`n$($runScript.Replace("`r", ''))`nWEBRTCME_EOF`nchmod +x /tmp/webrtcme-ios-run.sh"
            ssh -o BatchMode=yes $Mac $writeRun.Replace("`r", '') | Out-Null

            $code = (ssh -o BatchMode=yes $Mac '/tmp/webrtcme-ios-run.sh' 2>&1 | Select-Object -Last 1).ToString().Trim()

            if ($code -ne '0') {
                ssh -o BatchMode=yes $Mac 'grep -E "error" /tmp/webrtcme-ios-build.log | head -5' 2>&1 |
                    ForEach-Object { Write-Host "  $_" }
                throw "ios build failed on $Mac (exit $code). Is the Mac logged in at the desktop?"
            }

            Write-Host "  installing on $DeviceId..."
            $app = '$HOME/Projects/WebRTCme/Tests/WebRTCme.DeviceTests.Runner/bin/Debug/net10.0-ios/ios-arm64/WebRTCme.DeviceTests.Runner.app'
            $install = "xcrun devicectl device install app --device $DeviceId `"$app`""
            $installed = ssh -o BatchMode=yes $Mac $install.Replace("`r", '') 2>&1
            if (-not ($installed | Where-Object { $_ -match 'App installed' })) {
                $installed | Select-Object -Last 5 | ForEach-Object { Write-Host "  $_" }
                throw "ios install failed. Is the phone unlocked and trusted?"
            }
        }
        default {
            throw "Building for $Platform happens on the Mac. Build it there first and re-run with -SkipBuild:`n" +
                  "  ssh $Mac 'cd ~/Projects/WebRTCme && dotnet build Tests/WebRTCme.DeviceTests.Runner/WebRTCme.DeviceTests.Runner.csproj -f net10.0-$Platform -c Debug -p:WebRTCmePackageVersion=$Version -p:RestoreAdditionalProjectSources=<mac artifacts path>'`n" +
                  "Codesigning needs the build driven through Terminal.app via osascript - an SSH session's keychain is not the GUI session's. See doc/KnownGaps.md."
        }
    }
    Write-Host ""
}

# ---------------------------------------------------------------- run

Write-Host "All scenarios:"
$allOk = switch ($Platform) {
    'android'     { Invoke-Android }
    'maccatalyst' { Invoke-MacCatalyst }
    'ios'         { Invoke-Ios }
}

Write-Host ""
Write-Host "$isolated, in a process where no call has happened:"
$isolatedOk = switch ($Platform) {
    'android'     { Invoke-Android -Filter $isolated }
    'maccatalyst' { Invoke-MacCatalyst -Filter $isolated }
    'ios'         { Invoke-Ios -Filter $isolated }
}

Write-Host ""
if (-not ($allOk -and $isolatedOk)) {
    Write-Host "Device test FAILED for $Version on $Platform." -ForegroundColor Red
    Write-Host "A missing native payload reads as DllNotFoundException on the first call;" -ForegroundColor Red
    Write-Host "one built for the wrong architecture reads as BadImageFormatException." -ForegroundColor Red
    exit 1
}

Write-Host "Device test passed: $Version negotiates end to end on $Platform." -ForegroundColor Green
