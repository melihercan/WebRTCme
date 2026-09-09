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

### `IConnection` is narrow
Three members, all call-scoped. Anything a real app wants - mute, screen share, ICE restart, layer
control, device switching - has no route through the interface, which is why several of the items
above are "implemented but unreachable".

## Verified against, and not

Working and tested on this branch: three-peer calls (Blazor + Android + iOS) over both the
peer-to-peer and mediasoup paths, join/leave/rejoin, and camera release on leaving the call page.

Not verified, in rough order of risk:

- **Apple native linking.** The Mac Catalyst framework was corrupt in every package built on
  Windows until it was flattened; it is now structurally correct but nothing has linked it on a
  Mac. iOS links from Visual Studio on a Mac and has been run.
- **Mac Catalyst has never been run at all** - compile-verified only, on any release.
- **`ReplaceOutgoingTrackAsync`** - implemented, but the demo app has no path that calls it.
- **Camera actually released on Android** after `MediaStreamTrack.Stop()`. The capturer is stopped
  through the by-track-id registry, but this was written after the phone was unplugged.
- **Blazor Debug builds** fail to boot on a `.pdb` fetch; Release is unaffected. Cause unknown.
