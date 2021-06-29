using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme.Middleware
{
    public class PeerParameters
    {
        // Set for P2P or Mesh connections.
        public string TurnServerName { get; set; }

        // Set for Media Server connections.
        public string MediaServerName { get; set; }

        public string RoomName { get; set; }

        public string PeerUserName { get; set; }

    }
}
