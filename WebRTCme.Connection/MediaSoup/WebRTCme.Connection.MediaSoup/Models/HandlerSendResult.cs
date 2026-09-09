using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme.Connection.MediaSoup.Proxy.Models
{
    public class HandlerSendResult
    {
        public string LocalId { get; init; }
        public RtpParameters RtpParameters { get; init; }
        public IRTCRtpSender RtpSender { get; init; }
    }
}
