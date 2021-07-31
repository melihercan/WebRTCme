using System;
using System.Collections.Generic;
using System.Text;
using WebRTCme.Connection.MediaSoup.Proxy.Enums;

namespace WebRTCme.Connection.MediaSoup
{
    public class Rtx
    {
        public int? Ssrc { get; init; }
    }

    public class RtpEncodingParameters
    {
        public uint? Ssrc { get; init; }
        public string Rid { get; set; }
        public int? CodecPayloadType { get; init; }
        public Rtx Rtx { get; init; }
        public bool? Dtx { get; init; }
        public string ScalabilityMode { get; init; }
        public int? ScaleResolutionDownBy { get; init; }
        public int? MaxBitrate { get; init; }
        public int? MaxFramerate { get; init; }
        public bool? AdaptivePtime { get; init; }

        public Priority Priority { get; init; }
        public Priority NetworkPriority { get; init; }


    }
}
