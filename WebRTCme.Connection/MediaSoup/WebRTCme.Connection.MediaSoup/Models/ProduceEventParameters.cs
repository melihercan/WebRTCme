using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme.Connection.MediaSoup.Proxy.Models
{
    public class ProduceEventParameters
    {
        public MediaKind Kind { get; init; }
        public RtpParameters RtpParameters { get; init; }
        public Dictionary<string, object> AppData { get; init; }
    }
}
