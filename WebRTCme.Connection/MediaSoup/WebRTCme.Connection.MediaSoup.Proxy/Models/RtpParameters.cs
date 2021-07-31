using System;
using System.Collections.Generic;
using System.Text;
using Utilme.SdpTransform;

namespace WebRTCme.Connection.MediaSoup
{
    public class RtpParameters
    {
        public Mid Mid { get; init; }
        public RtpCodecParameters[] Codecs { get; set; } 
        public RtpHeaderExtensionParameters[] HeaderExtensions { get; init; }
        public RtpEncodingParameters[] Encodings { get; init; }
        public RtcpParameters Rtcp { get; init; }
    }
}
