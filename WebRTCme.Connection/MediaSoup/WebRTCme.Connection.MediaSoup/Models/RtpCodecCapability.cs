using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme.Connection.MediaSoup
{
    public class RtpCodecCapability
    {
        public MediaKind Kind { get; init; }
        public string MimeType { get; init; }
        public int PreferredPayloadType { get; init; }
        public int ClockRate { get; init; }
        public int? Channels { get; init; }
        public object Parameters { get; set; }
        public RtcpFeedback[] RtcpFeedback { get; set; }
    }
}
