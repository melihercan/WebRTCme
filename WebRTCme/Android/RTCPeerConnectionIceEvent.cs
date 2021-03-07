using Webrtc = Org.Webrtc;
using System;
using WebRTCme;

namespace WebRTCme.Android
{
    internal class RTCPeerConnectionIceEvent : ApiBase, IRTCPeerConnectionIceEvent
    {
        private readonly Webrtc.IceCandidate _nativeIceCandidate;

        public static IRTCPeerConnectionIceEvent Create(Webrtc.IceCandidate nativeIceCandidate) =>
            new RTCPeerConnectionIceEvent(nativeIceCandidate);
        
        private RTCPeerConnectionIceEvent(Webrtc.IceCandidate nativeIceCandidate) =>
            _nativeIceCandidate = nativeIceCandidate;

        public IRTCIceCandidate Candidate => 
            RTCIceCandidate.Create(_nativeIceCandidate);

    }
}