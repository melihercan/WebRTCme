# MediaSoup connection

An SFU alternative to the peer-to-peer `Signaling` connection. Every peer sends its media once to a
[mediasoup](https://github.com/versatica/mediasoup) server, which forwards it to everyone else, so a
call does not get more expensive for a client as peers are added.

Pick it at runtime: `ConnectionType.MediaSoup` instead of `ConnectionType.Signaling`. Both implement
`IConnection`, so nothing above the connection layer changes.

> 📖 **Full documentation:
> [Connection: MediaSoup](https://github.com/melihercan/WebRTCme/wiki/Connection-MediaSoup)** —
> how a call is set up, pointing clients at the server, the four things that will catch you, the
> simulcast measurements, and per-platform support.
>
> This file covers only what you need while standing in this folder.

## What is here

| | |
| --- | --- |
| `WebRTCme.Connection.MediaSoup` | A C# port of mediasoup-client v3 — `Device`, `Transport`, `Handler`, `Ortc`, the SDP layer — plus the protoo client and the server contracts |
| `server/` | A recipe for running the server this client talks to. **Not a server itself** |

`MediaSoupConnection` (in `WebRTCme.Connection`) drives the port and turns server events into the
`PeerResponse` stream the middleware consumes.

## The server is not part of WebRTCme

WebRTCme ships a **client**. The server it talks to is versatica's
[mediasoup-demo](https://github.com/versatica/mediasoup-demo), which you deploy yourself.

That matters more than it sounds, because of what the client speaks. mediasoup itself has no wire
protocol — it is a library your own signalling server calls. The `join`, `produce` and `newConsumer`
messages this client sends are **the demo app's own signalling API**, invented by that demo. So
"point it at a mediasoup server" is not enough: it has to be *that* server, and the API does change
between versions. Between 3.7.17 and 3.26 the response envelopes moved, ids were renamed, and SCTP
negotiation disappeared — each of which broke this client.

**So the version is pinned.** `server/Dockerfile` pins the exact mediasoup-demo commit
(`MEDIASOUP_DEMO_REF`) this client was verified against. Bump it deliberately and re-test; do not
float it.

The server announces its version in a `mediasoupVersion` notification on connect, which is the
quickest way to confirm what you are actually talking to.

## Running the server

mediasoup's worker is a native binary supported on Linux and macOS; there is no Windows build. On a
Windows machine, run it in Docker or on a Linux/macOS host.

```bash
cd WebRTCme.Connection/MediaSoup/server
docker build -t mediasoup-demo .
```

The build clones the pinned commit and compiles mediasoup's worker if no prebuilt binary exists for
the platform, which takes a few minutes the first time.

Run it, announcing the address your clients will reach it on — **not** `localhost`, unless every
client is on the same machine:

```bash
docker run -d --name mediasoup \
  -p 4443:4443/tcp \
  -p 44444:44444/udp -p 44444:44444/tcp \
  -e HTTP_LISTEN_PORT=4443 \
  -e NUM_WORKERS=1 \
  -e MEDIASOUP_LISTEN_IP=0.0.0.0 \
  -e MEDIASOUP_ANNOUNCED_ADDRESS=192.168.1.48 \
  -e DOMAIN=localhost \
  -e DEBUG='mediasoup-demo-server:*' \
  mediasoup-demo
```

**`NUM_WORKERS` is not a performance knob here — it decides how many ports you need.** The server
creates one worker per CPU by default and gives each its own RTC port, starting at 44444 and
counting up, so 32 cores means ports 44444-44475. Publish every port you create, or media silently
never flows while signalling looks perfectly healthy. One worker is plenty for testing.

**`MEDIASOUP_ANNOUNCED_ADDRESS`** is the address mediasoup puts in its ICE candidates, so it must be
reachable from every client.

Check it is up. The log should end with `Server started`. Over HTTP the API is access controlled, so
a **403 is the healthy answer** — it means TLS terminated and the server replied:

```bash
curl -sk -o /dev/null -w '%{http_code}' https://192.168.1.48:4443/rooms/test   # 403
```

`docker logs -f mediasoup` shows every protoo request per peer, which is the fastest way to see how
far a client got.

## Next

Pointing clients at it, the `DOMAIN` / `Origin` trap, certificates, simulcast and platform support
are all in
**[Connection: MediaSoup](https://github.com/melihercan/WebRTCme/wiki/Connection-MediaSoup)**.
