using System.Collections.Generic;
using WebRTCme.Connection.Signaling.Server.Enums;

namespace WebRTCme.Connection.Signaling.Server.Models
{
    public class Server
    {
        //public TurnServer TurnServer { get; set; }

        public RTCIceServer[] IceServers { get; set; }

        public List<Room> Rooms { get; set; } = new();
    }
}
