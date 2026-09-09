using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme.Connection.MediaSoup
{
    public class RtpCapabilities
    {
        public RtpCodecCapability[] Codecs { get; set; }
        public RtpHeaderExtension[] HeaderExtensions { get; set; }
    }
}
