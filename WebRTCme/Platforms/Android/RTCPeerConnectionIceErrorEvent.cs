using Webrtc = Org.Webrtc;
using System;
using WebRTCme;
using WebRTCme.Platforms.Android.Custom;

namespace WebRTCme.Android
{
    internal class RTCPeerConnectionIceErrorEvent : NativeBase<Webrtc.IceCandidateErrorEvent>,
        IRTCPeerConnectionIceErrorEvent
    {
        public void Dispose() { }

        public static IRTCPeerConnectionIceErrorEvent Create(Webrtc.IceCandidateErrorEvent nativeEvent) =>
            new RTCPeerConnectionIceErrorEvent(nativeEvent);

        public RTCPeerConnectionIceErrorEvent(Webrtc.IceCandidateErrorEvent nativeEvent) : base(nativeEvent)
        { }

        public string Address => NativeObject.Address;

        public ushort? Port => (ushort)NativeObject.Port;

        public string Url => NativeObject.Url;

        public ushort ErrorCode => (ushort)NativeObject.ErrorCode;

        public string ErrorText => NativeObject.ErrorText;
    }
}
