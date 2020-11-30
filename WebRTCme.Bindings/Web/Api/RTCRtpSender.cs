using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebRtcJsInterop.Interops;
using WebRTCme;

namespace WebRtcJsInterop.Api
{
    internal class RTCRtpSender : ApiBase, IRTCRtpSender
    {
        private RTCRtpSender(IJSRuntime jsRuntime, JsObjectRef jsObjectRef) : base(jsRuntime, jsObjectRef) { }


        internal static IRTCRtpSender Create(IJSRuntime jsRuntime, JsObjectRef jsObjectRefRtcRtpSender)
        {
            var rtcRtpSender = new RTCRtpSender(jsRuntime, jsObjectRefRtcRtpSender);
            return rtcRtpSender;
        }

        public void DeleteMe()
        {
            throw new NotImplementedException();
        }
    }
}
