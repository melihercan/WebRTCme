using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme.Connection.MediaSoup
{
    public class WebRtcTransportCreateRequest
    {
        public bool ForceTcp { get; init; }


        // The server takes the transport's direction in appData now, and enables SCTP
        // itself rather than negotiating it from the client's capabilities.
        public WebRtcTransportAppData AppData { get; init; }

    }
}
