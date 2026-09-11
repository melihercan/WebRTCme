using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme.Connection.MediaSoup
{
    /// <summary>
    /// Asks the server to gather a fresh set of ICE candidates for one transport.
    /// </summary>
    /// <remarks>
    /// A request, not a notification - the new ICE parameters come back in the response and the
    /// client cannot restart its own side without them.
    /// </remarks>
    public class RestartIceRequest
    {
        public string TransportId { get; init; }
    }
}
