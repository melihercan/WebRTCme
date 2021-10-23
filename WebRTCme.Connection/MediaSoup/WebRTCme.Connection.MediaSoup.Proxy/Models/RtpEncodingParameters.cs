using System;
using System.Collections.Generic;
using System.Text;
using WebRTCme.Connection.MediaSoup.Proxy.Enums;

namespace WebRTCme.Connection.MediaSoup
{
    public class Rtx
    {
        public uint? Ssrc { get; set; }
    }

    public class RtpEncodingParameters
    {
        public bool? Active { get; set; }
        public uint? Ssrc { get; set; }
        public int? ScaleResolutionDownBy { get; init; }
        public int? MaxBitrate { get; init; }
        public string Rid { get; set; }
        public int? CodecPayloadType { get; init; }
        public Rtx Rtx { get; set; }
        public string ScalabilityMode { get; set; }
        public bool? Dtx { get; set; }
        ////public string ScalabilityMode { get; set; }
        ////public int? ScaleResolutionDownBy { get; init; }
        ////public int? MaxBitrate { get; init; }

        public int? MaxFramerate { get; init; }
        public bool? AdaptivePtime { get; init; }

        public Priority? Priority { get; init; }
        public Priority? NetworkPriority { get; init; }


    }
}
