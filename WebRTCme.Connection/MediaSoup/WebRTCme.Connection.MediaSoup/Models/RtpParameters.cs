using System;
using System.Collections.Generic;
using System.Text;
using Utilme.SdpTransform;

namespace WebRTCme.Connection.MediaSoup
{
    public class RtpParameters
    {
////        public /*Mid*/string Mid { get; set; }
        public RtpCodecParameters[] Codecs { get; set; } 
        public RtpHeaderExtensionParameters[] HeaderExtensions { get; set; }
        public RtpEncodingParameters[] Encodings { get; set; }
        public RtcpParameters Rtcp { get; set; }
        public /*Mid*/string Mid { get; set; }
    }
}
