using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme.Connection.MediaSoup
{
    public class RtpHeaderExtension
    {
        public MediaKind Kind { get; init; }
        public string Uri { get; init; }
        public int PreferedId { get; init; }
        public bool? PreferredEncrypt { get; init; }
        public Direction? Direction { get; set; }

    }
}
