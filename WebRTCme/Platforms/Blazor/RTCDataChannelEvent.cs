using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebRTCme.Bindings.Blazor.Interops;
using WebRTCme.Bindings.Blazor.Extensions;
using WebRTCme;
using WebRTCme.Platforms.Blazor.Custom;

namespace WebRTCme.Blazor
{
    internal class RTCDataChannelEvent : NativeBase, IRTCDataChannelEvent
    {
        public static IRTCDataChannelEvent Create(IJSRuntime jsRuntime, JsObjectRef jsObjectRefRtcStatsReport) => 
            new RTCDataChannelEvent(jsRuntime, jsObjectRefRtcStatsReport);

        public RTCDataChannelEvent(IJSRuntime jsRuntime, JsObjectRef jsObjectRef) : base(jsRuntime, jsObjectRef) { }

        public IRTCDataChannel Channel => 
            new RTCDataChannel(JsRuntime, JsRuntime.GetJsPropertyObjectRef(NativeObject, "channel"));
    }
}
