using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebRtcJsInterop.Interops;
using WebRTCme;

namespace WebRtcJsInterop.Api
{
    internal class RTCRtpSender : BaseApi, IRTCRtpSender
    {
        private RTCRtpSender(IJSRuntime jsRuntime, JsObjectRef jsObjectRef) : base(jsRuntime, jsObjectRef) { }


        public ValueTask DisposeAsync()
        {
            throw new NotImplementedException();
        }

        internal static IRTCRtpSender New(IJSRuntime jsRuntime, JsObjectRef jsObjectRefRtcRtpSender)
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
