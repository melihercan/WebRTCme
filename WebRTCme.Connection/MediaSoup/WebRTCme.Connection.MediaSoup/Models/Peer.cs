using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme.Connection.MediaSoup.Proxy.Models
{
    public class Peer
    {
        // 'peerId' on the wire, not 'id'.
        public string PeerId { get; init; }
        public string DisplayName { get; set; }
        public Device Device { get; init; }
    }
}
