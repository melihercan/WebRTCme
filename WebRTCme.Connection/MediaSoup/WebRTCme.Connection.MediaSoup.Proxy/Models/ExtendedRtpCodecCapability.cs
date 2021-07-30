using WebRTCme.Connection.MediaSoup.Proxy.Models;

namespace WebRTCme.Connection.MediaSoup
{
    public class ExtendedRtpCodecCapability
    {
        public MediaKind Kind { get; init; }
        public string MimeType { get; init; }
        public int ClockRate { get; init; }
        public int? Channels { get; init; }

        public int LocalPayloadType { get; init; }
        public int? LocalRtxPayloadType { get; set; }
        public int RemotePayloadType { get; set; }
        public int? RemoteRtxPayloadType { get; init; }

        public CodecParameters LocalParameters { get; set; }
        public CodecParameters RemoteParameters { get; set; }

        public RtcpFeedback[] RtcpFeedback { get; set; }

    }
}