using System;
using WebRTCme;
using Webrtc = Org.Webrtc;

namespace WebRTCme.Android
{
    internal class RTCDataChannelEvent : ApiBase, IRTCDataChannelEvent
    {
        private readonly Webrtc.DataChannel _nativeDataChannel;

        public static IRTCDataChannelEvent Create(Webrtc.DataChannel nativeDataChannel) =>
            new RTCDataChannelEvent(nativeDataChannel);

        private RTCDataChannelEvent(Webrtc.DataChannel nativeDataChannel)
        {
            _nativeDataChannel = nativeDataChannel;
        }

        public IRTCDataChannel Channel => RTCDataChannel.Create(_nativeDataChannel);

    }
}