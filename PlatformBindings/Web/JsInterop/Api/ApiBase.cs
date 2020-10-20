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
    public abstract class ApiBase : INativeObjectAsync<JsObjectRef>
    {
        protected ApiBase(IJSRuntime jsRuntime,  JsObjectRef jsObjectRef = null)
        {
            JsRuntime = jsRuntime;
            NativeObject = jsObjectRef;
        }

        public IJSRuntime JsRuntime { get; }

        public JsObjectRef NativeObject { get; }


        public async ValueTask DisposeAsync()
        {
            if (NativeObject != null)
            {
                await JsRuntime.DeleteJsObjectRef(NativeObject.JsObjectRefId);
            }
        }
    }
}
