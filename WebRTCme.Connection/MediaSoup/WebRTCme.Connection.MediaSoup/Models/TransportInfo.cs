using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme.Connection.MediaSoup
{
    public class TransportInfo
    {
        public string TransportId { get; init; }
        public IceParameters IceParameters { get; init; }

        public IceCandidate[] IceCandidates { get; init; }

        public DtlsParameters DtlsParameters { get; init; }

        public SctpParameters SctpParameters { get; init; }

    }
}
