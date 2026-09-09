using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme.Connection.MediaSoup
{
    public class HandlerReceiveOptions
    {
        public string TrackId { get; init; }
        public MediaKind Kind { get; init; }
        public RtpParameters RtpParameters { get; init; }
    }
}
