using System;
using System.Collections.Generic;
using System.Text;
using WebRTCme;
using WebRTCme.Platforms.MacCatalyst.Custom;

namespace WebRTCme.MacCatalyst
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
