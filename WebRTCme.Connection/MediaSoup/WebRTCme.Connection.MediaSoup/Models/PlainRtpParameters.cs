using System;
using System.Collections.Generic;
using System.Text;
using Utilme.SdpTransform;

namespace WebRTCme.Connection.MediaSoup
{
    public class PlainRtpParameters
    {
        public string Ip { get; init; }
        public AddrType IpVersion { get; init; }
        public int Port { get; init; }
       
    }
}
