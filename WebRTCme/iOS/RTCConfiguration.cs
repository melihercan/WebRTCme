using System;
using System.Collections.Generic;
using System.Text;
using WebRTCme;

namespace WebRtc.iOS
{
    internal class RTCConfiguration : ApiBase, IRTCConfiguration
    {
        public static IRTCConfiguration Create() => 
            new RTCConfiguration(new Webrtc.RTCConfiguration());

        public static IRTCConfiguration Create(Webrtc.RTCConfiguration nativeConfiguration) => 
            new RTCConfiguration(nativeConfiguration);

        private RTCConfiguration(Webrtc.RTCConfiguration nativeConfiguration) : base(nativeConfiguration) { }

        public RTCBundlePolicy? BundlePolicy { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public IRTCCertificate[] Certificates { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public byte? IceCandidatePoolSize { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public RTCIceServer[] IceServers { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public RTCIceTransportPolicy? IceTransportPolicy { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string PeerIdentity { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public RTCRtcpMuxPolicy? RtcpMuxPolicy { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    }
}
