# The mediasoup demo server this client talks to.
#
# The server is not part of WebRTCme: it is versatica's mediasoup-demo, pinned to the commit
# this client was verified against. Its protoo signalling API is the demo's own invention
# rather than mediasoup's, and it does change between versions -- so the pin matters. Bump it
# deliberately and re-test, do not float it.
FROM node:22-bookworm

ARG MEDIASOUP_DEMO_REF=558aadd82985b98e293564288ee95c69ccd05e98

# mediasoup publishes prebuilt workers only for some platforms; anywhere else its native
# worker is compiled here, which needs meson and ninja driven from Python, plus a compiler.
RUN apt-get update && apt-get install -y --no-install-recommends \
        python3 python3-pip python3-venv build-essential ninja-build pkg-config git \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /src
RUN git clone https://github.com/versatica/mediasoup-demo.git . \
    && git checkout "$MEDIASOUP_DEMO_REF"

WORKDIR /src/server
RUN npm install --no-audit --no-fund

COPY config.mjs ./config.mjs
COPY certs /app/certs

CMD ["node", "bin/mediasoup-demo-server"]
