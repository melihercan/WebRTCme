using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme
{
    public class RTCRtpEncodingParameters
    {
        public bool Active { get; set; } = true;

        public RTCRtpCodec Codec { get; set; }

        public ulong? MaxBitrate { get; set; }

        public double? MaxFramerate { get; set; }

        public string Rid { get; set; }

        public double? ScaleResolutionDownBy { get; set; }

    }
}
