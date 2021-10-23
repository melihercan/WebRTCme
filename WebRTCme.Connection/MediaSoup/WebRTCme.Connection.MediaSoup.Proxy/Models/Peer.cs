using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme.Connection.MediaSoup.Proxy.Models
{
    public class Peer
    {
        public string Id { get; init; }
        public string DisplayName { get; init; }
        public Device Device { get; init; }
    }
}
