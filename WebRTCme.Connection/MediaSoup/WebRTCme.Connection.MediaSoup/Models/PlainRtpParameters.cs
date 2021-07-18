using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme.Connection.MediaSoup
{
    public class PlainRtpParameters
    {
        public string Ip { get; init; }
        public IpVersion IpVersion { get; init; }

        public int Port { get; init; }
       
    }
}
