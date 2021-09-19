using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme
{
    public class RTCConfiguration
    {
        public RTCBundlePolicy? BundlePolicy { get; set; }

        public IRTCCertificate[] Certificates { get; init; }

        public byte? IceCandidatePoolSize { get; set; }

        public RTCIceServer[] IceServers { get; set; }

        public RTCIceTransportPolicy? IceTransportPolicy { get; set; }

        public string PeerIdentity { get; set; }

        public RTCRtcpMuxPolicy? RtcpMuxPolicy { get; set; }

        public SdpSemantics? SdpSemantics { get; set; }
    }
}
