# Testing the packages

A plan for automated testing of `WebRTCme` and `WebRTCme.Middleware`, agreed 2026-09-14. Written
before any test code existed, so the decisions are visible and can be argued with rather than
reverse-engineered from a diff later. Same intent as `DemoAppRefactor.md`.

> **All five phases are implemented.** This is kept as the record of what was decided and why -
> including the parts the plan got wrong, which are marked where they occur and are the most useful
> thing in it. For *how to run the suite*, read
> [Testing](https://github.com/melihercan/WebRTCme/wiki/Testing) in the wiki, which is the
> current, readable version.

The goal in one line: **prove the published NuGet packages work, on every platform, without a
person clicking anything.**

The emphasis is on *published packages*. This repository had no tests at all when this was written,
so there was a temptation to write whatever was easiest. What is easiest is testing the source tree,
and the source tree is not what breaks - `doc/Packaging.md` and `KnownGaps.md` both record the same
failure shape, repeatedly: the code was right, the *package* was wrong, and nothing noticed until a
device did.

## What the survey found

Five things shape the plan, and none was obvious from outside.

**There is a large hardware-free surface, and it is genuinely pure.** `Ortc` is a public class with
22 public methods - `GetExtendedRtpCapabilites`, `GetSendingRtpParameters`, `ReduceCodecs`,
`CanSend`, `CanReceive`, a dozen validators - all taking and returning plain models. `H264` has six
public statics including `GenerateProfileLevelIdForAnswer` and `ParseProfileLevelId`.
`ScalabilityModes.Parse` is a regex over a string. None of this needs a camera, a device, or a
peer. This is the mediasoup-client port, which is the largest body of borrowed logic in the
repository and the one most likely to drift from its JavaScript original.

**Some of it is behind `internal`.** `CommonUtils` is an internal static class, and
`ExtractDtlsParameters` is internal within it. Testing it means `InternalsVisibleTo`, the same way
DartSimPro grants it for `OperationsStateMachine`.

**The speaking detection is testable logic trapped in an untestable shape.** The threshold rule in
`SignalingConnection` - noise floor times `SpeakingLevelFactor`, clamped to `SpeakingLevelFloor`,
adapted by `NoiseFloorAdaption`, held open for `SpeakingHangover` - is a pure state machine. It
lives inside an `async` loop driven by `Task.Delay(SpeakingSampleInterval)` and `DateTime.UtcNow`,
with its constants private. It took three attempts and a comma-decimal culture bug to get right
(see `DemoAppRefactor.md`), which is exactly the code that deserves tests. It has to be extracted
to a pure class first, fed levels and timestamps.

**The demo apps reference projects, not packages.** `WebRTCme.DemoApp.Maui` has a `ProjectReference`
to `WebRTCme.Middleware.csproj` and to all four binding projects; the Blazor demo references the
middleware project. Anything built this way tests the repository and would pass with a `.nupkg`
that is missing `libwebrtc.aar` entirely. **A test that does not restore from the package does not
test the package.**

**XHarness is not on nuget.org.** The official device-test runner ships from the `dotnet-eng` Azure
DevOps feed. This repository has no `nuget.config`, no `Directory.Build.props` and no
`Directory.Packages.props`, so using it means introducing the first of those.

## The shape - three tiers

> As built it is **five**: the single "Runtime" tier below turned out to be three, because a phone,
> a Mac Catalyst app and a browser each need a different way in. See *What the plan got wrong* at
> the foot of this file. The three-tier shape is left here as it was decided.

Deliberately *not* the DartSimPro pyramid, and the difference is the point. DartSimPro's product is
an application, so its top tier is Appium driving a UI because there is no other way in.
**WebRTCme's product is an API, and an API can be called directly.** There is no UI to drive, so
there is no Appium.

```
        ┌────────────────────────────────┐
        │  Runtime   loopback, on-device │  ← does the package actually work,
        ├────────────────────────────────┤     per platform. PC + Mac. No UI.
        │  Consumption   restore+compile │  ← does the package restore and
        ├────────────────────────────────┤     compile for a consumer, per TFM.
        │  Unit + Integration            │  ← the logic. Any machine, no
        └────────────────────────────────┘     hardware, sub-second.
```

| tier | project | runs on | proves |
| --- | --- | --- | --- |
| Unit + Integration | `Tests/WebRTCme.Tests` | any machine, `net10.0` | The logic is right. Tests the *source*. |
| Consumption | `Tests/WebRTCme.PackageTests` | PC (net10.0, -android, -windows), Mac (-ios, -maccatalyst) | The package restores and compiles for a consumer, on every TFM. |
| Runtime | `Tests/WebRTCme.DeviceTests` | PC + Mac, headless | The native half loads and negotiates, per platform. |

Test stack throughout: **xUnit + NSubstitute + FluentAssertions**, per the standing convention.

> **Invoke the test projects directly, not with `dotnet test`.** Since the xunit.v3 4.0 /
> Microsoft.Testing.Platform change, `dotnet test` fails outright on the .NET 10 SDK - *"Testing
> with VSTest target is no longer supported"*. This machine is on 10.0.400, well past where that
> bites. Use `dotnet run --project Tests/WebRTCme.Tests`; `--filter "X"` becomes
> `-filterVSTest "X"` and `--logger trx` becomes `-result-trx`. Learned the hard way in DartSimPro;
> there is no reason to learn it twice.

## The central idea: loopback

A WebRTC smoke test does not need two machines, a signalling server, or a network. **Two
`RTCPeerConnection` objects in the same process**, with offer, answer and ICE candidates handed
between them directly in code:

```
pc1.CreateOffer  →  pc1.SetLocalDescription  →  pc2.SetRemoteDescription
pc2.CreateAnswer →  pc2.SetLocalDescription  →  pc1.SetRemoteDescription
OnIceCandidate on each  →  AddIceCandidate on the other
                    ↓
        DTLS completes, data channel opens, message round-trips
```

That one test proves, on whichever platform the process is running:

- the native half loaded at all - `libwebrtc.aar`, `WebRTC.xcframework`, the versioned
  `WebRTC.framework`, `WebRtcInterop.dll`;
- the binding marshals SDP in both directions;
- ICE candidates survive the round trip;
- DTLS and SCTP work.

A package with a missing or flat framework fails it immediately, which is the failure this
repository keeps shipping. It needs no camera, so it runs anywhere - including, if ever wanted, on
a CI runner.

**Media is a second, separate group.** `GetUserMedia` needs a real capture device, and that is
where nearly every bug in `KnownGaps.md` actually lived: rotation, hot-plug, backgrounding,
device-died-under-a-call. Those tests skip cleanly when no device is present rather than failing,
so the negotiation core stays runnable everywhere.

## Phases

Ordered so that each phase is useful on its own and needs nothing from the phases after it.

### Phase 1 - unit and integration, no rig at all - implemented

`Tests/WebRTCme.Tests`, plain `net10.0`, runs on any machine in seconds. Nothing here needs the
package, a device, or a second peer. This is where roughly 80% of the defect surface is, and it
could start immediately.

**114 tests, about two seconds.** Two things landed differently from the list below:
`Integration/DiGraphTests` is called `ServiceRegistrationTests`, and `CommonUtilsTests` was added
for the `InternalsVisibleTo` surface described above.

- `Unit/Ortc*` - the capability negotiation: `GetExtendedRtpCapabilites` against known local and
  remote capability pairs, `GetSendingRtpParameters`, `ReduceCodecs`, `CanSend` / `CanReceive`, and
  the validators' rejection cases. Fixtures can be captured from a real mediasoup handshake.
- `Unit/H264Tests` - `ParseProfileLevelId`, `ProfileLevelIdToString` round-trips,
  `GenerateProfileLevelIdForAnswer`, `IsSameProfile`. Table-driven from the WebRTC spec's own
  examples.
- `Unit/ScalabilityModesTests` - `Parse` for `L1T3`, `S3T3`, malformed input, empty string.
- `Unit/ModelExtensionsTests` - `ToStringOrNumber`, whose in-place dictionary mutation was a fixed
  bug (`0f338c74` era) and deserves a regression test.
- `Unit/SpeakingDetectorTests` - **requires an extraction first**: lift the threshold rule out of
  `SignalingConnection` into a pure class taking `(level, now)` and returning a speaking flag. Then
  test the noise floor adapting, the clamp at `SpeakingLevelFloor`, the hangover holding through a
  pause, and - explicitly - a comma-decimal culture, which is the fault that made one peer report
  speaking permanently.
- `Integration/DiGraphTests` - `AddMiddleware()` and `AddMediaSoup()` build a container that
  resolves. Cheap, and catches a registration dropped during refactoring.
- `Integration/SignalingServerTests` - `WebRTCme.Connection.Signaling.Server` is an ASP.NET Core
  SignalR app in this repository. `WebApplicationFactory` can host it in-process and drive the hub
  with a real SignalR client: join, peer-joined notification, leave, dropped connection. No
  browser, no device.

`CommonUtils` needs `InternalsVisibleTo("WebRTCme.Tests")` on `WebRTCme.Connection.MediaSoup`.

### Phase 2 - consumption

`Tests/WebRTCme.PackageTests`: a project per target framework whose only job is to restore the
packed `.nupkg` and compile a few lines against it.

- A `nuget.config` adding a local folder feed pointing at the CI artifact.
- `PackageReference` at the exact version under test, never a `ProjectReference`.
- One file per TFM that touches the surface a consumer touches: construct an `RTCPeerConnection`,
  resolve `IWebRtc` from `CrossWebRtc`, call `AddMiddleware()`.

This is what catches a slice that `Verify-Packages.ps1` says is present but that a consumer cannot
actually use - a wrong `lib/` folder name, a missing transitive dependency, an assembly that does
not load. Windows covers `net10.0`, `net10.0-android`, `net10.0-windows`; the Mac covers
`net10.0-ios` and `net10.0-maccatalyst`.

**This phase alone would have caught the packaging faults this repository has actually shipped.**

### Done 2026-09-14, and what it found

`Tests/WebRTCme.PackageTests` plus `Tests/Test-Package.ps1`, run against the packages from CI run
34834534644:

```powershell
gh run download 34834534644 -n nupkg -D artifacts
./Tests/Test-Package.ps1 -Version 26.9.9
```

Deliberately **not** in `WebRTCme.sln`: there is no package to restore until someone downloads a CI
artifact, and a test that breaks the ordinary build gets deleted rather than fixed. The feed is
passed as `RestoreAdditionalProjectSources` rather than written into a `nuget.config`, so the
source is a parameter of the run and nothing it does changes what an ordinary build restores.

**Compiling is not the assertion.** NuGet falls back: a `net10.0-android` project with no Android
slice resolves `lib/net10.0/` instead and compiles perfectly, because the common API is identical
across slices. What it would not have is any Android binding, and nothing says so until the app
runs on a phone and finds nothing behind the interface. So the script reads `project.assets.json`
back and requires every target framework to have resolved **its own** slice. All ten
package/framework pairs do.

**The finding, and it is a consumer's problem rather than a test's.** The first run failed:

```
error NU1605: Detected package downgrade: Microsoft.Maui.Controls from 10.0.80 to 10.0.20
  WebRTCme.PackageTests -> WebRTCme 26.9.9 -> Microsoft.Maui.Controls (>= 10.0.80)
  WebRTCme.PackageTests -> Microsoft.Maui.Controls (>= 10.0.20)
```

The packages depend on a `Microsoft.Maui.Controls` newer than the one the MAUI workload bundles
implicitly, and NuGet calls the difference a downgrade and fails the restore. So **every MAUI
consumer has to name `Microsoft.Maui.Controls` and `Microsoft.Maui.Controls.Compatibility`
themselves**, at the version the packages ask for, exactly as `WebRTCme.DemoApp.Maui` already does
and exactly as NU1605 instructs.

> **The version above is what it was on 2026-09-14, against 26.9.9. It is now 10.0.101.** The error
> is left as it was seen rather than rewritten, because the number is the least interesting part of
> it. What matters is that **it moves** - and when it does, the pin has to be raised in
> `Tests/WebRTCme.PackageTests` and in the wiki's `Getting started`, `Platform prerequisites` and
> `Releases` pages in the same change. The tier catches a stale pin with the same NU1605 in the
> opposite direction, which is the tier working rather than the tier being annoying.

It is not optional, and it is not discoverable until a restore fails. **This has to be in the
documentation before the packages are published.** *(It now is - see
[Platform prerequisites](https://github.com/melihercan/WebRTCme/wiki/Platform-prerequisites).)*
The alternative is to lower what the packages demand to whatever the current workload ships, which
is a decision about which MAUI servicing band to support rather than a bug to fix.

### Phase 3 - runtime, where a plain process works - implemented

`Tests/WebRTCme.DeviceTests`, referencing the package the same way Phase 2 does.

**Windows only, and that is a correction.** This phase was planned for Windows *and* Mac Catalyst,
on the assumption that a Catalyst test process runs the way a Windows one does. It does not, and
the wall is hard rather than a matter of configuration. Catalyst builds an `.app` rather than an
executable; xUnit v3 refuses to build a test project without an app host; and there is no app host
to be had:

```
error : xUnit.net v3 test projects must build an app host ('<UseAppHost>true</UseAppHost>')
error NETSDK1084: no application host available for the RuntimeIdentifier 'maccatalyst-x64'
```

Setting `UseAppHost` is what produces the second, so the two cannot both be satisfied. **Mac
Catalyst moves to phase 4** and needs the same device runner as Android and iOS.

Getting that far also cost two smaller mistakes worth recording, since both look like the tooling
misbehaving and are neither. A hardcoded `net10.0-windows` default fails on a Mac with `NETSDK1100`
about targeting Windows, which is a confusing thing to be told when you asked for Catalyst. And
passing `-f` or `-p:TargetFramework` does not fix it: the build honours them and the implicit
restore does not, so the build fails with `NETSDK1005` saying the assets file has no target for the
framework it was just given. The project picks its own framework for that reason.

Contents: the loopback negotiation above, plus `GetMediaDevices` enumeration, plus the media group
that skips without a camera - `GetUserMedia`, track add, `OnTrack` firing on the far side, mute
stopping the sender.

### Phase 4 - runtime, on device - implemented

Android, iOS and Mac Catalyst cannot run a plain test process - they build an .apk or an .app, and
xUnit v3 refuses to build a test project without an app host, of which there is none for a phone or
for maccatalyst-x64. So the scenarios run inside `Tests/WebRTCme.DeviceTests.Runner`, a MAUI app
that executes `WebRTCme.DeviceTests.Core` and prints one line per result, and
`Tests/Test-Device-Phase4.ps1` builds it, deploys it, runs it twice and reads the lines back.

**Not XHarness**, which the plan originally assumed. The newest build on the `dotnet-eng` feed is
from September 2023: it predates .NET 10, and it drives Apple devices through `mlaunch`, which on
this SDK hangs with "Please connect the device" on a locked phone and does not parse `:v2:udid=`
selectors. `adb` and `devicectl` do work, and the harness drives them directly - so the extra feed
the plan worried about is not needed either.

All three run from the PC, including the two Apple ones, over SSH to the Mac mini.

- **Android** - `adb`, against a real device. The filter arrives as an intent extra. The launcher
  activity is named explicitly in `[Activity(Name = ...)]` and resolved from the device with
  `cmd package resolve-activity`, rather than hardcoding the hash MAUI would otherwise generate.
- **iOS** - a real iPhone, not the Simulator. The Simulator works and is one command, but it runs
  the simulator slice of `WebRTC.xcframework`; the arm64 slice is the one that ships, so the device
  is what the harness targets. It also skips the device-enumeration scenario, correctly - a
  simulator has no capture devices - which is a pass fewer than the device gives.
- **Mac Catalyst** - built and run over plain SSH. No signing identity is needed to run a Catalyst
  app locally from its build output, and no window server session either.

Four things about the two Apple platforms are worth writing down, because each cost time and none
is discoverable from an error message:

1. **Codesigning for a device build must go through the desktop session.** From SSH it fails with
   `/usr/bin/codesign exited with code 1: ... WebRTC.framework: errSecInternalComponent`, because
   an SSH session's keychain is not the GUI session's. The harness uses `osascript` to ask
   `Terminal.app` to run the build, and a completion file to carry the exit code back - `osascript`
   returns as soon as Terminal accepts the command and says nothing about how it ended. Mac
   Catalyst needs none of this.
2. **devicectl's command-line arguments never reach the app.** Passed plainly or after a `--`
   separator, the runner still executed all five scenarios instead of the one asked for, silently.
   The filter travels as `DEVICECTL_CHILD_WEBRTCME_FILTER` instead, which devicectl does forward.
   This one matters rather than merely annoys: an unfiltered "isolated" run is the full run again,
   and the device-enumeration scenario is meaningless once a call has happened in that process.
3. **macOS has no `timeout`.** Its absence is silent - "command not found" leaves the exit status to
   whatever came next in the pipeline, so the app appears to have run and reported nothing.
4. **A harness authored on Windows sends CRLF to zsh.** The remote script fails on every line with
   `command not found: do^M`, the grep comes back empty, and the harness reports "the runner did not
   finish" - indistinguishable from an app that crashed on launch. Every remote script has its
   carriage returns stripped before it is sent.

The last three were all found by running the thing. None of them produced an error that named the
cause, and all three fail in the same direction: a green-looking or empty result rather than a loud
one.

### Phase 5 - Blazor - implemented

The odd one out: the Blazor binding is JSInterop, so it only exists inside a browser. A Blazor
WebAssembly test host driven by headless Chrome covers it. Chrome's
`--use-fake-device-for-media-stream` supplies synthetic media, so Blazor is the one platform where
the media group needs no hardware at all - and the one tier that can run while the Windows app is
holding this machine's only camera.

Three pieces, the same division as phase 4:

- `Tests/WebRTCme.BlazorTestHost` - a Blazor WebAssembly app that runs the shared scenarios from
  `WebRTCme.DeviceTests.Core` and writes each result into the DOM. No root component and no Razor;
  the page is one `<pre>` element. The filter arrives as `?filter=`, the query-string equivalent of
  the Android runner's intent extra.
- `Tests/WebRTCme.BlazorTests` - Playwright driving headless Chromium, asserting on what the page
  reported. It waits on the results element's `data-state`, so a page that never finished fails
  rather than reading as zero failures.
- `Tests/Test-Blazor.ps1` - builds the host against the package under test, serves it, waits for it
  to answer, runs the tests against that URL.

Sharing `DeviceTests.Core` meant giving it a `net10.0` target - the package's Blazor slice - and a
`LoopbackScenarios.JsRuntime` hook, because Blazor is the one binding that needs an `IJSRuntime`
and `IWebRtc.Window` takes it as an optional argument for exactly that reason.

**What it found on its first run:** a Blazor consumer must reconfigure JSInterop's JSON serialiser
by reflection or no peer connection can be created at all. See `doc/KnownGaps.md`.

## Configuration - one env var set, no machine-specific anything

Every host-specific value comes from an environment variable with a sensible default, so anyone
with a PC and a Mac can run the suite by setting a handful of values and nothing else. Same
discipline as DartSimPro's `UiTestBuild.Configuration`: **the choice is stated, never inferred from
a file timestamp.**

| variable | default | what it is |
| --- | --- | --- |
| `WEBRTCME_PACKAGE_SOURCE` | `./artifacts` | Folder feed holding the `.nupkg` under test. |
| `WEBRTCME_PACKAGE_VERSION` | the csproj `<Version>` | Which version to restore. |
| `WEBRTCME_TESTS_REQUIRED` | unset | When `1`, a missing device or runner fails instead of skipping - what CI or a release check sets. |
| `WEBRTCME_TEST_ARTIFACTS` | `TestResults/` | Where logs and captured SDP go on failure. |
| `WEBRTCME_ANDROID_UDID` | first attached | Which Android device. |
| `WEBRTCME_IOS_DEVICE` | `iPhone` | Which Simulator. |

Note what is **absent**: no Appium URL, no per-host IP address, no signalling-server address. The
loopback design means each host runs its own suite against itself, so the two machines never have
to find each other. Driving both from one command is SSH to the Mac - orchestration convenience,
added later if wanted, not architecture.

## Where the builds come from

The tests consume **CI-built packages**, not packages built on the test machine. `ci.yml` already
produces exactly the right artifact: a merged, verified, publishable `.nupkg` with macOS-built
Apple slices, uploaded as `nupkg`. The local flow is:

```
gh run download <run-id> -n nupkg -D artifacts
WEBRTCME_PACKAGE_VERSION=26.9.14 dotnet run --project Tests/WebRTCme.PackageTests
```

The reason is the same one that makes the Windows/Mac split necessary in the first place: a package
built on the test machine has that machine's Apple slices, and on Windows those are flat and
unsignable. **Testing a locally built package would test something that will never be published.**

## What stays manual

Honest about the gap rather than pretending the suite closes it.

`WebRTCme.Middleware` ships the **video rendering control** - `Blazor/Media.razor` and MAUI
`Media.cs` with a handler per platform. That is UI, and it only exists as UI. A headless test can
prove frames *arrive*; it cannot prove they are *drawn*, the right way up, at the right size. That
is precisely the frame-rotation bug class that took four commits across three platforms in
September 2026.

So: a short manual checklist survives for rendering, and the demo apps remain how it is checked.
If that proves too weak later, a single Appium test per platform that launches a demo app and
screenshots one tile would cover it - but that is a deliberate later decision, not part of this
plan.

Also staying manual: real multi-party SFU behaviour against mediasoup, TURN relay paths, and
anything needing two physically separate networks.

## Open decisions - all three settled

Kept with their answers, because two of them were decided by discovering the question was wrong
rather than by weighing the alternatives.

1. **Does Phase 1 wait for the `SpeakingDetector` extraction?** **No - it was done first.** The
   threshold rule was lifted out of `SignalingConnection`'s `async` loop into a pure class taking
   `(level, now)`, and `Unit/SpeakingDetectorTests` tests it, comma-decimal culture included. A
   production-code change in service of testability, made explicitly rather than smuggled into a
   diff.
2. **XHarness or a hand-rolled runner app** for Phase 4? **Hand-rolled, and the trade turned out not
   to exist.** The newest XHarness on the `dotnet-eng` feed is from September 2023: it predates
   .NET 10, and it drives Apple devices through `mlaunch`, which hangs on a locked phone and does
   not parse `:v2:udid=` selectors. `adb` and `devicectl` work and the harness drives them directly
   - so the extra NuGet feed this worried about is not needed either.
3. **Is `net10.0-android` consumption testable on the PC without a device?** **Yes, and the answer
   below was too generous.** Restore and compile, yes - but compiling is not the assertion, because
   NuGet falls back to `lib/net10.0/` and compiles perfectly with no Android binding present. The
   tier reads `project.assets.json` back and requires each target framework to have resolved its own
   slice. Anything beyond that is Phase 4.

## What the plan got wrong

Collected, because it is the most useful thing here and it is otherwise scattered across the
phases above.

- **Mac Catalyst cannot run Phase 3.** The plan assumed a Catalyst test process runs the way a
  Windows one does. It builds an `.app` rather than an executable, xUnit v3 refuses to build a test
  project without an app host, and there is no app host for `maccatalyst-x64`. It moved to Phase 4.
- **XHarness was the wrong tool**, as above.
- **"A Mac" was treated as a blocker throughout.** It is not: Mac Catalyst and iOS are both built,
  launched and driven from the PC over SSH. Only taps on a physical iPhone need a person.
- **Three tiers became five.** Phases 4 and 5 did not exist in the original shape; the diagram near
  the top still shows three.
