using Webrtc = Org.Webrtc;
using System;
using WebRTCme;
using WebRTCme.Platforms.Android.Custom;

namespace WebRTCme.Android
{
    internal class RTCPeerConnectionIceEvent : NativeBase<Webrtc.IceCandidate>, IRTCPeerConnectionIceEvent
    {
        private readonly Webrtc.IceCandidate _nativeIceCandidate;

        public static IRTCPeerConnectionIceEvent Create(Webrtc.IceCandidate nativeIceCandidate) =>
            new RTCPeerConnectionIceEvent(nativeIceCandidate);
        
        public RTCPeerConnectionIceEvent(Webrtc.IceCandidate nativeIceCandidate) =>
            _nativeIceCandidate = nativeIceCandidate;

        public IRTCIceCandidate Candidate => 
            RTCIceCandidate.Create(_nativeIceCandidate);

    }
}