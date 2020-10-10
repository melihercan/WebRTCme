using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebRtcJsInterop.Extensions;
using WebRtcJsInterop.Interops;
using WebRTCme;

namespace WebRtcJsInterop.Api
{
    internal class RTCPeerConnection : BaseApi, IRTCPeerConnection
    {
        private RTCPeerConnection(IJSRuntime jsRuntime, JsObjectRef jsObjectRef) : base(jsRuntime, jsObjectRef) { }

        public async ValueTask DisposeAsync()
        {
            await DisposeBaseAsync();
        }

        internal static async Task<IRTCPeerConnection> New(IJSRuntime jsRuntime)
        {
            var jsObjectRef = await jsRuntime.CreateJsObject(null, "RTCPeerConnection", new object());
            var rtcPeerConnection = new RTCPeerConnection(jsRuntime, jsObjectRef);
            return rtcPeerConnection;
        }
    }
}
