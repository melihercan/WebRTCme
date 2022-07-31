using System;
using System.Collections.Generic;
using System.Text;
using WebRTCme;
using WebRTCme.Platforms.iOS.Custom;

namespace WebRTCme.iOS
{
    internal class RTCPeerConnectionIceEvent : NativeBase<Webrtc.RTCIceCandidate>, IRTCPeerConnectionIceEvent
    {
        private readonly Webrtc.RTCIceCandidate _nativeIceCandidate;

        public RTCPeerConnectionIceEvent(Webrtc.RTCIceCandidate nativeIceCandidate)
        {
            _nativeIceCandidate = nativeIceCandidate;
        }

        public IRTCIceCandidate Candidate =>
            new RTCIceCandidate(_nativeIceCandidate);

    }
}
