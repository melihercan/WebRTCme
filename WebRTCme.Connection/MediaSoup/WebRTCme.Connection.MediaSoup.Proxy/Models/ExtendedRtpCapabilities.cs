using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme.Connection.MediaSoup
{
    public class ExtendedRtpCapabilities
    {
        public ExtendedRtpCodecCapability[] Codecs { get; init; }
        public ExtendedRtpHeaderExtensions[] HeaderExtensions { get; init; }

    }
}
