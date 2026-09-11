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

### Mute / pause / resume - wired and verified peer-to-peer 2026-09-11
Was: the send side did not exist. `IConnection` had no mute, and the `PauseProducer` /
`ResumeProducer` calls in `MediaSoupConnection` were commented out, so muting locally never
reached the server and other peers kept receiving.

Now `IConnection.SetOutgoingMediaEnabledAsync(kind, enabled)` carries it on both paths:

- **peer-to-peer** disables the local track - one stream feeds every peer connection, so no
  per-peer work and no renegotiation - and then sends the signalling media message that already
  existed and that nothing had ever called;
- **mediasoup** pauses the producer *and* tells the server, which is what stops the SFU forwarding
  and what makes other peers see it. Pausing only locally leaves the SFU relaying silence.

Both directions now report: `SignalingConnection.OnPeerMediaAsync` used to `throw new
NotImplementedException`, which means the first peer in a room ever to mute would have taken down
every other client - the server relays that message whether or not anyone handles it. On the
mediasoup path `consumerPaused` / `consumerResumed` now raise the same `PeerMedia` response,
derived from all of that peer's consumers rather than the one that changed.

Still missing: **voice activity detection**. The wire carries a `speaking` flag and the server
relays it; nothing computes it, so it is always sent as false.

**Verified in an Android/Windows call on 2026-09-11**, in both directions and for both kinds. See
"How to tell a mute actually happened" below - it is not as obvious as it sounds, and the first
attempt at verifying it proved nothing.

**The mediasoup half is verified too** (Android/Windows, 2026-09-11), and it needed a protocol fix
first. `pauseProducer` was being sent as a protoo *request* and the server answered `invalid protoo
request method`. It is a **notification**: the demo server keeps two separate dispatchers, and
which side a method falls on has nothing to do with how important it is - pausing a producer is a
notification, while asking for transport statistics is a request.

```
14:17:55  <== request      pauseProducer      -> error: invalid protoo request method
14:40:21  <·· notification pauseProducer      -> consumerPaused to the other peer
```

The notifications are `closeProducer`, `pauseProducer`, `resumeProducer`, `pauseConsumer`,
`resumeConsumer`, `setConsumerPreferredLayers`, `setConsumerPriority`, `requestConsumerKeyFrame`
and `changeDisplayName`; everything else this client sends is a request. `MethodName` now groups
its constants that way. Sending one the wrong way fails in both directions and neither failure is
obvious: as a request it is rejected, and as a notification it is silently ignored.

`IMediaSoupServerApi` had no way to send a notification at all - only `ApiAsync`, which waits for a
response - so `NotifyAsync` was added alongside it.

### ICE restart - implemented but unreachable
`Handler.RestartIceAsync` and `Transport.RestartIceAsync` exist and are ported. Nothing calls
them: `IConnection` still has no route to a restart, and `MediaSoupConnection` never invokes one.
A connection that loses its ICE path stays lost.

Unlike mute, this is not simply a matter of widening the interface - but it is less work than it
first looked. **The server already supports it**: `restartIce` is a protoo *request* in the demo
server's dispatcher, alongside `join` and `produce`, and `MethodName.RestartIce` now names it. What
is missing on the mediasoup side is the request and response types and a caller. The peer-to-peer
path still needs an offer with `iceRestart` set plus a rule for which side starts it.

### The SFU will not climb past the bottom layer
**The biggest open quality problem on the mediasoup path.** Given a choice of simulcast layers, the
server parks a native consumer on spatial layer 0 and shuffles, and the picture is visibly blurry
and freezes. Measured Blazor -> Android on 2026-09-11, same LAN, same pair of peers, minutes apart:

| | inbound video | resolution | layer changes |
| --- | --- | --- | --- |
| simulcast on | 64-159 kbit/s | 289x240 | 21, parked on spatialLayer 0 |
| simulcast off | 1781-1828 kbit/s | 578x480 | none |

The single-layer figure is the important one: **the path demonstrably carries 1.8 Mbit/s**, so
capacity is not the constraint. The server chooses badly when it has something to choose from.

What has been ruled out, each by measurement rather than argument:

- **Packet loss.** 2 lost of 7074 on the receiving side, and `consumerScore` reports a flat 10.
- **The client's encodings.** All layers `active`, correct `scaleResolutionDownBy`.
- **The SDP.** All RIDs negotiated both ways, `a=simulcast` correct in both directions, no `b=`
  bandwidth line anywhere, transport-cc and the wide-CC header extension present.
- **The publisher's uplink estimate.** 2349 kbit/s available against 1290 used.
- **`initialAvailableOutgoingBitrate`.** Raised from 1 Mbit/s to 10 Mbit/s on the server and
  reverted again: no measurable effect. Do not reach for it again without new evidence.
- **`setConsumerPreferredLayers`.** Now implemented and accepted by the server, and it does not
  help: preferred layers are a **ceiling, not a floor**, so asking for the top layer changes
  nothing when the server has already decided to send the bottom one.

What is left, and where to look next: mediasoup's per-consumer layer selection and its probing -
why the estimate for these consumers never climbs, when the same transport carries 1.8 Mbit/s
happily as soon as there is only one layer to send.

**Until then the demo apps set `UseSimulcast: false`.** That is a demo-app configuration, not a
library change; anything consuming the package can still turn it on. It gives up per-consumer
adaptation, which is most of the point of an SFU, so it is a stopgap and not an answer.

### A simulcast ladder has to suit the camera - fixed 2026-09-11
Related but genuinely fixed, and worth separating from the above because it was a real defect with
a real cause.

The encodings were copied from mediasoup-demo: three layers at 1/4, 1/2 and full size with a
5 Mbit/s top, which assumes a 720p or 1080p camera. Against a 640x480 webcam the rate allocator has
to fund the lower layers' maxima - 500 kbit/s and 1 Mbit/s - before it reaches the top rung, and
**never got there**. Chrome reported the top encoding `active` with `framesEncoded: 0` and
`qualityLimitationReason: "none"` for 121 seconds: not throttled, simply never funded. So the best
layer did not exist, and no consumer could do better than 320x240 however it asked.

`SimulcastEncodingsFor` now picks the ladder from the track's height - three layers at 720p and
above, two below, with a top bitrate matched to the resolution. Verified: both layers encode, the
top one at 640x480 and 1093 kbit/s, where before it produced nothing.

The lesson is the diagnostic one. `qualityLimitationReason: "none"` on an encoding that has encoded
nothing is the signature of an unfunded layer, and it looks nothing like the bandwidth problem it
gets mistaken for - which is exactly what happened here, twice, before the stats were read
properly.

See "The jumping tile" below for what the layer changes do to the UI, which is a separate fault
with a much cheaper fix.

### Send-side statistics - added and verified 2026-09-11
Was: `GetStats` takes a peer id, and on an SFU a peer can only ever be a receive-side answer - the
producers carrying this client's own media belong to no peer in particular. So there was no
outbound bitrate and no loss figure for anything this client sent, on any platform.

`IConnection.GetOutgoingStatsAsync()` is the second route. MediaSoup merges the mic and webcam
producers' reports; the plumbing beneath them already ran all the way down to `Sender.GetStats()`
and nothing had ever called it. Peer-to-peer returns every peer's sending half at once, keyed by
peer id - the same camera is encoded once per peer, and stats ids are unique only within one peer
connection, so merging them raw would drop one peer's streams on top of another's. `CallViewModel`
polls it on its own timer, started with the connection rather than with the first peer, because
the interesting part happens before anyone else joins.

Read `bytesSent`, not `framesEncoded`, to judge a video mute - see "How to tell a mute actually
happened". The first thing this found is the entry below.

### Muting did not stop the sender - fixed 2026-09-11
`Transport.ProduceAsync` passed `options.DisableTrackOnPause ?? false`, and `MediaSoupConnection`
never sets that option. mediasoup-client defaults it to **true**. So `Producer.Pause()` set
`Paused = true` and did nothing else - it never touched `Track.Enabled`.

Everything observable pointed the other way: the button flipped, `pauseProducer` went out, the
server stopped forwarding, and every peer saw the mute and measured the drop. The client simply
kept encoding and sending a full-rate stream for the SFU to discard. On a phone that is battery and
mobile data spent on video nobody receives.

Measured on a muted Android producer before the fix: 130 frames and ~150 kB of video every 5s,
*identical* to the rate before the mute. After it, same device, same test:

| | before mute | after mute | |
|---|---|---|---|
| audio bytes / 5s | 38,516 | 775 | 50x |
| video bytes / 5s | 457,197 | 12,544 | 36x |

Blazor was affected identically - audio 40,250 -> 48 bytes per 5s, video 653,228 -> 7,392. This was
never platform-specific. It was invisible everywhere, because proving it needs a send-side report.

`StopTracks` stays at `?? false`, deliberately unlike mediasoup-client: `stopTracks: true` would
stop the camera track when a producer closes, and that is the same track the local preview renders.

**The lesson is about what the earlier mute testing proved.** Verifying a mute from the *receiving*
peer - consumer paused, inbound bitrate down 16x - proves the SFU stopped forwarding. It cannot
prove the sender stopped, and it was read as though it had.

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

### The jumping tile - the MAUI `Media` view has no stable size
A remote tile on Android resizes whenever the incoming video's resolution changes, and the whole
layout shifts with it. Seen on the SFU call of 2026-09-11, where the tile alternated between
289x240 and 144x120 every twenty to thirty seconds:

```
09-11 17:12:54  BLASTBufferQueue update, w= 289 h= 240  ... caller= MediaView.n_onLayout
09-11 17:12:56  BLASTBufferQueue update, w= 144 h= 120  ... caller= MediaView.n_onLayout
```

Those numbers come through `MediaView.n_onLayout`, so it is the view's own measured size following
the video, not merely the decode buffer being resized underneath a stable view.

**Only the SFU path shows it**, which is why it went unnoticed for so long: peer-to-peer resolution
is settled at negotiation and does not change mid-call, whereas an SFU switches spatial layers
whenever its estimate moves. The cause is upstream - see "Simulcast layer control" above - but the
two are worth fixing separately, because a tile that keeps its size would stop the jumping whatever
the server decides, and that is a self-contained change in `WebRTCme.Middleware`.

### `IConnection` is narrow - mute added 2026-09-10, the rest still missing
It had three members, all call-scoped, so anything a real app wants - mute, screen share, ICE
restart, layer control, device switching - had no route through the interface. That is why several
of the items above read "implemented but unreachable": the code exists, the interface just does
not mention it.

`IsOutgoingMediaEnabled` and `SetOutgoingMediaEnabledAsync` are the first of these to be closed.
The pattern they set is worth keeping for the rest: one member that means the same thing on both
paths, implemented differently by each, with the state read back from wherever it actually lives
rather than mirrored in the caller - on the mediasoup path that is the producer's own `Paused`
flag, which a reconnect resets without anyone asking.

Still with no route: screen share on the SFU path, ICE restart, simulcast layer control, and
device switching.

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

**MediaSoup runs on Windows** (2026-09-11), for the first time in this project's history. Android
and Windows held a two-way SFU call: both transports created, `join`, both peers consuming each
other's audio and video, SCTP connected on both transports, and live media in both directions.

It needed a change to the native shim rather than to this repository. The Windows binding threw
`NotSupportedException` from `AddTransceiver`, `GetTransceivers` and `GetReceivers`, and that was
honest: `WebRtcInterop.dll` exported 41 functions whose peer-connection surface was
`add_track`/`remove_track`, the Plan B shape, with no transceiver entry point at all. mediasoup-
client is unified plan throughout - it probes capabilities by adding a transceiver of each kind,
creates each send stream with its simulcast encodings, and finds a receive m-section by `mid` - so
it failed on its very first call, right after `getRouterRtpCapabilities`.

The shim now exports transceivers, receivers, per-sender and per-receiver statistics, and
`media_track_get_kind` (a track reached through a receiver arrived by negotiation and carries no
kind the caller already knows). See `WebRTCnative` `feature/transceivers`. **The DLL in
`WebRTCme.Bindings/Maui/WebRTCme.Bindings.Maui.Windows/native/win-x64/` is now built from that
branch** - a Windows build against an older shim will fail with `EntryPointNotFoundException`
rather than the old `NotSupportedException`.

Two things worth keeping from doing it:

- **Check the API against the branch being built, not against memory.** `cricket::MediaType` no
  longer exists in M152 and is `webrtc::MediaType::AUDIO`; the selector `GetStats` overloads take a
  `scoped_refptr` callback where the connection-wide one takes a raw pointer. Both were verified by
  fetching the headers first, and the compile confirmed it: the only errors were four instances of
  `-Wunsafe-buffer-usage`, which is a lint no header could have warned about.
- **Pass `webrtc_branch` explicitly when dispatching the build.** Left empty it resolves the latest
  stable Chromium milestone, which rolled to M153 on 2026-09-11; that branch fails `gn gen` on the
  runner image and wastes forty minutes before reaching a compiler. It would also have been the
  wrong WebRTC version to build against.

**Mute works peer-to-peer, both directions, both kinds** (Android + Windows, 2026-09-11). Camera
off collapsed outbound video from ~1.14 MB per 5s to 71 kB and it recovered on unmute; a remote mic
mute took the receiving peer's `inbound-rtp` audio level to exactly 0 and back. The receiving half
had never run before - `SignalingConnection.OnPeerMediaAsync` was `throw new
NotImplementedException()`, so the first peer in a room ever to mute would have thrown on every
other client.

Not verified, in rough order of risk:

- **What crashes the Windows app under Visual Studio's launch.** It is reproducible - twice under
  Ctrl+F5, against a deployment that activates and runs perfectly - but the cause is unknown. The
  managed exception behind the stowed one was `0x80131509`, an `InvalidOperationException`, raised
  before any of this project's code runs. See "Running and debugging the Windows app" above.
- **A peer consumed before it is announced still has no `DisplayName`** - `ReportPeerMedia` now
  falls back to the peer id, so the symptom is gone, but the underlying hole is not. The peer
  record is created on demand when its consumers arrive, and a client joining an occupied room
  never receives `newPeer` for the peers already in it; they arrive in the join response, which
  nothing reads into `PeerParameters.Peer`. The fallback works only because peer id and display
  name are the same string in this application - see "Peer id is the display name" above - so it
  breaks the moment that design gap is fixed.
- **The Mac Catalyst slice of a package built on Windows is still wrong.** See "The framework that
  fits neither platform" below: the repository is now correct for building from source on either
  OS, but a `.resources.zip` produced on Windows carries the flat framework, which macOS refuses.
  Nobody has consumed that slice from a package.
- **`ReplaceOutgoingTrackAsync`** - implemented, but the demo app has no path that calls it.
- **Blazor Debug builds** fail to boot on a `.pdb` fetch when served by `dotnet run`'s
  WebAssembly dev server; Release is unaffected. **Did not reproduce when launched from Visual
  Studio on 2026-09-10**, which points at the dev server rather than the app. Narrow it before
  spending time on it: reproduce with `dotnet run --launch-profile https` first.

## Running and debugging the Windows app

Three things about the Windows app cost time on 2026-09-11, and none of them is about WebRTC.

**A plain build does not refresh what gets deployed.** The MSIX layout under
`bin/Debug/<tfm>/win-x64/AppX/` is produced only by Visual Studio's deploy step. `dotnet build`,
`dotnet build -t:Rebuild` and `-p:GenerateAppxPackageOnBuild=true` all leave it alone - the last
one additionally fails, because the property flows to the referenced library projects, which are
not packaged - and there is no `Deploy` target on the project to invoke. So after any rebuild the
deployed copy is stale until VS deploys again, and an app launched from it runs old code while
looking entirely normal. **Check by hashing, not by timestamps**: compare the assemblies in `AppX/`
against the ones beside it in `win-x64/`. That is how a "the fix does not work" hour turned out to
be a build from thirty minutes earlier.

Do not delete the `AppX` folder to force a refresh. It does not come back from the command line,
and the app cannot be launched until VS regenerates it.

**Visual Studio's own launch crashes; the package runs fine.** Started with Ctrl+F5, the app dies
at startup with `0xc000027b` - a stowed exception - faulting in `Microsoft.UI.Xaml.dll`, having
produced no output of its own at all. The same deployed package, launched by activating it
directly, runs normally:

```powershell
Start-Process "shell:AppsFolder\3D60F9C0-02BD-427F-9DED-82EDCEE7EF30_9zz4h110yvjzm!App"
```

Observed twice each way within minutes, on an unchanged deployment, so the fault is in how VS
launches rather than in the app. **The correction that matters**: commit `23b23573` claims its
dispatcher fix is "consistent with" this crash. It is not. That fix concerns `Disconnect()`, which
runs only when a call ends or the subscription errors, and this crash happens before any of this
project's code executes. The fix is right on its own merits - bound state must not be touched off
the dispatcher - but it does not explain this crash, and the commit message overstates it.

So the procedure that works: **deploy from Visual Studio** (Ctrl+F5 - it regenerates the layout
even if its own launch then fails), then **activate the package** with the command above.

**Reading Windows debug output.** `Debug.WriteLine` reaches the debugger when one is attached, and
otherwise goes to the Win32 `OutputDebugString` channel, which a capture tool can read - only one
consumer gets it, so a capture works only when the app runs without a debugger. `Console.WriteLine`
reaches neither: a packaged WinUI app has no console, which is why `MediaSoupStub`'s protoo frames
were invisible on Windows until they were echoed to both.

## How to tell a mute actually happened

Two traps, both hit on 2026-09-11, and between them the first verification attempt proved nothing
at all despite the feature working perfectly.

**The apps register no logging provider.** There is no `AddLogging`, `AddDebug` or `AddConsole`
anywhere in `WebRTCme.DemoApp` or `WebRTCme.Middleware`, so **every `ILogger` call in the MAUI apps
goes nowhere** - not to logcat, not to the Visual Studio output window. Everything that does show
up uses `System.Diagnostics.Debug.WriteLine`, which is why `SignalingConnection` is full of
`Debug.WriteLine` with `////_logger.LogInformation` commented out beside it. The proof is in any
captured log: `CallViewModel` writes the same stats line twice, once each way, and only the
`########` one ever appears. New diagnostics must use `Debug.WriteLine` or they are invisible.

**Byte counters cannot show an audio mute.** Disabling an audio track makes it send *silence*, not
nothing - Opus keeps emitting packets. Measured across a real mute: inbound audio fell from ~24 kB
per 5s to ~8.7 kB, which is a dip, not a stop, and is easily mistaken for jitter. Video is the
opposite and is obvious: black frames compress to almost nothing, so outbound video drops by more
than an order of magnitude.

So the stats line carries `lvl:[…]`, printed from any report exposing `audioLevel` - `inbound-rtp`
for what a peer is sending you, `media-source` for your own microphone. That goes to exactly 0 on
mute and back on unmute, and it is the only cheap evidence available. Read it, not the bytes.

**`framesEncoded` cannot show a video mute either, on the sending side.** A disabled video track
does not stop feeding the encoder - it feeds it black frames, so the frame counter keeps climbing
at the full frame rate through a mute. Only the bytes fall. Measured on a correctly muted Android
producer: 128 frames per 5s either way, while video fell from 457 kB per 5s to 12.5 kB. Reading the
frame counter there would say the mute failed when it worked.

**And a receiving peer cannot show anything about the sender.** A peer's consumer pausing proves
the *SFU* stopped forwarding, which it will do whether or not the sender stopped - that is the
whole point of pausing server-side. Only `GetOutgoingStatsAsync` can tell you what left this
device. Conflating the two hid the `DisableTrackOnPause` bug above for a full day of testing that
looked, at every step, like it had passed.

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
