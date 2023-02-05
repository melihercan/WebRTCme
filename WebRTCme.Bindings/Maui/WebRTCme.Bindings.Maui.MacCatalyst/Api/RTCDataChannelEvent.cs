﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebRTCme.Bindings.Maui.MacCatalyst.Custom;

namespace WebRTCme.Bindings.Maui.MacCatalyst.Api
{
    internal class RTCDataChannelEvent : NativeBase<SIPSorcery.Net.RTCDataChannel>, IRTCDataChannelEvent
    {
        private readonly SIPSorcery.Net.RTCDataChannel _nativeDataChannel;

        public RTCDataChannelEvent(SIPSorcery.Net.RTCDataChannel nativeDataChannel)
        {
            _nativeDataChannel = nativeDataChannel;
        }

        public IRTCDataChannel Channel => new RTCDataChannel(_nativeDataChannel);

        public void Dispose()
        {
        }
    }
}