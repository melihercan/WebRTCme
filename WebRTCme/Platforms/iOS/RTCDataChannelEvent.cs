using System;
using System.Collections.Generic;
using System.Text;
using WebRTCme;
using WebRTCme.Platforms.iOS.Custom;

namespace WebRTCme.iOS
{
    internal class RTCDataChannelEvent : NativeBase<Webrtc.RTCDataChannel>, IRTCDataChannelEvent
    {
        private readonly Webrtc.RTCDataChannel _nativeDataChannel;

        public static IRTCDataChannelEvent Create(Webrtc.RTCDataChannel nativeDataChannel) => 
            new RTCDataChannelEvent(nativeDataChannel);

        private RTCDataChannelEvent(Webrtc.RTCDataChannel nativeDataChannel)
        {
            _nativeDataChannel = nativeDataChannel;
        }

        public IRTCDataChannel Channel => RTCDataChannel.Create(_nativeDataChannel);
    }
}
