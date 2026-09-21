# For the DirectCallMe session — WebRTCme 26.9.21

Written 2026-09-21 from the WebRTCme session. DirectCallMe is on
`WebRTCme` / `WebRTCme.Middleware` **26.9.19-sinkfix**; the current CI package is **26.9.21**
([run 35577897079](https://github.com/melihercan/WebRTCme/actions/runs/35577897079)).

DirectCallMe drives `IRTCPeerConnection` itself through `CallSession` rather than going through
`SignalingConnection`, so **none of the recovery below arrives for free** — but one thing does
change under you on Android whether you ask for it or not.

---

## 1. Android now reports real peer connection states. Two of your switch arms were dead.

`CallSession.OnConnectionStateChanged` switches on `_pc.ConnectionState`. Until 26.9.20 the Android
binding did not report that state at all: it **synthesised** it from the ICE connection state, under
a comment asking *"I don't know why Android DOES NOT provide Connection State Change event???"*. It
does, and has for years — `onConnectionChange` on `PeerConnection.Observer`, already bound.

The synthesis raised the event **once** on leaving `Connected`, and at that moment the state reads
`Disconnected`. Every later transition raised nothing. So on Android:

| your arm | before 26.9.20 | from 26.9.21 |
| --- | --- | --- |
| `Disconnected when CurrentState == Connected` → `_reconnecting.OnNext(true)` | fired | fires |
| `Connected` → `_reconnecting.OnNext(false)` | fired | fires |
| `Failed` → `End(CallEndReason.ConnectionFailed)` | **never fired** | fires |
| `Closed` → `End(CallEndReason.HungUpByPeer)` | **never fired** | fires |

So an Android call whose transport died sat at `IsReconnecting = true` **forever** and never ended.
If you have ever seen that and not explained it, this is why. iOS, Mac Catalyst, Windows and Blazor
were always correct — this was Android only.

Mostly this is a fix. The part to look at deliberately: your handler **ends the call on the first
`Failed`**, and `Failed` is now reachable on Android. A transient failure that a restart would have
recovered will now hang up.

## 2. `RestartIce()` works on all five platforms now. It threw on four until 26.9.20.

`IRTCPeerConnection.RestartIce()` was `NotImplementedException` on Android, iOS and Mac Catalyst and
`NotSupportedException` on Windows — implemented only on Blazor. All five now work: the three native
SDKs had the call and the bindings simply never made it, and Windows needed a new ABI export in
WebRtcInterop.

Verified on a live connection on every platform, in the loopback tier: it asserts the ICE ufrag in
the next offer actually changes, checked against a build with the call suppressed, where it fails.

**You call `RestartIce()` nowhere.** Now that it works, `Failed` has an option other than hanging up.

## 3. The trap: `RTCOfferOptions.IceRestart` is read by exactly one binding.

Blazor passes the options object to the browser's `createOffer`, which honours `iceRestart`. Android
passes `new MediaConstraints()`, iOS and Mac Catalyst pass `new RTCMediaConstraints(null, null)`, and
Windows ignores the parameter. So `CreateOffer(new RTCOfferOptions { IceRestart = true })` produces a
**plain re-offer carrying the old credentials** on four of five platforms — the call renegotiates,
the request is answered, and nothing restarts.

This was WebRTCme's own bug: its manual restart did exactly that and was documented as "verified on
all five", because the verification was done in a browser. Fixed in 26.9.21.

**If you add a restart, call `RestartIce()` and then offer. Do not rely on the offer option.**

## 4. The shape that works, if you want recovery rather than hangup

This is what `SignalingConnection` now does, and each clause is there for a reason worth keeping:

- **On `Failed`, not on `Disconnected`.** W3C has `Disconnected` as a state that frequently recovers
  by itself; restarting on it throws away connections that were about to come back. Your existing
  `Disconnected` → `IsReconnecting` indicator is the right treatment for it.
- **Initiator only.** Both ends see `Failed`. Both offering is glare — two offers crossing, one
  rolled back — for no gain, since one restart re-runs the checks for the pair.
- **Off the callback thread.** The state change arrives on libwebrtc's signalling thread, which every
  peer connection in the process shares. Creating an offer inline from that callback deadlocks. A
  `Task.Run` is enough.
- **Bounded, then give up.** A restart that cannot succeed fails again; an unbounded reaction to
  `Failed` is an offer storm, not a recovery. Three attempts, then end the call the way you do now.
- **Reset the counter on `Connected`,** so a call that recovers and later fails again gets a fresh
  allowance rather than inheriting a spent one.
- **Say something while it happens.** You already have `IsReconnecting` wired to the view model, so
  this part is free — set it true while restarting. It matters more than it sounds: a dead peer
  leaves a still picture of a person on screen, so saying nothing does not leave the user
  uninformed, it leaves them believing the call is fine.

## 5. Also in 26.9.21, for completeness

- Automatic recovery, the reconnecting state, and `PeerReconnecting` / `PeerReconnected` peer
  responses — all on the `SignalingConnection` path, which DirectCallMe does not use.
- Everything is on WebRTC **M153** (`branch-heads/8010`), run on all four native platforms.
- `Microsoft.Maui.Controls` is still pinned to **10.0.101** and every MAUI consumer has to name that
  version itself, or NuGet fails the restore with NU1605.

## What is not proved

Nobody has watched a peer that **genuinely lost its network route** go to `Failed` and come back. The
restart is proved on every platform; the full loop is not, because a loopback has no route to lose.
If DirectCallMe gets to a real two-device test before WebRTCme does, that result is worth sending
back the other way.
