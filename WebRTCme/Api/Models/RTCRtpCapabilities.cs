using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme
{
    public class RTCRtpCapabilities
    {
        public RTCRtpCodec[] Codecs { get; set; }

        public RTCRtpHeaderExtensionCapability[] HeaderExtensions { get; set; }
    }
}
