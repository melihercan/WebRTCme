using System;
using System.Collections.Generic;
using System.Text;
using WebRTCme;

namespace WebRtcMeMiddleware
{
    public class RoomResponseParameters
    {
        public string RoomName { get; set; }

        public string PeerUserName { get; set; }

        public string PeerSdp { get; set; }

        public RTCIceServer[] IceServers { get; set; }

    }
}
