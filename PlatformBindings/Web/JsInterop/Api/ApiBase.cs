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
    public abstract class ApiBase : INativeObjectAsync
    {
        protected ApiBase(IJSRuntime jsRuntime, JsObjectRef jsObjectRef = null)
        {
            JsRuntime = jsRuntime;
            NativeObject = jsObjectRef;
        }

        public IJSRuntime JsRuntime { get; }

        public object NativeObject { get; }
        
        public async ValueTask DisposeAsync()
        {
            if (NativeObject != null)
            {
                var jsObjectRef = NativeObject as JsObjectRef;
                await JsRuntime.DeleteJsObjectRef(jsObjectRef.JsObjectRefId);
            }
        }
    }
}
