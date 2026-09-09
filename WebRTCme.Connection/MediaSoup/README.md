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
| `WebRTCme.Connection.MediaSoup.Server` | The mediasoup demo server (`Scripts/`), hosted from ASP.NET via Jering.Javascript.NodeJS |

`MediaSoupConnection` (in `WebRTCme.Connection`) drives the proxy and turns server events into the
`PeerResponse` stream the middleware consumes.

## Running the server

mediasoup's worker is a native process and **does not run on Windows**. On a Windows dev machine,
run the server in Docker or WSL. The version in `Scripts/package.json` is old enough that it needs
an old Node, which is easiest to pin in a container.

Create a build context from `WebRTCme.Connection.MediaSoup.Server` with this `Dockerfile`:

```dockerfile
FROM node:16-bullseye
WORKDIR /app
COPY package.json ./
RUN npm install --unsafe-perm --no-audit --no-fund
COPY . ./
CMD ["node", "server.js"]
```

Copy `package.json` and the contents of `Scripts/` next to it, then:

```bash
docker build -t mediasoup-demo .
```

Run it, announcing the address your clients will reach it on — **not** `localhost`, unless every
client is on the same machine:

```bash
docker run -d --name mediasoup \
  -p 4443:4443/tcp \
  -p 40000-40049:40000-40049/udp \
  -e PROTOO_LISTEN_PORT=4443 \
  -e MEDIASOUP_LISTEN_IP=0.0.0.0 \
  -e MEDIASOUP_ANNOUNCED_IP=192.168.1.48 \
  -e MEDIASOUP_MIN_PORT=40000 \
  -e MEDIASOUP_MAX_PORT=40049 \
  -e DEBUG='mediasoup-demo-server:*' \
  mediasoup-demo
```

`MEDIASOUP_ANNOUNCED_IP` is the address mediasoup puts in its ICE candidates, so it must be
reachable from every client. The narrowed RTC port range keeps the published UDP range small;
widen it for more than a handful of peers.

Check it is up — a 404 for an unknown room means the HTTPS server is answering:

```bash
curl -k https://192.168.1.48:4443/rooms/test
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

## Certificates

`Scripts/certs` holds a self-signed certificate. Browsers need it accepted once — visit
`https://<host>:4443/` and click through — and mobile clients would otherwise refuse the handshake
outright, so **Debug** builds fall back to a websocket that can be told to ignore certificate
errors on Android and iOS. Release builds always validate, so a real deployment needs a real
certificate.

## Things worth knowing

- **A client killed abruptly does not disconnect.** Swiping an app away sends no close frame, so
  the server keeps the peer until its own keepalive notices, which takes minutes. To test peers
  leaving, leave the call page instead.
- **Peer id is the display name.** Two clients joining one room with the same name collide, so
  give each device a different name.
- **The server is the mediasoup demo**, and it also runs a bot peer that opens a data consumer with
  no peer id attached. Client code that assumes every consumer belongs to a peer will trip on it.
- **Screen sharing and mute are not wired up** for this connection type yet.
