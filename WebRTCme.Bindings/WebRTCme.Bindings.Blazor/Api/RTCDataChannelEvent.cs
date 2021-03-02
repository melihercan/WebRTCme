using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebRtcMeBindingsBlazor.Interops;
using WebRtcMeBindingsBlazor.Extensions;
using WebRTCme;

namespace WebRtcMeBindingsBlazor.Api
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
