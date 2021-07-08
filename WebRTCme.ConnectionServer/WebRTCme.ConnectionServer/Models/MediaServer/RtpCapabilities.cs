using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme.ConnectionServer
{
    public class RtpCapabilities
    {
        public RtpCodecCapability[] Codecs { get; init; }
        public RtpHeaderExtension[] HeaderExtensions { get; init; }
    }
}
