using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebRtcBindingsBlazor.Interops;
using WebRtcBindingsBlazor.Extensions;
using WebRTCme;

namespace WebRtcBindingsBlazor.Api
{
    internal class RTCDataChannelEvent : ApiBase, IRTCDataChannelEvent
    {
        public static IRTCDataChannelEvent Create(IJSRuntime jsRuntime, JsObjectRef jsObjectRefRtcStatsReport) => 
            new RTCDataChannelEvent(jsRuntime, jsObjectRefRtcStatsReport);

        private RTCDataChannelEvent(IJSRuntime jsRuntime, JsObjectRef jsObjectRef) : base(jsRuntime, jsObjectRef) { }

        public IRTCDataChannel Channel => 
            RTCDataChannel.Create(JsRuntime, JsRuntime.GetJsPropertyObjectRef(NativeObject, "channel"));
    }
}
