using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme.Connection.MediaSoup
{
    public class RtcpFeedback
    {
        public string Type { get; init; }
        public string Parameter { get; set; }
    }
}
