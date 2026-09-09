using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme.Connection.MediaSoup
{
    /// <summary>
    /// What a WebRTC transport is for, which is how the server decides which router and
    /// WebRTC server to put it on.
    /// </summary>
    public class WebRtcTransportAppData
    {
        public string Direction { get; init; }
    }
}
