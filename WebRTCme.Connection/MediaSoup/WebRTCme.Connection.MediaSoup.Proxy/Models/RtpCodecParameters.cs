using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme.Connection.MediaSoup
{
    public class  RtpCodecParameters
    {
        public string MimeType { get; init; }
        public int PayloadType { get; init; }
        public int ClockRate { get; init; }

        public int? Channels { get; init; }

        public Dictionary<string,string> /*object*/ Parameters { get; init; }
        public RtcpFeedback[] RtcpFeedback { get; set; }

    }
}
