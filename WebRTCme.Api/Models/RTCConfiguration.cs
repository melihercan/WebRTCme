using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace WebRTCme
{
    public class RTCConfiguration
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public RTCBundlePolicy? BundlePolicy { get; set; }
        
        public IRTCCertificate[] Certificates { get; set; }

        public byte? /*ushort?*/ IceCandidatePoolSize { get; set; }
        
        public RTCIceServer[] IceServers { get; set; }
        
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public RTCIceTransportPolicy? IceTransportPolicy { get; set; }
        
        public string PeerIdentity { get; set; }
        
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public RTCRtcpMuxPolicy? RtcpMuxPolicy { get; set; }
    }
}
