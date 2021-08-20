using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme.Connection.MediaSoup
{
    public class WebRtcTransportConnectParameters
    {
        public string TransportId { get; init; }
        public DtlsParameters DtlsParameters { get; init; }
    }
}
