using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme.Connection.MediaSoup
{
    public class HanderReceiveOptions
    {
        public string TrackId { get; init; }
        public MediaKind Kind { get; init; }
        public RtpParameters RtpParameters { get; init; }
    }
}
