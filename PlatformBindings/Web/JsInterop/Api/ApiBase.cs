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
    public abstract class ApiBase : INativeObjectsAsync
    {
        protected ApiBase(IJSRuntime jsRuntime, JsObjectRef jsObjectRef = null)
        {
            JsRuntime = jsRuntime;
            SelfNativeObject = jsObjectRef;
        }

        public IJSRuntime JsRuntime { get; }

        public object SelfNativeObject { get; }
        public List<object> OtherNativeObjects { get; set; } = new List<object>();

        public async ValueTask DisposeAsync()
        {
            foreach (var otherNativeObject in OtherNativeObjects)
            {
                var jsObjectRef = otherNativeObject as JsObjectRef;
                await JsRuntime.DeleteJsObjectRef(jsObjectRef.JsObjectRefId);
            }

            if (SelfNativeObject != null)
            {
                var jsObjectRef = SelfNativeObject as JsObjectRef;
                await JsRuntime.DeleteJsObjectRef(jsObjectRef.JsObjectRefId);
            }
        }
    }
}
