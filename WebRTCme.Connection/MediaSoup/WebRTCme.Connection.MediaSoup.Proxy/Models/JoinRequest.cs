using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme.Connection.MediaSoup.Proxy.Models
{
    public class JoinRequest
    {
        public string DisplayName { get; init; }
        public Device Device { get; init; }
        public RtpCapabilities RtpCapabilities { get; init; }
        public SctpCapabilities SctpCapabilities { get; init; }
    }
}
