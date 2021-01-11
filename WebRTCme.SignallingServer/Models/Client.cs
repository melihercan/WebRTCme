using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebRTCme.SignallingServer.Models
{
    public class Client
    {
        public string ClientId { get; set; }

        public bool IsInitiator { get; set; }

        public TurnServer TurnServer { get; set; }

        public string RoomId { get; set; }

        public string UserId { get; set; }

    }
}
