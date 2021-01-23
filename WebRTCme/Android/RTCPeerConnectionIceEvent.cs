using Webrtc = Org.Webrtc;
using System;
using WebRTCme;

namespace WebRtc.Android
{
    internal class RTCPeerConnectionIceEvent : ApiBase, IRTCPeerConnectionIceEvent
    {
        private readonly Webrtc.IceCandidate _nativeIceCandidate;

        public static IRTCPeerConnectionIceEvent Create(Webrtc.IceCandidate nativeIceCandidate) =>
            new RTCPeerConnectionIceEvent(nativeIceCandidate);

        private RTCPeerConnectionIceEvent(Webrtc.IceCandidate nativeIceCandidate) =>
            _nativeIceCandidate = nativeIceCandidate;

        // 'null' is valid and indicates end of ICE gathering process.
        public IRTCIceCandidate Candidate => _nativeIceCandidate == null ? null : 
            RTCIceCandidate.Create(_nativeIceCandidate);

    }
}