using System;
using System.Collections.Generic;
using System.Text;
using WebRTCme.Connection.MediaSoup.Proxy.Models;

namespace WebRTCme.Connection.MediaSoup
{
    public class  RtpCodecParameters
    {
        public string MimeType { get; init; }
        public int PayloadType { get; init; }
        public int ClockRate { get; init; }

        public int? Channels { get; init; }

        public CodecParameters/*Dictionary<string,string>*/ /*object*/ Parameters { get; init; }
        public RtcpFeedback[] RtcpFeedback { get; set; }

    }
}
