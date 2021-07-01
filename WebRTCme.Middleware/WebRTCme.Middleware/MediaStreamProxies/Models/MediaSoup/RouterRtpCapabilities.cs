using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme.Middleware.MediaStreamProxies.Models.MediaSoup
{
    class RouterRtpCapabilities
    {
        public RtpCodecCapability[] Codecs { get; init; }
        public RtpHeaderExtension[] HeaderExtensions { get; init; }
    }
}
