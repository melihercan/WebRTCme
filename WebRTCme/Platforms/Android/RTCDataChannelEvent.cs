using System;
using WebRTCme;
using WebRTCme.Platforms.Android.Custom;
using Webrtc = Org.Webrtc;

namespace WebRTCme.Android
{
    internal class RTCDataChannelEvent : NativeBase<Webrtc.DataChannel>, IRTCDataChannelEvent
    {
        private readonly Webrtc.DataChannel _nativeDataChannel;

        public static IRTCDataChannelEvent Create(Webrtc.DataChannel nativeDataChannel) =>
            new RTCDataChannelEvent(nativeDataChannel);

        public RTCDataChannelEvent(Webrtc.DataChannel nativeDataChannel)
        {
            _nativeDataChannel = nativeDataChannel;
        }

        public IRTCDataChannel Channel => RTCDataChannel.Create(_nativeDataChannel);

    }
}