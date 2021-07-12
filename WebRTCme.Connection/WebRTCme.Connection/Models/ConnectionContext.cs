using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme.Connection
{
    class ConnectionContext
    {
        public string ConnectionServer { get; init; }

        public Guid Id { get; init; }

        public string Name { get; init; }

        public string Room { get; init; }

        public IObserver<PeerResponse> Observer { get; init; }

        public List<PeerContext> PeerContexts { get; init; } = new();

        public RTCIceServer[] IceServers { get; init; }

    }
}
