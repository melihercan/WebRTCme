using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme.Connection
{
    public class ConnectionContext
    {
        public UserContext UserContext { get; init; }

        public IObserver<PeerResponse> Observer { get; init; }

        public List<PeerContext> PeerContexts { get; init; } = new();

        public RTCIceServer[] IceServers { get; set; }
    }
}
