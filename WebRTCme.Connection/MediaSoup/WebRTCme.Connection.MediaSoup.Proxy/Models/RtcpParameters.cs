using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme.Connection.MediaSoup
{
    public class RtcpParameters
    {
        public string Cname { get; set; }
        public bool ReducedSize { get; init; }
        public bool Mux { get; init; }
    }
}
