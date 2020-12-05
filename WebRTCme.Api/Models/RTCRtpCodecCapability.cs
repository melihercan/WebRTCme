using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme
{
    public class RTCRtpCodecCapability
    {
        public ushort? Channels { get; set; }

        public ulong ClockRate { get; }

        public string MimeType { get; set; }

        public string SdpFmtpLine { get; set; }
    }
}
