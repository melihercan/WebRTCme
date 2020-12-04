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
    internal class RTCCertificate : ApiBase, IRTCCertificate
    {

        internal static IRTCCertificate Create(IJSRuntime jsRuntime, JsObjectRef jsObjectRefCertificate)
        {
            return new RTCCertificate(jsRuntime, jsObjectRefCertificate);
        }

        private RTCCertificate() : base(null) { }

        private RTCCertificate(IJSRuntime jsRuntime, JsObjectRef jsObjectRef) : base(jsRuntime, jsObjectRef)
        {

        }

        public ulong Expires => throw new NotImplementedException();

    }
}
