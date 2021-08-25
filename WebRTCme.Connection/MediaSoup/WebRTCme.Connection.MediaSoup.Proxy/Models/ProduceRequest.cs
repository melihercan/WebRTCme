using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme.Connection.MediaSoup
{
    public class ProduceRequest
    {
        public string TransportId { get; init; }
        public MediaKind Kind { get; init; }
        public RtpParameters RtpParameters { get; init; }
        public Dictionary<string, object> AppData { get; init; }
    }
}
