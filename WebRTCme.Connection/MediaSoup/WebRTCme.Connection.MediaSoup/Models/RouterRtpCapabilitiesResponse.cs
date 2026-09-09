using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme.Connection.MediaSoup
{
    /// <summary>
    /// The envelope the router's capabilities arrive in.
    /// </summary>
    public class RouterRtpCapabilitiesResponse
    {
        public RtpCapabilities RouterRtpCapabilities { get; init; }
    }
}
