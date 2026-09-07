using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme
{
    public class RTCRtpCodec
    {
        public string MimeType { get; init; }

        public ulong? ClockRate { get; init; }

        public ushort? Channels { get; init; }

        public string SdpFmtpLine { get; init; }
    }
}
