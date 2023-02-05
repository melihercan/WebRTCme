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
    internal class RTCStatsReport : NativeBase, IRTCStatsReport
    {
        public RTCStatsReport(IJSRuntime jsRuntime, JsObjectRef jsObjectRef) : base(jsRuntime, jsObjectRef) { }

        public string Id 
        {
            get => GetNativeProperty<string>("id");
            set => SetNativeProperty("id", value);
        }
        
        public double Timestamp 
        {
            get => GetNativeProperty<double>("timestamp");
            set => SetNativeProperty("timestamp", value);
        }
        
        public RTCStatsType Type 
        {
            get => GetNativeProperty<RTCStatsType>("type");
            set => SetNativeProperty("type", value);
        }
    }
}
