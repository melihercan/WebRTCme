# WebRTCme

Cross-platform WebRTC framework for .NET. It maps each platform's native WebRTC SDK (browser
JS API via Blazor JSInterop, Android Java SDK, iOS ObjC SDK, desktop via SIPSorcery) to a single
common .NET API, so MAUI/Blazor apps can use one WebRTC surface across Windows, macOS (Mac
Catalyst), Android, iOS, and web. `WebRTCme.Api`'s interfaces/models/enums deliberately mirror the
[W3C WebRTC 1.0 API](https://w3c.github.io/webrtc-pc/) (the same spec browsers implement), so the
C# surface should feel familiar to anyone who's used the browser WebRTC JS API (see the
[project wiki](https://github.com/melihercan/WebRTCme/wiki)).

Currently on `feature/dotnet10_support`, migrating the whole solution from .NET 8 to **.NET 10**
(net10.0 / net10.0-android / net10.0-ios / net10.0-maccatalyst / net10.0-windows10.0.22621.0).
`master` is still on .NET 8/MAUI-era code — expect ongoing target-framework and binding fixes on
this branch.

## Layered architecture (bottom to top)

1. **Bindings** (`WebRTCme.Bindings/`) — raw per-platform WebRTC access, no common API:
   - `WebRTCme.Bindings.Blazor` — JSInterop wrapper over the browser WebRTC API (`wwwroot` has the JS).
   - `Maui/WebRTCme.Bindings.Maui.Android`, `Maui/WebRTCme.Bindings.Maui.iOS` — Android/iOS Java & ObjC bindings for MAUI.
   - `WebRTCme.Bindings.Native` — native interop (`webrtc.dll`, `libwebrtc.so`, `libwebrtc.dylib`) for desktop.
   - `WebRTCme.Bindings.SipSorcery` — desktop WebRTC via the SIPSorcery library.
   - `Xamarin/` — legacy Xamarin bindings, excluded from the solution, kept for reference only.
2. **WebRTCme.Api** — the common API surface (interfaces/models/enums, e.g. `IMediaDevices`,
   `IMediaStream`, `RTCPeerConnection`) that all bindings implement against.
3. **WebRTCme** (`WebRTCme/WebRTCme.csproj`) — the cross-platform plug-in library (`IWebRtc` /
   `CrossWebRtc`). Uses a bait-and-switch multi-target trick: per-`TargetFramework` conditional
   `<Compile Include>` picks the right `Platforms/{Blazor,Android,iOS,Windows,MacCatalyst}` folder.
4. **WebRTCme.Middleware** — services layer above the plug-in: media `<video>`-tag rendering
   (Blazor component / MAUI handler per platform), media stream service, signaling-server
   connection handling, and shared call/chat view models. Has Blazor/Maui/Xamarin sub-flavors.
5. **WebRTCme.Connection** — signaling and media-server integration:
   - `Signaling/` — client proxy + a signaling server implementation (mesh/P2P).
   - `MediaSoup/` — client proxy + server for SFU-based group calls via mediasoup (Blazor-only
     currently; Android/iOS have known issues).
6. **WebRTCme.DemoApp** — sample apps consuming the stack: `WebRTCme.DemoApp.Blazor` and
   `WebRTCme.DemoApp.Maui` (+ legacy `Xamarin/`).

`Tests/` holds interop test apps (`WebRtcInteropTestApp`, `TestApp`), not unit test projects.

## NuGet packaging

Three public packages, each a wider slice of the stack (see `NuGet/*.nuspec` and `README.md`):
`WebRTCme.Bindings` (bindings only) → `WebRTCme` (bindings + common API) → `WebRTCme.Middleware`
(everything, ready to build an app on top of).

## Build

Open `WebRTCme.sln` in Visual Studio, or:

```powershell
dotnet build WebRTCme.sln
```

Multi-target MAUI/binding projects generally need Visual Studio's workload tooling rather than
bare `dotnet build` on Windows for the Android/iOS/MacCatalyst targets. `WebRTCme.Api` and the
signaling/mediasoup server projects build fine with plain `dotnet build`/`dotnet run`.

## Notes specific to this repo

- No `global.json`; `dotnet --version` on this machine is 10.0.400.
- No xUnit/unit-test projects currently — `Tests/` are manual interop apps, not automated suites.
  Don't assume `dotnet test` has coverage here.
- `WebRTCme.Bindings/README_BuildBindings.txt`, `README_NativeSdkVersions.txt`, and
  `IOS_BINDINGS_BUILD_PROBLEMS_FROM_WINDOWS.txt` document native SDK/binding build quirks — check
  these before touching binding projects.
- CI workflows live in `.github/workflows/` (per-platform native builds, API build, testing).
