using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme.Connection.MediaSoup
{
    public class HandlerRunOptions
    {
        public InternalDirection Direction { get; init; }
        public IceParameters IceParameters { get; init; }
        public IceCandidate[] iceCandidates { get; init; }
        public DtlsParameters DtlsParameters { get; init; }
        SctpParameters SctpParameters { get; init; }
        RTCIceServer[] RTCIceServers { get; init; }
        RTCIceTransportPolicy IceTransportPolicy { get; init; }
        object AdditionalSettings { get; init; }
        object ProprietaryConstraints { get; init; }
        ExtendedRtpCapabilities ExtendedRtpCapabilities { get; init; }

    }
}
