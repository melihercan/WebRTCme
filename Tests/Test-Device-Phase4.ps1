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
    [switch] $Local,
    [switch] $Simulator,
    [string] $SimulatorName,
    [switch] $SkipBuild
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repoRoot  = Split-Path -Parent $PSScriptRoot
$runner    = Join-Path $PSScriptRoot 'WebRTCme.DeviceTests.Runner/WebRTCme.DeviceTests.Runner.csproj'
$appId     = 'com.melihercan.webrtcme.devicetests'

# The scenario that has to run on its own, and why - see the description above.
$isolated  = 'ACompletedCallDoesNotChangeWhatDevicesExist'

# Set by Read-Outcome when a run reported a summary, so the failure hint can tell "a check failed"
# from "it stopped".
$sawSummary = $false

# Where the Apple work happens. -Local means this machine is the Mac, which is how CI runs it on a
# macOS runner; otherwise it is the Mac mini over SSH and the repository is at a known path there.
$macRepo = if ($Local) { $repoRoot } else { '$HOME/Projects/WebRTCme' }

# Runs a shell script on the Mac - here or over SSH.
#
# Carriage returns are stripped every time, and that is not cosmetic: this file has CRLF line
# endings, so a here-string carries them into the script, and zsh treats the CR as part of the
# command. Every line fails with "command not found: do^M" while the exit status and the WEBRTCME-
# grep both come back empty - reported as "the runner did not finish", which is indistinguishable
# from an app that crashed on launch.
function Invoke-Shell {
    param([string] $Script)

    $clean = $Script.Replace("`r", '')

    if (-not $Local) { return ssh -o BatchMode=yes $Mac $clean 2>&1 }

    $file = Join-Path ([System.IO.Path]::GetTempPath()) "webrtcme-$([guid]::NewGuid().ToString('N')).sh"
    Set-Content -Path $file -Value $clean -NoNewline
    try { & /bin/zsh $file 2>&1 } finally { Remove-Item $file -Force -ErrorAction SilentlyContinue }
}

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
# $HOME rather than $env:USERPROFILE, which is empty on macOS and Linux - where this would have
# silently resolved to the filesystem root and evicted nothing.
$cached = Join-Path $HOME ".nuget/packages/webrtcme/$Version"
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

    if ($summary) { $script:sawSummary = $true }

    if (-not $summary) {
        Write-Host "  no summary line - the runner did not finish ($What)" -ForegroundColor Red

        # Name the scenario that did not come back. The runner announces each one before running
        # it, so the last BEGIN without a matching result is the one that blocked or crashed - and
        # that is the single most useful fact about a run that produced nothing else.
        $begun = @($Lines | Where-Object { $_ -match 'WEBRTCME-BEGIN' } |
                   ForEach-Object { ($_ -split '\|')[-1].Trim() })
        $done  = @($scenarios | ForEach-Object { ($_ -split '\|')[3].Trim() })
        $stuck = $begun | Where-Object { $_ -notin $done } | Select-Object -Last 1

        if ($stuck) {
            Write-Host "  last scenario begun and never finished: $stuck" -ForegroundColor Red
        } elseif (-not $begun) {
            Write-Host "  no scenario ever began - the app did not start, or its output never arrived" -ForegroundColor Red
        }

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

    # Anything that looks like a death, before the app is stopped.
    #
    # The WEBRTCME- filter above is deliberately narrow, and that narrowness hid something: a run
    # that prints BEGIN for a scenario and then nothing looks identical whether the scenario blocked
    # or the process died. A block would have been caught by the scenario timeout and reported; a
    # crash leaves exactly this silence. Only logcat knows which, and only if asked.
    $died = & $adb logcat -d 2>$null |
            # Wide, because libwebrtc writes its RTC_CHECK text under its own tag - libjingle, rtc,
            # WebRTC, jingle depending on the build - and the previous filter kept the backtrace but
            # lost the one line that says which assertion failed.
            Where-Object { $_ -match 'FATAL|AndroidRuntime|SIGSEGV|SIGABRT|tombstone|UnsatisfiedLink|dlopen failed|Abort message|F DEBUG|#0[0-9] pc |libjingle|libwebrtc|rtc|WebRTC|CHECK|DCHECK|FATAL_ERROR' } |
            Select-Object -Last 40

    & $adb shell am force-stop $appId | Out-Null

    $ok = Read-Outcome -Lines $log -What 'android'

    if (-not $ok -and $died) {
        Write-Host ""
        Write-Host "  what logcat says about the process dying:" -ForegroundColor Yellow
        $died | ForEach-Object { Write-Host "    $_" -ForegroundColor DarkYellow }
    }

    return $ok
}

# ---------------------------------------------------------------- mac catalyst

function Invoke-MacCatalyst {
    param([string] $Filter)

    # The runtime identifier is found rather than named. macos-latest is Apple Silicon and builds
    # maccatalyst-arm64; an Intel Mac builds maccatalyst-x64. Hardcoding either means the path does
    # not exist on the other, the app never starts, and this reports "the runner did not finish" -
    # which reads exactly like a crash on launch.
    #
    # The bundle is also named from ApplicationTitle rather than the assembly - "WebRTCme Device
    # Tests.app" - so the path contains spaces and stays quoted throughout.
    $app = '$(find "__REPO__/Tests/WebRTCme.DeviceTests.Runner/bin/Debug/net10.0-maccatalyst" -name "WebRTCme.DeviceTests.Runner" -type f -perm +111 | head -1)'
    $app = $app.Replace('__REPO__', $macRepo)
    $filterArg = if ($Filter) { "--filter=$Filter" } else { '' }

    # A literal here-string with the variable parts substituted afterwards: every $ below belongs to
    # the remote shell, and escaping each one by hand is how the first version of this silently sent
    # a script zsh could not run.
    #
    # The executable inside the bundle is run directly so stdout comes back; `open` would detach it
    # and send its output to the system log. No window server session is needed.
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

    $log = Invoke-Shell ($script.Replace('__APP__', $app).Replace('__FILTER__', $filterArg))
    return Read-Outcome -Lines $log -What 'maccatalyst'
}

# ---------------------------------------------------------------- ios

function Invoke-Ios {
    param([string] $Filter)

    if ($Simulator) { return Invoke-IosSimulator -Filter $Filter }

    if (-not $DeviceId) {
        throw "iOS needs -DeviceId (or WEBRTCME_IOS_DEVICE): the paired identifier from " +
              "''xcrun devicectl list devices''. Several iPhones can be paired and reported " +
              "''available'' whether or not they are plugged in, so this is not guessed."
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

    $log = Invoke-Shell ($script.Replace('__FILTER_EXPORT__', $filterExport).
                                Replace('__DEVICE__', $DeviceId).
                                Replace('__APPID__', $appId))
    return Read-Outcome -Lines $log -What 'ios'
}

# ---------------------------------------------------------------- ios simulator
#
# What CI runs, because a hosted runner has no phone. It is a weaker test than the device and is
# not a substitute for it: the Simulator loads the *simulator* slice of WebRTC.xcframework, while
# ios-arm64 is the slice that ships. It also has no capture devices, so the enumeration scenario
# skips where the device passes - which is why WEBRTCME_TESTS_REQUIRED must not treat a skip here as
# a failure, and why a green run must not be read as device verification.
#
# It is still worth running. Both faults found on 2026-09-15 were managed-code faults - a null
# dereference and a serialisation shape - and this would have caught either.

function Invoke-IosSimulator {
    param([string] $Filter)

    $filterExport = if ($Filter) { "export SIMCTL_CHILD_WEBRTCME_FILTER=$Filter" } else { '' }

    # simctl forwards SIMCTL_CHILD_-prefixed variables into the app, the same trick devicectl needs
    # and for the same reason.
    $script = @'
__FILTER_EXPORT__
LOG=$(mktemp)
xcrun simctl launch --console-pty "__SIM__" __APPID__ > "$LOG" 2>&1 &
LAUNCHPID=$!
for i in $(seq 1 60); do
  grep -q WEBRTCME-SUMMARY "$LOG" && break
  sleep 2
done
kill $LAUNCHPID 2>/dev/null
xcrun simctl terminate "__SIM__" __APPID__ > /dev/null 2>&1
grep WEBRTCME- "$LOG"
rm -f "$LOG"
'@

    $log = Invoke-Shell ($script.Replace('__FILTER_EXPORT__', $filterExport).
                                Replace('__SIM__', $ResolvedSimulator).
                                Replace('__APPID__', $appId))
    return Read-Outcome -Lines $log -What 'ios simulator'
}

# ---------------------------------------------------------------- build and install

# ---------------------------------------------------------------- simulator selection

# Which simulator, when -Simulator is given: named explicitly, or the first available iPhone. The
# runner image decides what exists, and pinning a model here would break the day GitHub changes
# the image.
$ResolvedSimulator = $null
if ($Simulator) {
    $ResolvedSimulator = if ($SimulatorName) { $SimulatorName } else {
        $found = Invoke-Shell 'xcrun simctl list devices available | grep -E "^ +iPhone" | head -1 | sed -E "s/.*\(([0-9A-Fa-f-]{36})\).*/\1/"'
        ($found | Select-Object -Last 1).ToString().Trim()
    }

    if (-not $ResolvedSimulator) {
        throw "no iOS simulator available on $(if ($Local) { 'this machine' } else { $Mac })"
    }
    Write-Host "Simulator      : $ResolvedSimulator"
    Write-Host ""
}

if (-not $SkipBuild) {
    Write-Host "Building and installing the runner..."

    # The Mac restores from its own copy of the artifact, and evicts the cached package first for
    # the reason given at the top of this script.
    $macFeed = if ($Local) { "$macRepo/artifacts" } else { '$HOME/Projects/WebRTCme/artifacts' }
    $macPull = if ($Local) { '' } else { 'git pull --ff-only > /dev/null 2>&1' }

    switch ($Platform) {
        'android' {
            # An emulator is just another adb device, so this one path serves CI and a real phone.
            & dotnet build $runner -f net10.0-android -c Debug -t:Install `
                "-p:WebRTCmePackageVersion=$Version" "-p:RestoreAdditionalProjectSources=$PackageSource" --nologo |
                Select-String -Pattern 'error|Build succeeded' | Select-Object -First 5
            if ($LASTEXITCODE -ne 0) { throw "android build/install failed" }
        }

        'maccatalyst' {
            # No codesigning ceremony. A Catalyst app run locally from its build output needs no
            # signing identity, so the Terminal.app route an iOS device build needs does not apply
            # here, and neither does a window server session.
            $buildCatalyst = @'
set -e
cd "__REPO__"
__PULL__
rm -rf ~/.nuget/packages/webrtcme/__VERSION__
dotnet build Tests/WebRTCme.DeviceTests.Runner/WebRTCme.DeviceTests.Runner.csproj -f net10.0-maccatalyst -c Debug -p:WebRTCmePackageVersion=__VERSION__ -p:RestoreAdditionalProjectSources=__FEED__ --nologo
'@

            $built = Invoke-Shell ($buildCatalyst.Replace('__REPO__', $macRepo).
                                                 Replace('__PULL__', $macPull).
                                                 Replace('__VERSION__', $Version).
                                                 Replace('__FEED__', $macFeed))

            if (-not ($built | Where-Object { $_ -match 'Build succeeded' })) {
                $built | Select-String -Pattern 'error' | Select-Object -First 5 |
                    ForEach-Object { Write-Host "  $_" }
                throw "maccatalyst build failed. Is the artifact in $macFeed ?"
            }
            Write-Host "  build succeeded"
        }

        'ios' {
            if ($Simulator) {
                # No RuntimeIdentifier and no signing identity: a simulator build needs neither,
                # which is the whole reason CI can run this and cannot run the device.
                $buildSim = @'
set -e
cd "__REPO__"
__PULL__
rm -rf ~/.nuget/packages/webrtcme/__VERSION__
dotnet build Tests/WebRTCme.DeviceTests.Runner/WebRTCme.DeviceTests.Runner.csproj -f net10.0-ios -c Debug -p:WebRTCmePackageVersion=__VERSION__ -p:RestoreAdditionalProjectSources=__FEED__ --nologo
xcrun simctl bootstatus "__SIM__" -b
xcrun simctl uninstall "__SIM__" __APPID__ > /dev/null 2>&1 || true
xcrun simctl install "__SIM__" "$(dirname "$(find "__REPO__/Tests/WebRTCme.DeviceTests.Runner/bin/Debug/net10.0-ios" -name "WebRTCme.DeviceTests.Runner.app" -maxdepth 2 | head -1)")/WebRTCme.DeviceTests.Runner.app"
echo SIMULATOR-READY
'@

                $built = Invoke-Shell ($buildSim.Replace('__REPO__', $macRepo).
                                                Replace('__PULL__', $macPull).
                                                Replace('__VERSION__', $Version).
                                                Replace('__FEED__', $macFeed).
                                                Replace('__SIM__', $ResolvedSimulator).
                                                Replace('__APPID__', $appId))

                if (-not ($built | Where-Object { $_ -match 'SIMULATOR-READY' })) {
                    $built | Select-Object -Last 8 | ForEach-Object { Write-Host "  $_" }
                    throw "ios simulator build/install failed"
                }
                Write-Host "  installed on the simulator"
                break
            }

            # A device build. Codesigning from an SSH session fails with
            #
            #   /usr/bin/codesign exited with code 1: ... WebRTC.framework: errSecInternalComponent
            #
            # because an SSH session's keychain is not the GUI session's and cannot reach the
            # signing identity. osascript asks the logged-in desktop to run the build instead,
            # which can. A completion file carries the exit code back, because osascript returns as
            # soon as Terminal has accepted the command and says nothing about how it ended.
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

            # Both go into files on the Mac. They contain the double quotes osascript needs, and a
            # PowerShell double-quoted string has no backslash escape - only the backtick - so
            # building this inline ends the string at the first of them.
            $eof = "WEBRTCME_EOF"
            Invoke-Shell "cat > /tmp/webrtcme-ios-build.sh <<'$eof'`n$buildScript`n$eof`nchmod +x /tmp/webrtcme-ios-build.sh" | Out-Null
            Invoke-Shell "cat > /tmp/webrtcme-ios-run.sh <<'$eof'`n$($runScript.Replace("`r", ''))`n$eof`nchmod +x /tmp/webrtcme-ios-run.sh" | Out-Null

            Write-Host "  building on the Mac desktop (codesigning needs the GUI session's keychain)..."
            $code = (Invoke-Shell '/tmp/webrtcme-ios-run.sh' | Select-Object -Last 1).ToString().Trim()

            if ($code -ne '0') {
                Invoke-Shell 'grep -E "error" /tmp/webrtcme-ios-build.log | head -5' |
                    ForEach-Object { Write-Host "  $_" }
                throw "ios build failed on $Mac (exit $code). Is the Mac logged in at the desktop?"
            }

            Write-Host "  installing on $DeviceId..."
            $app = '$HOME/Projects/WebRTCme/Tests/WebRTCme.DeviceTests.Runner/bin/Debug/net10.0-ios/ios-arm64/WebRTCme.DeviceTests.Runner.app'
            $installed = Invoke-Shell "xcrun devicectl device install app --device $DeviceId `"$app`""
            if (-not ($installed | Where-Object { $_ -match 'App installed' })) {
                $installed | Select-Object -Last 5 | ForEach-Object { Write-Host "  $_" }
                throw "ios install failed. Is the phone unlocked and trusted?"
            }
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

    # Two different hints, because one for every failure sends readers to the wrong place. A run
    # that reported nothing did not fail a check - it stopped - and a missing or mismatched native
    # library is not what that looks like: those throw, immediately and by name.
    if ($allOk -eq $false -and -not $sawSummary) {
        Write-Host "It stopped rather than failed. The line above names the scenario that began and" -ForegroundColor Red
        Write-Host "never came back, if one did; nothing at all means the app never started." -ForegroundColor Red
    } else {
        Write-Host "A missing native payload reads as DllNotFoundException on the first call;" -ForegroundColor Red
        Write-Host "one built for the wrong architecture reads as BadImageFormatException." -ForegroundColor Red
    }
    exit 1
}

Write-Host "Device test passed: $Version negotiates end to end on $Platform." -ForegroundColor Green
