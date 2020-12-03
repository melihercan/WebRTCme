using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebRtcBindingsWeb.Extensions;
using WebRtcBindingsWeb.Interops;
using WebRTCme;

namespace WebRtcBindingsWeb.Api
{
    public abstract class ApiBase : INativeObject
    {
        protected ApiBase(IJSRuntime jsRuntime, JsObjectRef jsObjectRef = null)
        {
            JsRuntime = jsRuntime;
            NativeObject = jsObjectRef;
        }

        public IJSRuntime JsRuntime { get; }
        
////        public object NativeObject { get; set; }

        private object _nativeObject;
        public object NativeObject// { get; protected set; }
        {
            get => _nativeObject;
            set
            { 
                _nativeObject = value;
                JsObjects.Add(value as JsObjectRef);
            }
        }

        public List<JsObjectRef> JsObjects { get; set; } = new List<JsObjectRef>();

        public List<IDisposable> JsEvents { get; set; } = new List<IDisposable>();
        

        public void Dispose()
        {
            ////            if (NativeObject != null)
            ////        {
            ////        var jsObjectRef = NativeObject as JsObjectRef;
            ////                JsRuntime.DeleteJsObjectRef(jsObjectRef.JsObjectRefId);
            ////        }
            foreach (var jsEvent in JsEvents)
            {
                jsEvent.Dispose();
            }
            foreach (var jsObjectRef in JsObjects)
            {
                JsRuntime.DeleteJsObjectRef(jsObjectRef.JsObjectRefId);
            }

        }
    }
}
