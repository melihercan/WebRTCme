using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace WebRTCme
{
    public class RTCConfiguration
    {
        public RTCBundlePolicy? BundlePolicy { get; set; }
        
        public IRTCCertificate[] Certificates { get; set; }

        public byte? /*ushort?*/ IceCandidatePoolSize { get; set; }
        
        public RTCIceServer[] IceServers { get; set; }
        
        public RTCIceTransportPolicy? IceTransportPolicy { get; set; }
        
        public string PeerIdentity { get; set; }
        
        public RTCRtcpMuxPolicy? RtcpMuxPolicy { get; set; }
    }
}
