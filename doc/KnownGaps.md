# Known gaps

What is missing, half-wired or fragile, as of the .NET 10 branch (2026-09-09). Everything here was
checked against the code rather than remembered, and each entry says where it actually stands -
"not written" and "written but unreachable" need very different work.

Bugs found and fixed during the migration are in the git history, not here.

## Unimplemented features

### Screen sharing - works peer-to-peer on Blazor and Windows only
`ILocalMediaStream.GetDisplayMediaStreamAync` and `CallViewModel` both wire it up, and
`MediaDevices.GetDisplayMedia` is implemented for Blazor and Windows. On **Android, iOS and Mac
Catalyst it throws `NotImplementedException`** - Android needs a MediaProjection foreground
service, Apple needs ReplayKit and a broadcast extension, so neither is a small addition.
Separately, `MediaSoupConnection` never produces a display stream, so screen sharing does not
reach the SFU path on any platform.

### Mute / pause / resume - not wired for MediaSoup
The receive side is done: `consumerPaused` / `consumerResumed` notifications are handled and act
on the consumer. The send side is not - the `PauseProducer` and `ResumeProducer` calls in
`MediaSoupConnection` are commented out, so muting locally never reaches the server and other
peers keep receiving. `IConnection` exposes no mute at all.

### ICE restart - implemented but unreachable
`Handler.RestartIceAsync` and `Transport.RestartIceAsync` exist and are ported. Nothing calls
them: `IConnection` has only `ConnectionRequest`, `ReplaceOutgoingTrackAsync` and `GetStats`, and
`MediaSoupConnection` never invokes a restart. A connection that loses its ICE path stays lost.

### Simulcast layer control - absent
The client produces simulcast encodings, but there is no `setPreferredLayers` or
`setMaxSpatialLayer` anywhere, so a consumer cannot ask for a lower layer and nothing adapts to a
slow receiver.

### Send-side statistics - absent
`MediaSoupConnection.GetStats` walks the peer's *consumers* only and merges their reports. That is
the right shape for an SFU, where there is no per-peer send side, but it means no producer
statistics are available at all - no outbound bitrate, no packet loss on what this client sends.

### Camera selection ignores constraints
Android takes `GetCameraIdList()[1]` and iOS/Mac Catalyst take the front camera or simply the
first device, all with a TODO saying so. `MediaStreamConstraints` asking for a specific camera or
resolution is not honoured.

### Binding surface is incomplete
`NotImplementedException` counts under `WebRTCme/Platforms/`: ~47 Android, ~40 iOS, ~40 Mac
Catalyst, plus ~17 each in the Apple `Custom/` helpers. The paths the demo apps exercise work; the
rest of the W3C surface is stubs. `Blob` on Blazor cannot produce a `byte[]` from a JS
ArrayBuffer, and `MediaRecorder`/`Window` carry TODOs proposing the whole Blazor layer be rewritten
on `System.Runtime.InteropServices.JavaScript` instead of JSInterop.

## Design gaps

### Peer id is the display name
`MediaSoupConnection` joins with `DisplayName = userContext.Name` and keys every peer dictionary
by the server's `peerId`. Two clients joining one room under the same name collide, and the second
displaces the first. Documented as a caveat in the MediaSoup README, but it is a design choice
worth revisiting rather than a documented feature.

### `Handler._sem` is static
`static SemaphoreSlim _sem = new(1)` in the mediasoup `Handler` serialises SDP work across *every*
handler in the process, not per transport. With one connection it is invisible; with two it means
a send transport waits on an unrelated receive transport.

### `ToStringOrNumber` mutates dictionaries in place
`ModelExtensions.ToStringOrNumber` / `ToStringOrNumberOrBool` walk a `Dictionary<string, object>`
and rewrite its values to coerce what `System.Text.Json` produced into what mediasoup expects.
It is called on codec parameters and consumer `appData`. Fragile in both directions: a shape it
does not anticipate passes through unchanged, and the mutation is invisible to the caller.

### Teardown is fire-and-forget in places
Some close paths start async work without awaiting it, so a leave can return before the server has
been told. The server's own keepalive covers it eventually, which is why this rarely shows.

**Observed 2026-09-10 on three platforms out of four.** iOS, Android and Windows each produced
the same sequence against the signalling server: `LeaveAsync` arrives and succeeds, then the
socket dies without a closing handshake and the server logs

```
Socket connection closed prematurely.
WebSocketException: The remote party closed the WebSocket connection without completing the
close handshake.
```

**Blazor is the control case, and it is clean:**

```
######## LeaveAsync - id:...
Socket closed.
OnConnectedAsync ending.
Removing connection ... from the list of connections.
```

That split is what identifies the bug. All four run the same `WebRTCme.Connection` code, so the
shared layer is not behaving differently per platform - what differs is the WebSocket underneath.
Blazor WASM goes through the browser's own WebSocket, which completes the closing handshake when
the connection is torn down. iOS, Android and Windows use `System.Net.WebSockets.ClientWebSocket`,
which does not unless something calls `CloseAsync` and awaits it.

So this is one fix in the shared layer, but the mechanism is narrower than "teardown is not
awaited": the hub connection is disposed without being closed first, and only the browser
transport hides it. It also explains why it never caused visible trouble - the browser was the
most-tested platform.

Harmless as it stands: `LeaveAsync` has already removed the peer, so no ghost is left behind.

### `IConnection` is narrow
Three members, all call-scoped. Anything a real app wants - mute, screen share, ICE restart, layer
control, device switching - has no route through the interface, which is why several of the items
above are "implemented but unreachable".

## Verified against, and not

Working and tested on this branch: three-peer calls (Blazor + Android + iOS) over both the
peer-to-peer and mediasoup paths, join/leave/rejoin, and Windows in a call with Android over the
peer-to-peer path (2026-09-10).

**All four clients start, connect, join and leave** against the signalling server - iOS, Android,
Windows and Blazor, each run on its own from Visual Studio on 2026-09-10, with no application-level
errors on any of them.

**Android really does release the camera** on `MediaStreamTrack.Stop()` (verified 2026-09-10).
This was an open question because the code was written with the phone unplugged. Android reports
the device closed, not merely the track disabled:

```
Camera2Session: Stop camera2 session on camera 1
CameraManagerGlobal: Camera 1 ... state now CAMERA_STATE_IDLE
CameraManagerGlobal: Camera 1 ... state now CAMERA_STATE_CLOSED
Camera2Session: Camera device closed.
```

A `W/CameraCapturer: onFrameCaptured from another session` appears alongside it. During shutdown
that is a late frame from the session being torn down and is benign - but it is the same message
that masked the double-capture crash fixed in `8be81e88` for months, so treat it as a real signal
if it ever shows up at *startup* rather than at teardown.

**iOS links on a Mac again as of 2026-09-10**, which is worth its own note because the break was
invisible everywhere else. See "A failure only the Mac can see" below.

Not verified, in rough order of risk:

- **Mac Catalyst native linking.** Its framework was corrupt in every package built on Windows
  until it was flattened; it is structurally correct now, but nothing has linked it on a Mac. iOS
  is the reassuring precedent, not proof - they are separate frameworks.
- **Mac Catalyst has never been run at all** - compile-verified only, on any release.
- **`ReplaceOutgoingTrackAsync`** - implemented, but the demo app has no path that calls it.
- **Blazor Debug builds** fail to boot on a `.pdb` fetch when served by `dotnet run`'s
  WebAssembly dev server; Release is unaffected. **Did not reproduce when launched from Visual
  Studio on 2026-09-10**, which points at the dev server rather than the app. Narrow it before
  spending time on it: reproduce with `dotnet run --launch-profile https` first.

## A failure only the Mac can see

The bindings are referenced with `PrivateAssets="all"` so they stay out of the published packages'
dependency lists. That also stops the binding's `<name>.resources.zip` flowing to consumers - and
that zip is what the Apple SDK extracts `WebRTC.xcframework` from. The result:

- the app compiles;
- the registrar emits references to `_OBJC_CLASS_$_RTCAudioSession` and every other bound class;
- `clang++` then fails with *Undefined symbols for architecture arm64*, because no
  `-framework WebRTC` was ever passed.

**Nothing outside the Mac shows it.** A green CI, a clean `dotnet build WebRTCme.sln`, a package
that passes `Verify-Packages.ps1`, and a demo app built from those packages on Windows are all
consistent with a link that cannot succeed. The artefacts are all present and correct on Windows;
only the *reference set handed to the linker* is wrong.

The fix is in `WebRTCme.DemoApp.Maui.csproj`: the app references each platform binding directly, so
the zip travels with the assembly. Package consumers do not need it - there the assembly and its
zip sit side by side in `lib/<tfm>/`.

**How to check it without a Mac**, which is the only cheap signal available:

```powershell
dotnet msbuild WebRTCme.DemoApp/WebRTCme.DemoApp.Maui/WebRTCme.DemoApp.Maui.csproj `
  -p:TargetFramework=net10.0-ios -t:ResolveReferences -getItem:ReferenceCopyLocalPaths |
  Select-String "Bindings.Maui.iOS.resources.zip"
```

No match means the link will fail. On the Mac itself, the proof is that
`find ~/Library/Caches/maui/PairToMac/Builds -type f -name WebRTC` finds an extracted binary -
note the path is `maui/PairToMac`, not the older `Xamarin/mtbs`.
