using System;
using System.Collections.Generic;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using WebRTCme;
using WebRTCme.Middleware;

namespace WebRtcMeMiddleware.Models
{
    internal class CallContext
    {
        public JoinCallRequestParameters JoinCallRequestParameters { get; set; }

////        public Dictionary<string /*peerUserName*/, IRTCPeerConnection> PeerConnectionContexts { get; set; } = new();

        public List<PeerConnectionContext> PeerConnectionContexts { get; set; } = new();

        public RTCIceServer[] IceServers { get; set; }
    }
}
