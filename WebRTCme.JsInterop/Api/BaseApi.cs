using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebRtcJsInterop.Extensions;
using WebRtcJsInterop.Interops;

namespace WebRtcJsInterop.Api
{
    public abstract class BaseApi 
    {
        protected IJSRuntime JsRuntime { get; }
        protected JsObjectRef JsObjectRef { get; }

        protected BaseApi(IJSRuntime jsRuntime, JsObjectRef jsObjectRef)
        {
            JsRuntime = jsRuntime;
            JsObjectRef = jsObjectRef;
        }

        protected async ValueTask DisposeBaseAsync()
        {
            if (JsObjectRef != null)
            {
                await JsRuntime.DeleteJsObject(JsObjectRef.JsObjectRefId);
            }
        }
    }
}
