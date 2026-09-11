using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme.Connection.MediaSoup
{
    /// <summary>
    /// The server's answer to <see cref="RestartIceRequest"/>.
    /// </summary>
    /// <remarks>
    /// Wrapped in its own member rather than being the response body, like the router capabilities
    /// and the newPeer notification. That wrapping has now caused two separate bugs in this client,
    /// so it is worth stating plainly: check the payload shape before deserialising into the type
    /// the member holds.
    /// </remarks>
    public class RestartIceResponse
    {
        public IceParameters IceParameters { get; init; }
    }
}
