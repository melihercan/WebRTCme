using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme.Middleware.MediaStreamProxies.Models.MediaSoup
{
    class WebRtcTransportCreateParameters
    {
        public bool ForceTcp { get; init; }
        public bool Producing { get; init; }
        public bool Consuming { get; init; }

        public SctpCapabilities SctpCapabilities { get; init; }

    }
}
