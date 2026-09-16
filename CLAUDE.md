# WebRTCme

Cross-platform WebRTC framework for .NET. It maps each platform's native WebRTC SDK (browser
JS API via Blazor JSInterop, Android Java SDK, Apple ObjC SDK, Windows via a C ABI over
libwebrtc) to a single common .NET API, so MAUI/Blazor apps can use one WebRTC surface across
Windows, macOS (Mac Catalyst), Android, iOS, and web. The API's interfaces/models/enums mirror the
[W3C WebRTC 1.0 API](https://w3c.github.io/webrtc-pc/) (the same spec browsers implement), so the
C# surface should feel familiar to anyone who's used the browser WebRTC JS API (see the
[project wiki](https://github.com/melihercan/WebRTCme/wiki)).

On **.NET 10** throughout, on `master` (net10.0 / net10.0-android / net10.0-ios /
net10.0-maccatalyst / net10.0-windows10.0.22621.0). The migration from .NET 8 landed via
`feature/dotnet10_support`; the 2.0.0 line is the .NET 8 line and is frozen, with no back-port.

**The wiki is the user-facing documentation** and is current:
<https://github.com/melihercan/WebRTCme/wiki>. Prefer updating it over growing the READMEs, which
deliberately stay short and link to it. `What works where` is the per-platform feature matrix and
`Releases` holds the release notes and upgrade steps.

## Layered architecture (bottom to top)

1. **Bindings** (`WebRTCme.Bindings/`) — raw per-platform WebRTC access, no common API. The only
   layer still split across several assemblies, because each binds a different native SDK:
   - `WebRTCme.Bindings.Blazor` — JSInterop wrapper over the browser WebRTC API.
   - `Maui/WebRTCme.Bindings.Maui.{Android,iOS,MacCatalyst}` — Java and ObjC bindings. Their
     native halves are committed: `Jars/libwebrtc.aar`, `WebRTC.xcframework`, `WebRTC.framework`.
   - `Maui/WebRTCme.Bindings.Maui.Windows` — not a binding project at all: `WebRtcInterop.dll`
     exposes a flat C ABI, so it is all P/Invoke over DLLs in `native/win-x64/`.
2. **WebRTCme** (`WebRTCme/WebRTCme.csproj`) — the API *and* the cross-platform plug-in library
   (`IWebRtc` / `CrossWebRtc`). `Api/` holds the common surface (interfaces/models/enums, e.g.
   `IMediaDevices`, `IMediaStream`, `RTCPeerConnection`) every binding implements against; it was
   a separate `WebRTCme.Api` project until it was compiled in. A bait-and-switch multi-target
   trick does the rest: per-`TargetFramework` conditional `<Compile Include>` picks the right
   `Platforms/{Blazor,Android,iOS,Windows,MacCatalyst}` folder. `wwwroot/JsInterop.js` lives here
   and ships as `_content/WebRTCme/JsInterop.js`.
3. **WebRTCme.Middleware** — services layer above the plug-in: media rendering (Blazor component
   / MAUI handler per platform), media stream service, connection handling and shared call/chat
   view models. One multi-targeted project, formerly three: `Core/` on every TFM, plus `Blazor/`
   on net10.0 and `Maui/` on the platform TFMs, whose per-platform code sits in
   `Maui/Platforms/{Android,iOS,MacCatalyst,Windows}/`.
4. **WebRTCme.Connection** — signaling and media-server integration:
   - `Signaling/` — client proxy plus a standalone SignalR signaling server (mesh/P2P).
   - `MediaSoup/` — a C# port of mediasoup-client v3 for SFU group calls, verified on Blazor,
     Android and iOS; Mac Catalyst and Windows compile against it but have never run it. The
     server is *not* here: it is versatica's mediasoup-demo, pinned to a commit in
     `MediaSoup/server/Dockerfile` and deployed separately.
5. **WebRTCme.DemoApp** — sample apps consuming the stack: `WebRTCme.DemoApp.Blazor` and
   `WebRTCme.DemoApp.Maui`.

Only two of these are published, as two NuGet packages — see `doc/Packaging.md`. Everything else
is folded into them.

Xamarin is gone from the repo entirely; `README_V1.md` is the only record of that era.

## NuGet packaging

Two public packages: `WebRTCme` (API, plug-in, bindings, native payloads) and
`WebRTCme.Middleware` (middleware + connection layer), which depends on it. Versions are the build
date, written normalised - `26.9.9`, not `26.09.09`, because NuGet strips leading zeros and every
place that builds a file name from `<Version>` then stops matching.

`doc/Packaging.md` is the reference, and worth reading before changing anything here: pack drops
payloads it does not recognise *silently*, and `PrivateAssets="all"` rather than `IsPackable` is
what keeps a folded project out of the dependency list.

## Build

Open `WebRTCme.sln` in Visual Studio, or:

```powershell
dotnet build WebRTCme.sln
```

Everything builds with plain `dotnet build` on Windows, including the iOS and Mac Catalyst
library slices - only linking and deploying an *app* for those needs a Mac. `windows-latest` is
the only runner that can build all five target frameworks in one job, since
`net10.0-windows10.0.22621.0` builds nowhere else.

**Packing is the exception, and it is two machines.** Mac Catalyst's `WebRTC.framework` has to be a
versioned bundle - `Versions/A` plus symlinks - or macOS refuses to codesign an app embedding it,
and Windows can neither check those symlinks out of git nor write them into a zip. So CI builds the
Apple slices on `macos-latest` and merges them into a package built on Windows;
`-RequireAppleNativeLayout` fails the build if they did not arrive. A package built entirely on
Windows restores and compiles fine and then fails on a device, which is how this shipped broken for
a long time.

## Testing

Five tiers, and the point of all of them is to test the **published package** rather than the
source tree - the source tree is not what breaks here. `doc/TestingPlan.md` is the design;
the wiki's `Testing` page is the readable version.

| tier | project | proves |
| --- | --- | --- |
| 1 | `Tests/WebRTCme.Tests` | the logic. 114 tests, ~2s, any machine |
| 2 | `Tests/WebRTCme.PackageTests` | the package restores and resolves *its own* slice per TFM |
| 3 | `Tests/WebRTCme.DeviceTests` | the native half loads and negotiates, on Windows |
| 4 | `Tests/WebRTCme.DeviceTests.Runner` | the same, on Android / iOS / Mac Catalyst |
| 5 | `Tests/WebRTCme.BlazorTestHost` + `.BlazorTests` | the same, in headless Chromium |

**`dotnet test` does not work on this SDK** - xunit.v3 4.0 moved to Microsoft.Testing.Platform and
`dotnet test` fails with *"Testing with VSTest target is no longer supported"*. Invoke the projects
directly instead: `dotnet run --project Tests/WebRTCme.Tests`. `--filter` becomes `-filterVSTest`,
`--logger trx` becomes `-result-trx`.

Tiers 2-5 need a CI-built `.nupkg`, not a locally built one, and are driven by the `Tests/Test-*.ps1`
scripts. They are deliberately outside `WebRTCme.sln`, because there is no package to restore until
someone downloads a CI artifact.

Rendering is **not** covered and is checked by hand against the demo apps: a headless test can prove
frames arrive, not that they are drawn the right way up.

## Known gaps

`doc/KnownGaps.md` lists what is unimplemented, half-wired or fragile, and what has and has
not been verified. Worth reading before promising a feature works - several things are implemented
but unreachable, and a good deal of Apple code compiles without ever having been run. The wiki's
`What works where` is the same information as a matrix, and distinguishes *implemented* from *run*.

## Notes specific to this repo

- No `global.json`; `dotnet --version` on this machine is 10.0.400.
- `WebRTCme.Bindings/README_BuildBindings.txt`, `README_NativeSdkVersions.txt`, and
  `IOS_BINDINGS_BUILD_PROBLEMS_FROM_WINDOWS.txt` document native SDK/binding build quirks — check
  these before touching binding projects. The native binaries themselves are built by a separate
  repo, [WebRTCnative](https://github.com/melihercan/WebRTCnative), and committed here.
- **`.github/workflows/ci.yml` is manual (`workflow_dispatch`), not on push.** It builds the Apple
  slices on a Mac, packs and verifies on Windows, and runs the test tiers. Nothing verifies a commit
  automatically, so a packaging fault sits undetected until someone runs it — **run it before
  tagging a release, not after.**
- `publish.yml` publishes on a `v*` tag via NuGet Trusted Publishing (no API key secret). It
  **builds nothing**: it promotes the exact `.nupkg` bytes a CI run produced and you tested. A tag
  with no matching CI artifact has nothing to publish.
- The MAUI packages are pinned to `Microsoft.Maui.Controls` 10.0.101, and **every MAUI consumer has
  to name that version themselves** or NuGet fails the restore with NU1605. Raising it here means
  raising it in `Tests/WebRTCme.PackageTests` and in the wiki's `Getting started`,
  `Platform prerequisites` and `Releases` pages in the same change.
