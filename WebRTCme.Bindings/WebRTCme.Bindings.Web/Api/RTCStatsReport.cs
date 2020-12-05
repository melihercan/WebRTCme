using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebRtcBindingsWeb.Interops;
using WebRTCme;

namespace WebRtcBindingsWeb.Api
{
    internal class RTCStatsReport : ApiBase, IRTCStatsReport
    {
        internal static IRTCStatsReport Create(IJSRuntime jsRuntime, JsObjectRef jsObjectRefRtcStatsReport) => 
            new RTCStatsReport(jsRuntime, jsObjectRefRtcStatsReport);

        private RTCStatsReport(IJSRuntime jsRuntime, JsObjectRef jsObjectRef) : base(jsRuntime, jsObjectRef) { }


        public string Id { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public double Timestamp { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public RTCStatsType Type { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }



    }
}
