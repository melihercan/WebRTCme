using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebRTCme.SignallingServer.Models
{
    public class Server
    {
        public TurnServer TurnServer { get; set; }

        public IEnumerable<Room> Rooms { get; set; } 
    }
}
