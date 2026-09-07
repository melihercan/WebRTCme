using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme
{
    public class RTCRtpCapabilities
    {
        public RTCRtpCodecCapability[] Codecs { get; set; }

        public RTCRtpHeaderExtensionCapability[] HeaderExtensions { get; set; }
    }
}
