# Known gaps

What is missing, half-wired or fragile, as of the .NET 10 branch (2026-09-09). Everything here was
checked against the code rather than remembered, and each entry says where it actually stands -
"not written" and "written but unreachable" need very different work.

Bugs found and fixed during the migration are in the git history, not here.

## Unimplemented features

### Screen sharing - its own source on the SFU path 2026-09-11
`ILocalMediaStream.GetDisplayMediaStreamAync` and `CallViewModel` both wire it up, and
`MediaDevices.GetDisplayMedia` is implemented for Blazor and Windows. On **Android, iOS and Mac
Catalyst it throws `NotImplementedException`** - Android needs a MediaProjection foreground
service, Apple needs ReplayKit and a broadcast extension, so neither is a small addition. That is
the whole of the limitation: sharing can only *start* where `getDisplayMedia` exists.

This entry used to claim screen sharing did not reach the SFU path at all. **That was wrong.**
`CallViewModel` shares by swapping tracks through `ReplaceOutgoingTrackAsync`, which
`MediaSoupConnection` implements by replacing the webcam producer's track - no separate display
producer is needed for it to work. Verified Blazor -> Android over mediasoup on 2026-09-11: the
shared tab appeared on the phone at about 2.25 Mbit/s, and stopping the share put the camera back.

**The screen is now its own producer** (2026-09-11), so on the mediasoup path a peer sees the
camera and the screen at once. `IConnection.StartScreenShareAsync` / `StopScreenShareAsync` carry
it, and the two paths deliberately differ in what peers end up seeing, because the difference is
visible to the user and not worth pretending away:

- **mediasoup** produces the screen separately with `appData { source: "screen" }`. The server
  copies `source` onto every consumer it creates, so the receiving end can tell two video streams
  from one peer apart - there is nothing else in a consumer that distinguishes them.
- **peer-to-peer** still swaps the camera track on the existing sender, so the screen arrives
  *instead of* the camera. Carrying both would mean negotiating a second transceiver with every
  peer.

Verified both ways on 2026-09-11, Blazor sharing to Android: over mediasoup the phone showed
`Alice` and `Alice (screen)` together and stopping retired only the screen tile; peer-to-peer
showed the screen in place of Alice's camera and put the camera back on stop.

**The real work was on the receiving side, not the producing side.** The `newConsumer` handler
used to pair a peer's consumers with `FirstOrDefault` for audio and `FirstOrDefault` for video and
emit one `PeerJoined` when it had both:

```csharp
// TODO: ASSUMED ONLY 1 video and 1 audio trak per peer.
if (audioConsumer is not null && videoConsumer is not null)
```

A second video consumer would never have been looked at. Consumers are now grouped by source,
each group is announced as its own tile with its own label, and `MediaStreamManager.Add` replaces
by label instead of appending - because a group is announced as soon as it has any track and again
as the rest arrive, audio and video being two separate notifications.

**That fixed the audio-only bug with it.** A peer publishing audio and no video used to produce no
`PeerJoined` at all - no tile, no name, while its audio played - reachable from a device with no
camera, from `MediaSoupServer:AudioOnly`, or from joining with the camera already off. The author
knew: the line above the guard read `// TODO: WE can have audio only calls!!!`.

And announcing earlier exposed a crash that had been waiting for it. All four platform handlers
did `mediaView.SetTrack(stream.GetVideoTracks().FirstOrDefault())` and every renderer dereferences
what it is handed, so the first audio-only announcement took the Android app down with a
`NullReferenceException` raised inside a MAUI property mapper. Guarded in all four, and again
inside `AndroidSupport.SetTrack`. A tile with no video now stays blank, and its audio plays
regardless - the peer connection plays it, not the view.

`OnPeerClosed` reads a peer's tile labels *before* closing its consumers, since closing them is
what makes the answer unavailable, and retires every one - a sharing peer that left used to leave
its screen tile on screen for the rest of the call.

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

**Voice activity now works on the mediasoup path** (2026-09-11). It cost nothing to compute: the
server runs an audio level observer and had been telling this client who was audible several times
a second, and the notification was being discarded. `MediaContext.Speaking` was on the response all
along, hardcoded false.

The notification to use is **`speakingPeers`**, not `activeSpeaker`, and that is worth knowing
because the names suggest the opposite:

- `speakingPeers` carries `{ peerVolumes: [ { peerId, volume } ] }`, continuously, per peer, with
  volume in dBov. Membership of the list is itself the signal - the observer only reports producers
  above its own threshold.
- `activeSpeaker` carries a peer id only from the `ActiveSpeakerObserver`'s `dominantspeaker` event,
  which **never fired once in testing**: not for tones, not for continuous synthesised speech, not
  even with a single audio producer left in the room. What did arrive was the audio level
  observer's *silence* case, which the server sends as `{ peerId: undefined }` - so it serialises
  to `{}` and is indistinguishable from a dominant speaker whose `appData` is missing. Half an hour
  went into that ambiguity before reading `Room.js` settled it.

Only the difference between snapshots is reported, since these arrive several times a second and
re-reporting every peer each time would push a stream of identical updates at the UI. Verified
Blazor -> Android: `speaking` toggled true and false in step with two bursts of speech separated by
a pause, one report per transition.

The flag follows the observer directly, so it flickers across the short gaps in natural speech. A
UI that highlights the speaker will want its own hold-off; that is a presentation decision and is
deliberately not made here.

**Peer-to-peer computes it locally** (2026-09-11), since there is no server in the media path to
observe anything. `SignalingConnection` samples `media-source`'s `audioLevel` - the microphone
before encoding - every 400ms from any one peer connection, since they all send the same local
track, and sends the signalling message only when the state changes.

Both constants came from measurement, and are worth keeping as measurements rather than taste:

- **Threshold 0.01.** In this project's own stats silence sits between 0.0001 and 0.0006 and speech
  runs 0.005 to 0.16, so 0.01 clears the noise and stays under the quietest speech seen. A noisy
  room will need it raised.
- **Hangover 2 seconds.** The first version used 900ms and the flag fell and rose *twice inside a
  single spoken sentence*, with the quiet stretches running 1.2 to 1.7 seconds. Anything under
  about 1.8s reproduces that flicker. At 2s, two bursts of speech produced exactly two
  transitions - held 8.7s and 10.4s - with nothing in between.

Muting is not special-cased: a disabled track reports a level of zero, so mute drops the flag by
itself. It is forced to false anyway, because "muted and speaking" is a contradiction no peer
should be sent.

The cost is one `getStats` call every 400ms, which on Blazor crosses the JS interop boundary. That
is the reason for sampling one peer connection rather than all of them, and the reason the interval
is not shorter.

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

### ICE restart - reachable and verified 2026-09-11
Was: `Handler.RestartIceAsync` and `Transport.RestartIceAsync` existed, were ported, and nothing
called them. A connection that lost its ICE path stayed lost.

`IConnection.RestartIceAsync()` is the route. MediaSoup restarts both transports independently and
collects failures rather than stopping at the first - they are separate ICE sessions, and a dead
send path does not imply a dead receive path. Peer-to-peer offers afresh with `IceRestart` set, and
only to peers where this side is the initiator, so the two ends do not both offer into a glare.

Verified on a two-peer mediasoup call: two `restartIce` requests, both answered, and both video
elements' `currentTime` advanced 4.5s across 4s of wall clock at 640x480 - media flowed straight
through the restart rather than recovering after it.

### The SFU's bandwidth estimate collapses under simulcast
**The biggest open quality problem on the mediasoup path**, and the entry that has been rewritten
most - each time because a measurement contradicted the explanation standing here. Read it from the
bottom if you want the current answer; the top is kept because knowing what was ruled out, and how,
is most of its value.

The symptom: given a choice of simulcast layers, the server parks a native consumer on spatial
layer 0 and shuffles, and the picture is visibly blurry and freezes. Measured Blazor -> Android on
2026-09-11, same LAN, same pair of peers, minutes apart:

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

**Re-measured 2026-09-11 with send-side statistics**, which the original investigation did not
have. Blazor -> Android, simulcast on, both layers published:

```
Alice sends:      video/r0 320x240  ~294 kbit/s   frames=1500
                  video/r1 640x480  ~1.5 Mbit/s   frames=1500
Android receives:                   38-87 kbit/s
```

Two things this settles that were previously only inferred from the encoder's own report:

- **The sender is ruled out by direct measurement.** Both layers encode, at identical frame counts,
  at healthy rates, continuously. Nothing on this side is failing to produce the top layer - which
  is what the ladder fix below was about, and is now confirmed rather than assumed.
- **It is not only spatial.** The receiver gets *less than a third of what layer 0 alone produces*.
  The server is not merely choosing the bottom spatial layer, it is throttling below it as well.
  "Parks on spatial layer 0" understates it.

**The server's own view, read for the first time on 2026-09-11** through
`MediaSoupServer:LogServerStats`. This is the measurement everything above was missing, and it
changes the diagnosis.

```
recvTransport: availableOutgoingBitrate over six samples
  142948 -> 142242 -> 108394 -> 156480 -> 47041 -> 62504

consumer outbound bitrate over the same window
  250429 -> 136394 -> 90064 -> 111619 -> 107168 -> 0

consumer score 10,  producer score 10,  packetsLost 1,  inbound rid: "r0" in all 12 samples
```

**The SFU is not choosing badly. Its own bandwidth estimate has collapsed.**
`availableOutgoingBitrate` sits between 47 and 156 kbit/s and never climbs, on the same LAN path
that carries 1.8 Mbit/s the moment there is one layer instead of two. Everything downstream follows
from that number: layer 0 is the only layer that fits in it, and the temporal throttling below layer
0 is the same estimate being enforced further. The scores being a flat 10 at both ends says the
media that *is* being sent arrives perfectly - this was never a quality problem, it is an estimate
problem.

So the earlier framing in this entry - "the server chooses badly when it has something to choose
from" - was wrong. It chooses correctly for what it believes the path can carry, and what it
believes is wrong by more than a factor of ten.

**The Docker bridge was the prime suspect, and it has been ruled out.** The same measurement taken
with the *browser* as the consumer, over the same container and the same bridge:

```
                    availableOutgoingBitrate           remoteIp
toward Blazor       469692 -> 504807 -> 565778 -> 568755    172.17.0.1
toward Android      142948 -> 108394 ->  47041 ->  62504    172.17.0.1
```

Same bridge, same NATed remote address, opposite behaviour - one climbing to half a megabit, the
other collapsing to a twentieth of that. Whatever sets the estimate apart, it is not the container's
networking. The paragraph that used to stand here blamed it, on the strength of the address alone.

One thing the browser run does show is that the estimate is **pessimistic in both directions**: the
transport toward Blazor reported 568 kbit/s available while successfully sending 900 kbit/s to that
same consumer. So `availableOutgoingBitrate` is not simply "low for Android" - it is unreliable
here generally, and only *matters* when simulcast gives the allocator something to choose with.

**And then the same transport, measured with simulcast off.** Same phone, same WiFi, same
container, same bridge, same room - the only thing changed is whether the publisher sends a ladder:

| | availableOutgoingBitrate | delivered to the consumer | probationBytesSent |
| --- | --- | --- | --- |
| simulcast **on** | 142948 -> 47041 -> 62504, falling | 250429 -> ... -> 0 | 17792 in every sample |
| simulcast **off** | 797244 -> 844077 -> 873838 -> 881760, rising | 1095354 -> ... -> 1298294 | 21824 -> 27328 -> 33472 |

**The estimate only collapses when simulcast is in play.** The path is not the constraint, the
phone's WiFi is not the constraint, and the container is not the constraint - all three are
identical across those two rows, and the estimate differs by a factor of fourteen. Every network
explanation this entry has carried is now ruled out by measurement rather than by argument.

The second column is the mechanism, or the start of it. `probationBytesSent` **grows** in the
healthy case and is **frozen** in the broken one. Probation is precisely what is supposed to break
the deadlock this entry describes - a low estimate picks a low layer, a low layer sends little
traffic, and little traffic gives the estimator nothing to raise its estimate with. With simulcast
off there is no layer decision to make: mediasoup forwards everything the producer sends, the
estimator has 1.3 Mbit/s of real traffic to measure, and it climbs.

So the question is no longer "why is the estimate low" but **"why does probation stop when the
consumer is a simulcast consumer"**. That is inside mediasoup's transport congestion control, not
in this client, and it is where anyone picking this up should start.

The remaining rig detail, kept because it is still true and still worth knowing when reading these
stats - it simply is not the cause: the selected ICE tuple is

```
"iceSelectedTuple": { "localIp": "0.0.0.0", "localPort": 44444,
                      "remoteIp": "172.17.0.1", "remotePort": 40343, "protocol": "udp" }
```

`172.17.0.1` is the Docker bridge gateway. The container runs with `--network bridge`, so every
peer reaches the SFU through Docker's NAT and appears at the gateway address regardless of where it
actually is. `MEDIASOUP_ANNOUNCED_ADDRESS` is set correctly, which is why media flows at all. It is
worth knowing when reading any of these stats - the remote address tells you nothing about which
peer you are looking at - but it is not what is holding the estimate down.

Where to look next, in order:

1. **Why probation stops for a simulcast consumer.** That is the one measured difference between a
   transport whose estimate climbs and one whose estimate collapses, and it sits in mediasoup's
   `TransportCongestionControlClient` and the simulcast consumer's layer allocation - server side,
   not here. Everything above is groundwork for this question.
2. **The initial layer.** If the allocator starts a simulcast consumer at layer 0 and probation is
   not running, nothing can ever lift it: little traffic gives the estimator nothing to measure, and
   a low estimate keeps the layer low. Worth checking whether starting higher breaks the loop, since
   that is testable from the server's configuration.
3. The rig is no longer a suspect at all, and re-measuring on host networking is no longer worth
   doing for this. It is kept here only because the notes below explain what the stats look like.

### Android cannot encode simulcast with this libwebrtc build
Found on 2026-09-11 while measuring the above, and separate from it. With `UseSimulcast: true` on
the phone, its own send-side statistics read:

```
out:[ audio bytes=514407 | video/r1 x bytes=0 frames=0 | video/r0 640x480 bytes=13377552 frames=1594 ]
```

`r1` is negotiated and reported, and encodes **nothing at all** - no frames, no bytes, not even a
frame size. `r0` carries the whole picture at full 640x480. That is the unfunded-layer signature
described under "A simulcast ladder has to suit the camera": an encoding that is active and simply
never funded, which looks nothing like throttling.

**Traced to the root on the same day, and it is not this project's code.** The chain, each step
measured rather than reasoned:

1. The ladder asked for is correct: `scale=2 max=400000, scale=1 max=1500000`, the same array that
   works on Blazor.
2. The ladder **negotiated** is correct too - the sender reports back
   `rid=r0 active=True scale=2 max=400000, rid=r1 active=True scale=1 max=1500000`. So the
   encodings reach the peer connection intact, and the earlier guess that they were being reordered
   or dropped in the binding was wrong.
3. The encoder ignores it anyway. `r0` encodes at the full 640x480 - its `scale=2` unapplied - and
   `r1` never encodes a frame.
4. `WebRtc.cs` builds the factory with `DefaultVideoEncoderFactory`, and **the AAR contains no
   simulcast-capable factory at all**. The only encoder factories in
   `Jars/libwebrtc.aar` are `DefaultVideoEncoderFactory`, `HardwareVideoEncoderFactory` and
   `SoftwareVideoEncoderFactory`. `SimulcastVideoEncoderFactory` - the class that wraps encoders in
   a `SimulcastEncoderAdapter` - is not there. Google's stock Android build does not ship it; the
   forks that support simulcast add it.

So Android can negotiate a ladder and can only ever encode one stream of it. **The fix is a
libwebrtc AAR that includes `SimulcastVideoEncoderFactory`**, or a simulcast-capable
`VideoEncoderFactory` written against the binding - either of which is a change to the native
dependency, not to this code.

Corroborated from the encoder's own side on 2026-09-11. libwebrtc logs what it was asked to build:

```
[VESFW] InitEncode(codec=VideoCodec {type: VP8, mode: RealtimeVideo,
        Simulcast: {[320x240 L1T3, active][640x480 L1T3, active]}}, ...)
```

Both layers arrive at the encoder, correctly configured, both active. `VESFW` is
`video_encoder_software_fallback_wrapper` - a **single** encoder. Nothing between the ladder and the
encoder is losing anything; there is simply no adapter to run two of them.

Until then, asking for simulcast on Android is actively worse than not asking: the SFU is told the
producer has two spatial layers, and one of them never carries anything. `UseSimulcast: false` is
right for the phone for that reason as well as the SFU one.

`LogNegotiatedEncodings` prints the requested ladder beside the negotiated one at produce time,
which is what separated step 2 from step 3 here. Reach for it first next time simulcast misbehaves:
it tells you immediately whether to look at this code or below it.

**What this does *not* invalidate:** the "SFU's bandwidth estimate" measurements were taken with Blazor
publishing and Android consuming, and Blazor's ladder is genuinely two layers. Only the later run
with Android as publisher was measuring a producer that could publish one.

### Reading RTP parameters back was broken on every native platform - fixed 2026-09-11
Found while chasing the above, and the reason it took as long as it did.

`RTCRtpSender.GetParameters()` threw on Android every single time, so nothing could read back a
negotiated ladder - and `SetMaxOutgoingSpatialLayerAsync`, added earlier the same day, was broken
on Android for exactly the same reason it had been on Blazor.

- `RtpParameters.Codecs` and `.Encodings` are bound as the **non-generic**
  `System.Collections.IList`. `as List<T>` on one of those yields null rather than failing, so
  `FromNativeToSend` and `FromNativeToReceive` threw `ArgumentNullException` from inside LINQ,
  naming the parameter `source` and pointing nowhere near the cast. Now read with `Cast<T>()`.
- `Codec.FromNative` unwrapped `NumChannels` and `ClockRate` without checking. Both are boxed Java
  `Integer`s and `numChannels` is **null for every video codec**, so this threw
  `NullReferenceException` on the first video codec in the list - which is to say on any peer
  connection actually carrying video.
- The encoding conversions unwrapped `MaxBitrate`, `MaxFramerate` and `ScaleResolutionDownBy` the
  same way, in both directions. Null means "no preference" on the way out as much as on the way in.

**iOS and Mac Catalyst had the same fault**, found by sweeping for the pattern once Android proved
it real. There the boxes are `NSNumber` rather than Java `Integer`, and the binding is explicit
about it - `clockRate`, `numChannels`, `maxBitrateBps`, `maxFramerate` and `scaleResolutionDownBy`
are all declared `_Nullable` in `ApiDefinitions.cs`. Every one was read with `.UInt64Value` or
`.DoubleValue` straight off a possibly-nil reference, so `GetParameters()` threw there too, on any
peer connection carrying video. Now read with `?.`, and the bridged arrays guarded the same way.

The outbound direction needed nothing on Apple: `NSNumber` converts from a nullable implicitly and
a null stays null. Android's did need it, because unwrapping a null `Integer` throws.

So the same defect existed on three platforms and, between them, `RTCRtpSender.GetParameters()`
worked on none of them - which is why nothing had ever noticed: no caller could get far enough to
find out. Blazor's version was broken too, differently, and was fixed hours earlier the same day
(see "A JS return value is a reference, not its contents"). **Four platforms, four bugs, one API.**
An API that no platform implements correctly is indistinguishable from one that is not there, and
this one had been sitting behind a feature nobody had asked for until simulcast layer control did.

The shape of the Android one is worth remembering on its own: an `as` cast that cannot succeed does
not fail, it produces null, and the exception then surfaces several frames away wearing someone
else's name.

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

### Camera selection ignores constraints - honoured 2026-09-11
Was: Android took `GetCameraIdList()[1]`, iOS and Mac Catalyst took the front camera or simply the
first device, and every platform captured at a fixed size. `CameraType` was worse than ignored -
`GetCameraMediaStreamAsync` accepted it and then asked for `video: true` regardless, so naming a
camera did exactly nothing.

`VideoConstraints` (in `Api/Helpers`) reads the four answers a camera needs out of the constraint
unions once, for everyone: which device, which way it faces, what size, how fast. `deviceId`,
`facingMode`, `width`, `height` and `frameRate` are now honoured on Android, iOS, Mac Catalyst and
Windows, in all their forms - a bare value, an array, `exact`, `ideal`, a min/max range. An `exact`
that cannot be met throws; an `ideal` is dropped quietly. `CameraType` becomes an ideal
`facingMode`.

Selection is by lens facing rather than by list index. Index 1 is the front camera on many devices
and not on others, and asking each camera which way it faces is what `facingMode` needs anyway.
Defaults are unchanged in intent and in fact: front camera, 640x480, 30fps.

Verified on Android: `CameraType.Back` opened the back camera where before the parameter did
nothing, and explicit constraints of 1280x720@24 produced `video 1280x720` in the send-side stats
where the default gives `640x480`. iOS, Mac Catalyst and Windows are compile-verified only.

**Two hidden defects fell out of this**, both of the same kind - libwebrtc answers an unsupported
capture request with the nearest supported format and says nothing:

- Android asked for `StartCapture(480, 640, 30)`. No camera here publishes 480x640;
  `Camera2Enumerator` reports landscape formats. 640x480 is simply what is nearest to the
  transposed pair, so the portrait request had been "working" by accident for as long as it existed.
  The first version of this fix preserved the transposition and asked a camera that publishes
  1280x720 for 720x1280 - which opened at **1088x1088**, nearer to the transposed pair than the
  format actually meant. The capture format the camera chose is now logged beside the list it
  supports, because that is the only way this class of mistake is visible.
- iOS used `SupportedFormatsForDevice(device)[6]`. The index is meaningless: the list differs per
  device and per iOS version, so index 6 is a different resolution on every phone, and it throws
  outright on a camera publishing fewer than seven formats. It now picks the closest supported
  format to the request and clamps the frame rate to what that format allows.

### Three native stubs were on live paths - filled in 2026-09-11
Auditing the stubs by whether anything actually calls them, rather than by counting them, turned up
two on `RTCRtpSender` that features added the same day walked straight into:

- **`SetParameters`** backs `SetMaxOutgoingSpatialLayerAsync`, so simulcast layer control threw
  `NotImplementedException` on Android. Implemented by fetching the native parameters, mutating
  them and handing them back - Java's `RtpParameters` has no public constructor, the only
  legitimate instance comes from `getParameters()`, and it carries state the native side checks.
  Encodings are matched by rid where there is one and by position otherwise. The native call
  reports refusal by returning false rather than throwing, so that is turned into an exception:
  a silently ignored parameter change is the failure this path exists to prevent.
- **`ReplaceTrack`** backs `ReplaceOutgoingTrackAsync` and therefore screen sharing and device
  switching. Implemented with `SetTrack(track, takeOwnership: false)` - ownership deliberately not
  taken, because the track belongs to the caller's stream, which the local preview is still
  rendering, and letting the sender dispose it would stop a track that is still in use.

Verified on device: the layer button now reports `max outgoing spatial layer set to 0` where it
previously raised an error popup. `ReplaceTrack` is **not** verified - reaching it on Android needs
`GetDisplayMedia`, which that platform does not have.

**`MediaStreamTrack.GetSettings` was the third**, and the one that had been failing quietly for
longest. `SimulcastEncodingsFor` picks the ladder from the track's height inside a `try`, so on
Android every video produce threw here, was swallowed, and took the fallback ladder. Nothing
downstream could tell: the ladder for an unknown camera and the ladder for a 480p camera are the
same ladder. It now returns the format the camera was opened with - Android reports nothing about
a track's live size, so that is the honest best answer - and the chosen height is logged either
way, so a guess no longer looks like a decision. Verified on device: `ladder chosen from height
480` where it used to be 0.

`GetConfiguration` is referenced but not on a live path - only `Handler.UpdateIceServersAsync`
calls it and nothing calls that - so it is left alone.

**iOS and Mac Catalyst had the same three**, found by running the audit against them once Android
proved it worth doing. Apple's versions are simpler - `parameters` and `track` are both settable
properties, where Android needs `SetParameters` and `SetTrack` and a decision about ownership - but
the shape of `SetParameters` is the same and for the same reason: the object handed back has to be
the one the sender produced, because it carries the codecs and header extensions the far side
negotiated, and a fresh one would send those back empty.

Compile-verified only on Apple; neither platform runs here.

The audit is worth repeating; the counting is not. 41 distinct members threw on Android and only
16 were referenced anywhere in the connection or middleware layers - after this, 38 and 13. Of
those 16, three were on paths that actually run, and all three are now implemented on all three
native platforms. The rest can wait indefinitely, and the useful question for the next one is not
"how many are left" but "does anything call it".

The remaining referenced ones, for whoever asks that question next: `GetDisplayMedia` (a platform
project of its own, above), `MediaRecorder`, `MediaStream.Create`, `GetCapabilities`,
`GetConstraints`, and the data-channel properties `BinaryType`, `Protocol` and
`BufferedAmountLowThreshold`. The data-channel three are referenced by `DataConsumer` and
`DataProducer` as pass-through properties rather than invoked by anything, so they are referenced
without being reached - which the audit cannot tell apart, and a reader has to check by hand.

### Binding surface is incomplete
`NotImplementedException` counts under `WebRTCme/Platforms/`: ~47 Android, ~40 iOS, ~40 Mac
Catalyst, plus ~17 each in the Apple `Custom/` helpers. The paths the demo apps exercise work; the
rest of the W3C surface is stubs. `Blob` on Blazor cannot produce a `byte[]` from a JS
ArrayBuffer, and `MediaRecorder`/`Window` carry TODOs proposing the whole Blazor layer be rewritten
on `System.Runtime.InteropServices.JavaScript` instead of JSInterop.

## Design gaps

### Peer id is the display name - fixed 2026-09-11
Was: `MediaSoupConnection` joined with the peer id set to the user's name, so two clients in one
room under the same name collided and the second displaced the first. The peer now joins under its
own GUID with the display name carried separately, and `newPeer` is deserialized correctly -
the payload nests the peer under `"peer"`, and reading the body as a `Peer` had produced nulls,
which were then discarded as peers with no id. That had been broken since the 3.26 upgrade.

Verified with two peers both named "Melih" in one room: both present, four `consume()` calls, no
displacement.

### `Handler._sem` is static - fixed 2026-09-11
Was: `static SemaphoreSlim _sem = new(1)` serialised SDP work across *every* handler in the
process rather than per transport, so a send transport waited on an unrelated receive transport.
Now an instance field.

### `ToStringOrNumber` mutates dictionaries in place - fixed 2026-09-11
Was: `ModelExtensions.ToStringOrNumber` / `ToStringOrNumberOrBool` rewrote the caller's dictionary
in place to coerce what `System.Text.Json` produced into what mediasoup expects. They return a new
dictionary now.

One trap worth keeping, because the first version of the fix fell into it:

```csharp
JsonValueKind.Number => element.TryGetInt32(out var i) ? (object)i : element.GetDouble(),
```

Without that `(object)` cast the ternary unifies to `double`, so every integer is boxed as a
double and the `(int)` unboxing downstream throws on something as ordinary as `"apt": 101`. It
killed the connection at `getRouterRtpCapabilities`, which looks nothing like a boxing fault.

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

### `IConnection` is narrow - four routes added, two still missing
It had three members, all call-scoped, so anything a real app wants - mute, screen share, ICE
restart, layer control, device switching - had no route through the interface. That is why several
items above used to read "implemented but unreachable": the code existed, the interface just did
not mention it.

Closed so far, in order: `IsOutgoingMediaEnabled` and `SetOutgoingMediaEnabledAsync`
(2026-09-10), then `RestartIceAsync`, `GetOutgoingStatsAsync`,
`SetPreferredIncomingLayersAsync` and `SetMaxOutgoingSpatialLayerAsync` (2026-09-11).

The pattern they set is worth keeping for the rest: one member meaning the same thing on both
paths, implemented differently by each, with the state read back from wherever it actually lives
rather than mirrored in the caller - on the mediasoup path that is the producer's own `Paused`
flag, which a reconnect resets without anyone asking.

**The interface is no longer the thing holding anything back.** The two entries that remained on
this list both turned out to need nothing from it: screen share already reaches both paths through
`ReplaceOutgoingTrackAsync`, and device switching is the same member plus a camera opened with
different constraints. What is left of either is platform work and features, not routes -
`getDisplayMedia` on the mobile platforms, and a second producer so a peer can see a camera and a
screen at once.

Layer control is the last member added, and it is worth knowing what it is not. The receive half
sets a **ceiling**: it caps what a peer costs, and it cannot raise a floor the server has put
down - which is why it was no help against the entry below. The send half is a real cap, because
switching an encoding off means those frames are never produced at all.

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

## A JS return value is a reference, not its contents

`JsInterop.callMethod` hands back an **object reference** for anything object-typed. That is right
for a thing you go on to call methods on - a track, a sender, a transport - and wrong for a plain
value object you want to read: deserializing a reference into a C# model produces a model with
every property null, and nothing anywhere says so.

Eleven Blazor binding methods were doing exactly that, so every one of them returned an empty
shell: `RTCRtpSender.GetParameters`, `RTCRtpReceiver.GetParameters`, both `GetCapabilities`
overloads, `MediaStreamTrack.GetCapabilities` / `GetConstraints` / `GetSettings`,
`RTCCertificate.GetFingerprints`, `RTCIceTransport.GetLocalParameters` / `GetRemoteParameters`,
and `RTCPeerConnection.GetConfiguration`.

Found because simulcast layer control threw `ArgumentNullException` with the parameter name
`source` - a LINQ call inside `Handler.SetMaxSpatialLayerAsync` on a null `Encodings` array. The
exception points at LINQ, four layers away from the interop that actually produced the null.

`callMethodWithContent` copies the result's content the way `getPropertyValue` already did, and
`CallJsMethodWithContent<T>` is its C# side. **Use it for any method returning a value object.**
Two of the eleven take an argument, and those must pass `null` for `contentSpec` explicitly -
`CallJsMethodWithContent<T>(parent, method, null, kind)` - because the optional `contentSpec`
parameter sits before `params object[] args` and will otherwise swallow the first argument.
