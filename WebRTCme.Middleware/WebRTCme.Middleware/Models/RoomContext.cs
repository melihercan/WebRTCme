using System;
using System.Collections.Generic;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using WebRTCme;
using WebRTCme.Middleware;

namespace WebRtcMeMiddleware
{
    public class RoomContext
    {
        //public RoomState RoomState { get; set; }

        public JoinRoomRequestParameters JoinRoomRequestParameters { get; set; }

        public Dictionary<string /*peerUserName*/, IRTCPeerConnection> PeerConnections { get; set; } = new();

        public RTCIceServer[] IceServers { get; set; }
    }
}
