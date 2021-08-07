using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme.Connection.MediaSoup
{
    public class TransportOptions
    {
        public string Id { get; init; }
        public IceParameters IceParameters { get; init; }

        public IceCandidate[] IceCandidates { get; init; }

        public DtlsParameters DtlsParameters { get; init; }

        public SctpParameters SctpParameters { get; init; }
        
        public RTCIceServer[] IceServers { get; init; }

        public RTCIceTransportPolicy? IceTransportPolicy { get; init; }

        public object AdditionalSettings { get; init; }
        
        public object ProprietaryConstraints { get; init; }

        public Dictionary<string, object> AppData { get; init; }
    }
}
