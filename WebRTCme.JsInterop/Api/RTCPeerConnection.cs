using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebRtcJsInterop.Extensions;
using WebRtcJsInterop.Interops;
using WebRTCme;

namespace WebRtcJsInterop
{
    internal class RTCPeerConnection : IRTCPeerConnection
    {
        private readonly IJSRuntime _jsRuntime;
        private readonly JsObjectRef _jsObjectRef;

        private RTCPeerConnection(IJSRuntime jsRuntime, JsObjectRef jsObjectRef)
        {
            _jsRuntime = jsRuntime;
            _jsObjectRef = jsObjectRef;
        }

        public ValueTask DisposeAsync()
        {
            throw new NotImplementedException();
        }

        internal static async Task<IRTCPeerConnection> New(IJSRuntime jsRuntime)
        {
            var jsObjectRef = await jsRuntime.CreateJsObject(null, "RTCPeerConnection", new object());
            var rtcPeerConnection = new RTCPeerConnection(jsRuntime, jsObjectRef);
            return rtcPeerConnection;
        }
    }
}
