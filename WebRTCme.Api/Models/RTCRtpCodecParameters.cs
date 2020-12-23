using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme
{
    public class RTCRtpCodecParameters
    {
        public byte? PayloadType { get; init; }

        public string MimeType { get; init; }

        public ulong? ClockRate { get; init; }

        public ushort? Channels { get; init; }

        public string SdpFmtpLine { get; init; }
    }
}
