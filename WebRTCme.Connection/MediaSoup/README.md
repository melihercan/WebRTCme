# MediaSoup connection

An SFU alternative to the peer-to-peer `Signaling` connection. Every peer sends its media once to
a [mediasoup](https://github.com/versatica/mediasoup) server, which forwards it to everyone else,
so a call does not get more expensive for a client as peers are added.

Pick it at runtime: `ConnectionType.MediaSoup` instead of `ConnectionType.Signaling`. Both
implement `IConnection`, so nothing above the connection layer changes.

## What is here

| Project | What it is |
| --- | --- |
| `WebRTCme.Connection.MediaSoup` | The `IMediaSoupServerApi` / `IMediaSoupServerNotify` contracts |
| `WebRTCme.Connection.MediaSoup.Proxy` | A C# port of mediasoup-client v3 — `Device`, `Transport`, `Handler`, `Ortc`, the SDP layer — plus `MediaSoupStub`, the protoo client |
| `server/` | A recipe for running the server this client talks to. Not a server itself |

`MediaSoupConnection` (in `WebRTCme.Connection`) drives the proxy and turns server events into the
`PeerResponse` stream the middleware consumes.

## The server is not part of WebRTCme

WebRTCme ships a client. The server it talks to is **versatica's
[mediasoup-demo](https://github.com/versatica/mediasoup-demo)**, which you deploy yourself.

That matters more than it might sound, because of what the client speaks. mediasoup itself has no
wire protocol — it is a library your own signalling server calls. The `join`, `produce`,
`newConsumer` and friends this client sends are **the demo app's own signalling API**, invented by
that demo. So "point it at a mediasoup server" is not enough: it has to be *that* server, and the
API does change between versions. Between 3.7.17 and 3.26 the response envelopes moved, ids were
renamed, and SCTP negotiation disappeared, all of which broke this client.

**So the version is pinned.** `server/Dockerfile` pins the exact mediasoup-demo commit this client
was verified against. Bump it deliberately and re-test; do not float it. The server announces its
version in a `mediasoupVersion` notification on connect, which is the quickest way to confirm what
you are actually talking to.

Verified against **mediasoup 3.26.0**, mediasoup-demo commit
`558aadd82985b98e293564288ee95c69ccd05e98`.

## Running the server

mediasoup's worker is a native binary supported on Linux and macOS; there is no Windows build. On
a Windows machine, run it in Docker or on a Linux/macOS host.

```bash
cd WebRTCme.Connection/MediaSoup/server
docker build -t mediasoup-demo .
```

The build clones the pinned mediasoup-demo commit and compiles mediasoup's worker if no prebuilt
binary exists for the platform, which takes a few minutes the first time.

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

**`NUM_WORKERS` is not a performance knob here, it decides how many ports you need.** The server
creates one worker per CPU by default and gives each its own RTC port, starting at 44444 and
counting up — so 32 cores means ports 44444-44475. Publish every port you create, or media
silently never flows while signalling looks perfectly healthy. One worker is plenty for testing.

`MEDIASOUP_ANNOUNCED_ADDRESS` is the address mediasoup puts in its ICE candidates, so it must be
reachable from every client.

Check it is up. The log should end with `Server started`. Over HTTP the API is access
controlled, so a **403** is the healthy answer — it means TLS terminated and the server replied:

```bash
curl -sk -o /dev/null -w '%{http_code}' https://192.168.1.48:4443/rooms/test   # 403
```

`docker logs -f mediasoup` shows every protoo request per peer, which is the fastest way to see
how far a client got.

## Pointing the clients at it

Set `MediaSoupServer:BaseUrl` in the app's `appsettings.json` (both
`WebRTCme.DemoApp.Blazor/wwwroot` and `WebRTCme.DemoApp.Maui`) to the same address:

```json
"MediaSoupServer": {
  "BaseUrl": "wss://192.168.1.48:4443",
  "Produce": true,
  "Consume": true,
  "UseDataChannel": true,
  "UseSimulcast": true,
  "ForceTcp": false,
  "ForceH264": false,
  "ForceVP9": false,
  "AudioOnly": false
}
```

Then run a client, choose **MediaSoup** as the connection server, enter a room name, and join. Any
number of clients joining the same room name land in the same call.

## Things worth knowing

- **`DOMAIN` must match the host in the clients' `BaseUrl`, and there is only one of it.** The
  server rejects a WebSocket whose HTTP `Origin` does not match its own, comparing scheme and host
  only, and a missing `Origin` counts as a mismatch and is refused with a 403. A browser sets the
  header itself, from wherever the app is served; the mobile clients derive it from the server
  address. So a browser served from `localhost` and a phone reaching `192.168.1.48` cannot both be
  accepted at once — serve the app on the same host the clients connect to, and set `DOMAIN` to
  that host.
- **Certificates.** `server/certs` holds a self-signed pair. Browsers need it accepted once — visit
  `https://<host>:4443/` and click through. Debug builds of the mobile clients fall back to a
  websocket that ignores certificate errors; release builds always validate, so a real deployment
  needs a real certificate.
- **A client killed abruptly does not disconnect.** Swiping an app away sends no close frame, so
  the server keeps the peer until its own keepalive notices, which takes minutes. To test peers
  leaving, leave the call page instead.
- **Peer id is the display name.** Two clients joining one room with the same name collide, so
  give each device a different name.
- **The demo server runs a bot peer** that opens a data consumer with no peer id attached. Client
  code that assumes every consumer belongs to a peer will trip on it.
- **Screen sharing and mute are not wired up** for this connection type yet.
