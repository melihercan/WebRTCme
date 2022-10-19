using Webrtc = Org.Webrtc;
using System;
using WebRTCme;
using WebRTCme.Platforms.Android.Custom;

namespace WebRTCme.Android
{
    internal class RTCPeerConnectionIceEvent : NativeBase<Webrtc.IceCandidate>, IRTCPeerConnectionIceEvent
    {
        private readonly Webrtc.IceCandidate _nativeIceCandidate;

        public RTCPeerConnectionIceEvent(Webrtc.IceCandidate nativeIceCandidate) =>
            _nativeIceCandidate = nativeIceCandidate;

        public IRTCIceCandidate Candidate => 
            new RTCIceCandidate(_nativeIceCandidate);

    }
}