using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme
{
    public class RTCRtpParameters
    {
        public RTCRtpCodecParameters[] Codecs { get; init; } 

        public RTCRtpHeaderExtensionParameters[] HeaderExtensions { get; init; }

        public RTCRtcpParameters Rtcp { get; init; }
    }
}
