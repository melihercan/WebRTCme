using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebRtcBindingsWeb.Interops;
using WebRTCme;

namespace WebRtcBindingsWeb.Api
{
    internal class RTCRtpTransceiver : ApiBase, IRTCRtpTransceiver
    {
        internal static IRTCRtpTransceiver Create(IJSRuntime jsRuntime, JsObjectRef jsObjectRefRtpTransceiver) => 
            new RTCRtpTransceiver(jsRuntime, jsObjectRefRtpTransceiver);

        private RTCRtpTransceiver() : base(null) { }

        private RTCRtpTransceiver(IJSRuntime jsRuntime, JsObjectRef jsObjectRef) : base(jsRuntime, jsObjectRef) { }



    }
}
