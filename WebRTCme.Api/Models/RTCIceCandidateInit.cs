using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme
{
    public record RTCIceCandidateInit
    {
        public string Candidate { get; init; }

        public string SdpMid { get; init; }

        public ushort? SdpMLineIndex { get; init; }

        public string UsernameFragment { get; set; }

    }
}
