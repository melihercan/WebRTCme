using System;
using WebRTCme.Connection.Signaling.Server.Enums;

namespace WebRTCme.Connection.Signaling.Server.Models
{
    public class Client
    {
        // From Context.ConnectionId.
        public string ConnectionId { get; set; }

        //public TurnServer TurnServer { get; set; }

        public Guid Id { get; set; } 
        
        public string RoomName { get; set; }

        public string UserName { get; set; }
    }
}
