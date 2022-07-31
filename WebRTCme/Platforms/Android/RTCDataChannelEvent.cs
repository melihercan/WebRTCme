using System;
using WebRTCme;
using WebRTCme.Platforms.Android.Custom;
using Webrtc = Org.Webrtc;

namespace WebRTCme.Android
{
    internal class RTCDataChannelEvent : NativeBase<Webrtc.DataChannel>, IRTCDataChannelEvent
    {
        private readonly Webrtc.DataChannel _nativeDataChannel;

        public RTCDataChannelEvent(Webrtc.DataChannel nativeDataChannel)
        {
            _nativeDataChannel = nativeDataChannel;
        }

        public IRTCDataChannel Channel => new RTCDataChannel(_nativeDataChannel);

    }
}