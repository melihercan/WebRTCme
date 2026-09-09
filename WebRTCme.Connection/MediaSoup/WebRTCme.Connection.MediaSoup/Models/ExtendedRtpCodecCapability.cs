using System.Collections.Generic;
using WebRTCme.Connection.MediaSoup.Proxy.Models;

namespace WebRTCme.Connection.MediaSoup
{
    public class ExtendedRtpCodecCapability
    {
        public string MimeType { get; init; }
        public MediaKind Kind { get; init; }
        public int ClockRate { get; init; }
        public int? Channels { get; init; }

        public int LocalPayloadType { get; init; }
        public int? LocalRtxPayloadType { get; set; }
        public int RemotePayloadType { get; set; }
        public int? RemoteRtxPayloadType { get; set; }

        public Dictionary<string, object/*string*/> LocalParameters { get; set; }
        public Dictionary<string, object/*string*/> RemoteParameters { get; set; }

        public RtcpFeedback[] RtcpFeedback { get; set; }

    }
}