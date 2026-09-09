using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme.Connection.MediaSoup
{
    public class HandlerSendResult
    {
        public string LocalId { get; init; }
        public RtpParameters RtpParameters { get; init; }
        public IRTCRtpSender RtpSender { get; init; }
    }
}
