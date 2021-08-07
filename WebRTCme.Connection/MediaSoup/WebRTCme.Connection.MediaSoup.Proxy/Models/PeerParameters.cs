using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme.Connection.MediaSoup.Proxy.Models
{
    public class PeerParameters
    {
        public Peer Peer { get; init; }
        public List<string> ConsumerIds { get; set; } 
        public List<string> DataConsumerIds { get; set; } 
    }
}
