using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebRTCme.Bindings.Blazor.Interops;
using WebRTCme;

namespace WebRTCme.Bindings.Blazor.Api
{
    internal class RTCStatsReport : ApiBase, IRTCStatsReport
    {
        internal static IRTCStatsReport Create(IJSRuntime jsRuntime, JsObjectRef jsObjectRefRtcStatsReport) => 
            new RTCStatsReport(jsRuntime, jsObjectRefRtcStatsReport);

        private RTCStatsReport(IJSRuntime jsRuntime, JsObjectRef jsObjectRef) : base(jsRuntime, jsObjectRef) { }

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
