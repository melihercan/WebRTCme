using System;
using System.Collections.Generic;
using System.Text;
using WebRTCme;

namespace WebRTCme.iOS
{
    internal class RTCPeerConnectionIceEvent : ApiBase, IRTCPeerConnectionIceEvent
    {
        private readonly Webrtc.RTCIceCandidate _nativeIceCandidate;

        public static IRTCPeerConnectionIceEvent Create(Webrtc.RTCIceCandidate nativeIceCandidate) => 
            new RTCPeerConnectionIceEvent(nativeIceCandidate);

        private RTCPeerConnectionIceEvent(Webrtc.RTCIceCandidate nativeIceCandidate)
        {
            _nativeIceCandidate = nativeIceCandidate;
        }

        public IRTCIceCandidate Candidate =>
            RTCIceCandidate.Create(_nativeIceCandidate);

    }
}
