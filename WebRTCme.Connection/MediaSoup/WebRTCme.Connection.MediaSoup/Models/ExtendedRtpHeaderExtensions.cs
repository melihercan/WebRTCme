using Utilme.SdpTransform;

namespace WebRTCme.Connection.MediaSoup
{
    public class ExtendedRtpHeaderExtensions
    {
        public MediaKind Kind { get; init; }
        public string Uri { get; init; }
        public int SendId { get; init; }
        public int RecvId { get; init; }
        public bool? PreferredEncrypt { get; init; }
        public Direction? Direction { get; set; }

    }
}