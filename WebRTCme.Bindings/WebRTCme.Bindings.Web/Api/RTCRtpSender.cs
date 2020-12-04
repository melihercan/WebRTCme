using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebRtcBindingsWeb.Interops;
using WebRTCme;

namespace WebRtcBindingsWeb.Api
{
    internal class RTCRtpSender : ApiBase, IRTCRtpSender
    {
        internal static IRTCRtpSender Create(IJSRuntime jsRuntime, JsObjectRef jsObjectRefRtcRtpSender) => 
            new RTCRtpSender(jsRuntime, jsObjectRefRtcRtpSender);

        private RTCRtpSender() : base(null) { }

        private RTCRtpSender(IJSRuntime jsRuntime, JsObjectRef jsObjectRef) : base(jsRuntime, jsObjectRef) { }



    }
}
