using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme.Connection.MediaSoup
{
    public class RtpCapabilities
    {
        public RtpCodecCapability[] Codecs { get; init; }
        public RtpHeaderExtension[] HeaderExtensions { get; init; }
    }
}
