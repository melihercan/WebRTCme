using System;
using System.Collections.Generic;
using System.Text;
using WebRTCme;
using WebRTCme.Platforms.iOS.Custom;

namespace WebRTCme.iOS
{
    internal class RTCPeerConnectionIceErrorEvent : NativeBase<Webrtc.RTCIceCandidateErrorEvent>,
        IRTCPeerConnectionIceErrorEvent
    {
        public static IRTCPeerConnectionIceErrorEvent Create(Webrtc.RTCIceCandidateErrorEvent nativeEvent) =>
            new RTCPeerConnectionIceErrorEvent(nativeEvent);

        public RTCPeerConnectionIceErrorEvent(Webrtc.RTCIceCandidateErrorEvent nativeEvent) : base(nativeEvent)
        { }

        public string Address => NativeObject.Address;

        public ushort? Port => (ushort)NativeObject.Port;

        public string Url => NativeObject.Url;

        public ushort ErrorCode => (ushort)NativeObject.ErrorCode;

        public string ErrorText => NativeObject.ErrorText;
    }
}
