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

        public RTCDataChannelEvent(Webrtc.RTCDataChannel nativeDataChannel)
        {
            _nativeDataChannel = nativeDataChannel;
        }

        public IRTCDataChannel Channel => new RTCDataChannel(_nativeDataChannel);
    }
}
