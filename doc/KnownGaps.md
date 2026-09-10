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

### Teardown left the transport open - fixed 2026-09-10
Kept here because the diagnosis was wrong twice, and the wrong versions are the tempting ones.

The symptom: iOS, Android and Windows each made the server log

```
Socket connection closed prematurely.
WebSocketException: The remote party closed the WebSocket connection without completing the
close handshake.
```

right after a successful `LeaveAsync`. Blazor did not - it closed cleanly.

The cause was not that teardown went un-awaited, and not that `SignalingStub.DisposeAsync`
cancelled its own token before calling `StopAsync` (it does, and that would abort the handshake -
but that path never ran). **Nothing disposed anything.** `SignalingStub` and `SignalingConnection`
are both DI singletons, `SignalingConnection.DisposeAsync` only unsubscribes event handlers, and no
code anywhere called `DisposeAsync` on either. The socket was therefore only ever closed by the app
process going away. Blazor looked clean because the browser closes its own WebSocket regardless of
what managed code does - the most-tested platform was the one hiding it.

The fix closes the transport when a call ends instead of leaving it to the process:
`ISignalingServerApi` gained `EnsureConnectedAsync` / `DisconnectAsync` (default no-op, so the
server-side `RoomHub` implementation is unaffected), `SignalingConnection` connects before joining
and disconnects after closing the peers, and `SignalingStub` serialises the two behind a semaphore
so a join cannot race a close. Verified on Android over two consecutive calls: both produce
`Socket closed` and `Removing connection`, and the second reconnects through
`EnsureConnectedAsync`.

**The lesson worth keeping:** three platforms agreeing did *not* mean they shared a bug in the code
they share - it meant three of them lacked a workaround that the fourth had. When one platform out
of four behaves differently, the odd one out is as likely to be the one masking the problem as the
one causing it.

### CoreAudio object-not-found spam on Mac Catalyst
Every few seconds during a call, Mac Catalyst logs

```
(CoreAudio) AudioObjectHasProperty: no object with given ID 136
AVAudioSessionHALUtils.mm:277 GetSampleRateOfDevice: Error getting sampleRate of device 136
    err: 2003332927
```

`2003332927` is `kAudioHardwareBadObjectError`: WebRTC's audio layer is polling an audio device id
that no longer exists. Audio works regardless - the Telephony chain runs and the call is audible -
so this is noise rather than a fault, but it is worth knowing three things about it. It comes from
inside WebRTC's `AVAudioSessionHALUtils`, not from this project; it is Mac Catalyst specific,
because `AVAudioSession` there is emulated over the macOS HAL and device ids come and go in a way
iOS never sees; and it will bury anything else in the log at default verbosity. If a genuine audio
fault is ever chased on Catalyst, filter this out first rather than reading it as the cause.

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
errors on any of them. Android additionally ran two consecutive calls after the teardown fix, so
closing the transport between calls and reconnecting for the next one is covered.

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

**Mac Catalyst runs** (2026-09-10), for the first time in this project's history. On the Mac mini
it links, codesigns, launches, joins and leaves cleanly, captures from a USB webcam at a steady
30fps, and **holds a two-way call with Android** - remote video rendered on screen, and CoreAudio
running its `use_case=Telephony` chain with both uplink and downlink nodes, which only happens once
a peer is actually connected. That exercises `Platforms/MacCatalyst/MediaView.MaciOS.cs`, which had
never run. It needed the framework fix below.

Not verified, in rough order of risk:

- **The Mac Catalyst slice of a package built on Windows is still wrong.** See "The framework that
  fits neither platform" below: the repository is now correct for building from source on either
  OS, but a `.resources.zip` produced on Windows carries the flat framework, which macOS refuses.
  Nobody has consumed that slice from a package.
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

## The framework that fits neither platform

`WebRTC.framework` for Mac Catalyst cannot be stored in a form that works on both operating
systems, which took three attempts to establish:

| Layout | Windows checkout | macOS codesign |
| --- | --- | --- |
| Versioned - `Versions/A` plus symlinks | corrupt: git writes symlinks as small text files unless `core.symlinks` is on, so the 26MB binary arrives as 23 bytes | **required** |
| Flat, `Info.plist` under `Resources/` | intact | refused - *bundle format is ambiguous (could be app or framework)* |
| Flat, `Info.plist` at the root | intact | refused - same error; tested, not assumed |

So the repository stores it **flat**, and `build-versioned-framework.sh` rebuilds the versioned
bundle into `obj/versioned-framework/` at build time, on macOS only. Verified from a genuinely flat
tree - the state a Windows clone produces - and the committed framework is left untouched.

Two traps in that script's own history, both now guarded:

- `$(IntermediateOutputPath)` is **not defined** in the project body; the common targets that set
  it are imported afterwards. Using it collapsed the destination onto the source, and the script's
  `rm -rf` deleted the committed framework. Hence the fixed `objersioned-framework\` path.
- The script refuses to run when source and destination resolve to the same directory. That check
  exists because the above actually happened.

**Packaging: solved by building the Apple slices on macOS.** The binding's `.resources.zip` is
built from whatever `NativeReference` points at, so a package built entirely on Windows contains
the flat framework and its Mac Catalyst slice is unusable - and no single machine can produce all
five slices, because macOS cannot build `net10.0-windows` either.

`publish.yml` therefore runs two jobs. `apple` builds `net10.0-ios` and `net10.0-maccatalyst` on
`macos-latest` and uploads the assemblies and their `.resources.zip`; `publish` packs everything on
Windows and `Merge-AppleSlices.ps1` swaps those two slices in. iOS is built there too, not because
it needs to be - its xcframework is flat and packs correctly on Windows - but because keeping both
Apple slices on one machine removes a way to be subtly wrong.

The rule that makes it work: **the `.resources.zip` files are copied byte for byte and never
unpacked.** The symlinks live inside them, and rewriting one on Windows would flatten the framework
again. Verified end to end - symlink entries (`Versions/Current` at 1 byte, `Headers` at 24) are
still present after the merge rezips the outer package on Windows.

Two guards, because a merge that quietly does nothing looks exactly like one that worked:
`Merge-AppleSlices.ps1` fails if any artifact matches no entry in the package, and
`Verify-Packages.ps1 -RequireAppleNativeLayout` opens the Mac Catalyst `.resources.zip` and insists
on `Versions/A/WebRTC`. Both were tested against a Windows-only package, which they correctly
reject.
