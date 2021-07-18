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

        public object LocalParameters { get; set; }
        public object RemoteParameters { get; set; }

        public RtcpFeedback[] RtcpFeedback { get; set; }

    }
}