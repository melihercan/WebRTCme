using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme
{
    public class RTCRtpCodecParameters : RTCRtpCodec
    {
        public byte? PayloadType { get; init; }
    }
}
