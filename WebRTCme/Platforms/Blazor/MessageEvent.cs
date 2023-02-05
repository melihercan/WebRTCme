using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebRTCme.Bindings.Blazor.Interops;
using WebRTCme;
using WebRTCme.Platforms.Blazor.Custom;

namespace WebRTCme.Blazor
{
    internal class MessageEvent : NativeBase, IMessageEvent
    {
        public static IMessageEvent Create(IJSRuntime jsRuntime, JsObjectRef jsObjectRefRtcStatsReport) => 
            new MessageEvent(jsRuntime, jsObjectRefRtcStatsReport);

        public MessageEvent(IJSRuntime jsRuntime, JsObjectRef jsObjectRef) : base(jsRuntime, jsObjectRef) { }

        public object Data => GetNativeProperty<string>("data");

        public string Origin => GetNativeProperty<string>("origin");

        public string LastEventId => GetNativeProperty<string>("lastEventId");

        public object Source => throw new NotImplementedException();

        public object[] Ports => throw new NotImplementedException();
    }
}
