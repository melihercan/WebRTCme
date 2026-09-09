using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme.Connection.MediaSoup.Proxy.Models
{
    public class PeerParameters
    {
        // Stable for the peer's lifetime, so the response reporting it joined and the one
        // reporting it left carry the same id and callers can correlate the two.
        public Guid Id { get; } = Guid.NewGuid();

        // Settable: the record is created as soon as something references the peer, which can
        // be a consumer arriving before the peer itself has been announced.
        public Peer Peer { get; set; }
        public List<string> ConsumerIds { get; set; } 
        public List<string> DataConsumerIds { get; set; } 
    }
}
