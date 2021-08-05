﻿using System;
using System.Collections.Generic;
using System.Text;
using WebRTCme.Connection.MediaSoup.Proxy.Models;

namespace WebRTCme.Connection.MediaSoup
{
    public class RtpCodecCapability
    {
        public MediaKind Kind { get; set; }
        public string MimeType { get; init; }
        public int PreferredPayloadType { get; init; }
        public int ClockRate { get; init; }
        public int? Channels { get; set; }
        public object Parameters { get; set; }
        public RtcpFeedback[] RtcpFeedback { get; set; }
    }
}
