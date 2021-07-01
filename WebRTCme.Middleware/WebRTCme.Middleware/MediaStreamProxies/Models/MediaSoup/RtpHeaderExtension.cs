using System;
using System.Collections.Generic;
using System.Text;
using WebRTCme.Middleware.MediaStreamProxies.Enums.MediaSoup;

namespace WebRTCme.Middleware.MediaStreamProxies.Models.MediaSoup
{
    class RtpHeaderExtension
    {
        public MediaKind Kind { get; init; }
        public string Uri { get; init; }
        public int PreferedId { get; init; }
        public bool? PreferredEncrypt { get; init; }
        public Direction? Direction { get; set; }

    }
}
