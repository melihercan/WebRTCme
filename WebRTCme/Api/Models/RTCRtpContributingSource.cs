using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme
{
    public class RTCRtpContributingSource
    {
        public double? AudioLevel { get; set; }

        public ulong? RtpTimestamp { get; set; } 

        public uint? Source { get; set; }

        public ulong? Timestamp { get; set; }

    }
}
