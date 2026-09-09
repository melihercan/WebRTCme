# WebRTCme

Cross-platform WebRTC framework for .NET. It maps each platform's native WebRTC SDK (browser
JS API via Blazor JSInterop, Android Java SDK, Apple ObjC SDK, Windows via a C ABI over
libwebrtc) to a single common .NET API, so MAUI/Blazor apps can use one WebRTC surface across
Windows, macOS (Mac Catalyst), Android, iOS, and web. The API's interfaces/models/enums mirror the
[W3C WebRTC 1.0 API](https://w3c.github.io/webrtc-pc/) (the same spec browsers implement), so the
C# surface should feel familiar to anyone who's used the browser WebRTC JS API (see the
[project wiki](https://github.com/melihercan/WebRTCme/wiki)).

Currently on `feature/dotnet10_support`, migrating the whole solution from .NET 8 to **.NET 10**
(net10.0 / net10.0-android / net10.0-ios / net10.0-maccatalyst / net10.0-windows10.0.22621.0).
`master` is still on .NET 8/MAUI-era code — expect ongoing target-framework and binding fixes on
this branch.

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
   - `MediaSoup/` — a C# port of mediasoup-client v3 for SFU group calls, working on Blazor,
     Android and iOS. The server is *not* here: it is versatica's mediasoup-demo, deployed
     separately, and `MediaSoup/README.md` says how.
5. **WebRTCme.DemoApp** — sample apps consuming the stack: `WebRTCme.DemoApp.Blazor` and
   `WebRTCme.DemoApp.Maui`.

Only two of these are published, as two NuGet packages — see `doc/Packaging.md`. Everything else
is folded into them.

There are no test projects. Xamarin is gone from the repo entirely; `README_V1.md` is the only
record of that era.

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
library slices - only linking and deploying an *app* for those needs a Mac. CI does exactly this
on `windows-latest`, which is the only runner that can build all five target frameworks in one
job, since `net10.0-windows10.0.22621.0` builds nowhere else.

## Known gaps

`doc/KnownGaps.md` lists what is unimplemented, half-wired or fragile, and what has and has
not been verified. Worth reading before promising a feature works - several things are
implemented but unreachable, and screen sharing works on two platforms out of five.

## Notes specific to this repo

- No `global.json`; `dotnet --version` on this machine is 10.0.400.
- No test projects at all. Don't assume `dotnet test` has coverage here; verification in this
  repo means building, reading the packed `.nupkg`, and running the demo apps.
- `WebRTCme.Bindings/README_BuildBindings.txt`, `README_NativeSdkVersions.txt`, and
  `IOS_BINDINGS_BUILD_PROBLEMS_FROM_WINDOWS.txt` document native SDK/binding build quirks — check
  these before touching binding projects.
- `.github/workflows/ci.yml` builds and packs on every push; `publish.yml` publishes both
  packages on a `v*` tag via NuGet Trusted Publishing (no API key secret). Packing runs in CI on
  every push on purpose, because packaging is where this repo fails quietly.
