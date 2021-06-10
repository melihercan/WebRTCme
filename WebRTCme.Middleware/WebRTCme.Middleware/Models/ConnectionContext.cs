using System;
using System.Collections.Generic;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using WebRTCme;
using WebRTCme.Middleware;

namespace WebRTCme.Middleware.Models
{
    public class ConnectionContext
    {
        public ConnectionRequestParameters ConnectionRequestParameters { get; set; }

        public IObserver<PeerResponseParameters> Observer { get; set; }

        public List<PeerContext> PeerContexts { get; set; } = new();

        public RTCIceServer[] IceServers { get; set; }
    }
}
