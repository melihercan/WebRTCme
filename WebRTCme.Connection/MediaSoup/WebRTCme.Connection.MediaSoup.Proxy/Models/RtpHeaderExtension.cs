using System;
using System.Collections.Generic;
using System.Text;
using Utilme.SdpTransform;

namespace WebRTCme.Connection.MediaSoup
{
    public class RtpHeaderExtension
    {
        public MediaKind Kind { get; init; }
        public string Uri { get; init; }
        public int PreferredId { get; init; }
        public bool? PreferredEncrypt { get; set; }
        public Direction? Direction { get; set; }

    }
}
