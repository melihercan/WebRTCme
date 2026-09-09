using System;
using System.Collections.Generic;
using System.Text;
using WebRTCme.Connection.MediaSoup.Proxy.Models;

namespace WebRTCme.Connection.MediaSoup
{
    public class  RtpCodecParameters
    {
        public string MimeType { get; init; }
        public int PayloadType { get; set; }
        public int ClockRate { get; init; }

        public int? Channels { get; set; }

        public Dictionary<string, object> Parameters { get; init; }
        public RtcpFeedback[] RtcpFeedback { get; set; }

    }
}
