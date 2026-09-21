# WebRTCme

**WebRTC for .NET, with one API across five platforms.**

[![NuGet](https://img.shields.io/nuget/v/WebRTCme.svg?label=WebRTCme)](https://www.nuget.org/packages/WebRTCme)
[![NuGet](https://img.shields.io/nuget/v/WebRTCme.Middleware.svg?label=WebRTCme.Middleware)](https://www.nuget.org/packages/WebRTCme.Middleware)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://github.com/melihercan/WebRTCme/blob/master/LICENSE)

Every platform has its own WebRTC SDK, and no two look alike: a Java SDK on Android, an
Objective-C framework on Apple, a C++ library on Windows, and the browser's own JavaScript API on
the web. WebRTCme maps all of them onto a single C# surface that mirrors the
[W3C WebRTC 1.0 API](https://w3c.github.io/webrtc-pc/) — so if you have used `RTCPeerConnection` in
JavaScript, you already know this API.

```csharp
var window = CrossWebRtc.Current.Window();
var pc     = window.RTCPeerConnection(new RTCConfiguration { IceServers = [] });
var offer  = await pc.CreateOffer();
```

That runs unchanged on **Blazor WebAssembly, Android, iOS, Mac Catalyst and Windows**.

## 📖 Documentation is in the [Wiki](https://github.com/melihercan/WebRTCme/wiki)

| | |
| --- | --- |
| [**Getting started**](https://github.com/melihercan/WebRTCme/wiki/Getting-started) | Installing the packages and wiring them into a Blazor or MAUI app |
| [**Hello world**](https://github.com/melihercan/WebRTCme/wiki/Hello-world) | Sixty lines that negotiate a real connection — no server, no camera, no network |
| [**Platform prerequisites**](https://github.com/melihercan/WebRTCme/wiki/Platform-prerequisites) | **Read this before filing a bug.** Three traps, each of which fails silently |
| [**What works where**](https://github.com/melihercan/WebRTCme/wiki/What-works-where) | Honest per-platform matrix, including what is unverified |
| [**Releases**](https://github.com/melihercan/WebRTCme/wiki/Releases) | Versions, upgrading from 2.0.0, what is embedded |
| [**Troubleshooting**](https://github.com/melihercan/WebRTCme/wiki/Troubleshooting) | Symptoms, and what actually causes them |

## Packages

Two, and which you want depends on how much you want built for you.

| | What you get | When |
| --- | --- | --- |
| **[`WebRTCme`](https://www.nuget.org/packages/WebRTCme)** | The unified API and all five bindings, with their native halves. Peer connections, media streams, devices, data channels. | You have your own signalling and UI, or you are building a library. |
| **[`WebRTCme.Middleware`](https://www.nuget.org/packages/WebRTCme.Middleware)** | The above, plus a video tile, media managers, view models, and a connection layer that does the signalling for you — peer-to-peer or through a mediasoup SFU. | You want a working call app without writing the plumbing. |

`WebRTCme.Middleware` depends on `WebRTCme`, so referencing the middleware brings both. You never
reference a binding directly.

```powershell
dotnet add package WebRTCme.Middleware
```

Both packages carry every target framework — `net10.0` for Blazor, plus `net10.0-android`,
`net10.0-ios`, `net10.0-maccatalyst` and `net10.0-windows10.0.22621.0` for .NET MAUI — so you
reference the same package whatever you are building and NuGet picks the slice that matches.

> **.NET 10 only.** The 2.0.0 line is the .NET 8 line and is frozen; there is no back-port.
> Upgrading from 2.0.0 takes three edits — see
> [Releases](https://github.com/melihercan/WebRTCme/wiki/Releases#upgrading-from-200).

## Layout

```
WebRTCme.Bindings/     per-platform native access — Blazor JSInterop, Java, ObjC, Windows P/Invoke
WebRTCme/             the unified API and the plug-in            → package 1
WebRTCme.Middleware/  video tile, managers, view models          → package 2
WebRTCme.Connection/  signalling: mesh/P2P and a mediasoup SFU   ↳ folded into package 2
WebRTCme.DemoApp/     sample apps, Blazor and MAUI
Tests/                five tiers, from unit tests to on-device runs
doc/                  KnownGaps.md, TestingPlan.md, Packaging.md
```

The native WebRTC libraries are built by a separate repository,
[**WebRTCnative**](https://github.com/melihercan/WebRTCnative), and consumed here as prebuilt
binaries.

## Building

```powershell
dotnet build WebRTCme.sln
```

Everything builds on Windows, including the iOS and Mac Catalyst library slices — only *linking and
deploying an app* for those needs a Mac.

There is no `dotnet test`; the test projects are invoked directly. See
[Testing](https://github.com/melihercan/WebRTCme/wiki/Testing).

## Contributing

Issues and pull requests are welcome. Two things worth knowing first:

- `doc/KnownGaps.md` records what is missing, half-wired or fragile, and keeps entries after they
  are fixed with what the fault looked like beforehand. It is the fastest way to recognise
  something you have just hit.
- A `NotImplementedException` on a path that matters to you is **worth an issue**. Most of the
  unimplemented W3C surface has never been reached by anything, so a real call site is the signal
  that decides what gets filled in next.

## Credits

Special thanks to [Gøran Yri](https://github.com/EagleDelux) for his major contributions to the
.NET MAUI porting.

WebRTCme is MIT licensed. It embeds Google's WebRTC and its dependencies, which carry their own
terms — see
[what is inside the packages](https://github.com/melihercan/WebRTCme/wiki/Releases#what-is-inside-the-packages).
