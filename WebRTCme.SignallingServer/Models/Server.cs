using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebRTCme.SignallingServer.Enums;

namespace WebRTCme.SignallingServer.Models
{
    public class Server
    {
        public TurnServer TurnServer { get; set; }

        public RTCIceServer[] IceServers { get; set; }

        public List<Room> Rooms { get; set; }
    }
}
