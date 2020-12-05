using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme
{
    public class RTCRtpParameters
    {
        public RTCRtpCodecParameters[] Codecs { get; } 

        public RTCRtpHeaderExtensionParameters[] HeaderExtensions { get; }

        public RTCRtcpParameters Rtcp { get; }


    }
}
