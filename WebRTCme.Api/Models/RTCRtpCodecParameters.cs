using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme
{
    public class RTCRtpCodecParameters
    {
        public byte? PayloadType { get; }

        public string MimeType { get; }

        public ulong? ClockRate { get; }

        public ushort? Channels { get; }

        public string SdpFmtpLine { get; }
    }
}
