using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using WebRtcBindingsWeb.Interops;
using WebRTCme;

namespace WebRtcBindingsWeb.Api
{
    internal class RTCRtpReceiver : ApiBase, IRTCRtpReceiver
    {

        internal static IRTCRtpReceiver Create(IJSRuntime jsRuntime, JsObjectRef jsObjectRefRtpReceiver) => 
            new RTCRtpReceiver(jsRuntime, jsObjectRefRtpReceiver);

        private RTCRtpReceiver() : base(null) { }

        private RTCRtpReceiver(IJSRuntime jsRuntime, JsObjectRef jsObjectRef) : base(jsRuntime, jsObjectRef) { }


    }
}
