using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme.MediaSoupClient.Models
{
    class RtpCapabilities
    {
        public RtpCodecCapability[] Codecs { get; init; }
        public RtpHeaderExtension[] HeaderExtensions { get; init; }
    }
}
