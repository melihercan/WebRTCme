using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme.ConnectionServer
{
    public class RtpCodecCapability
    {
        public MediaStreamTrackKind Kind { get; init; }
        public string MimeType { get; init; }
        public int PreferredPayloadType { get; init; }
        public int ClockRate { get; init; }
        public int? Channels { get; init; }
        public object Parameters { get; set; }
        public RtcpFeedback[] RtcpFeedback { get; init; }
    }
}
