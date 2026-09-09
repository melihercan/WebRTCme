using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme.Connection.MediaSoup.Proxy.Models
{
    public class DataProducerOptions
    {
        public bool Ordered { get; set; }
        public int? MaxPacketLifeTime { get; init; }
        public int? MaxRetransmits { get; init; }
        public string Label { get; init; }
        public string Protocol { get; init; }
        public Dictionary<string, object> AppData { get; init; }

    }
}
