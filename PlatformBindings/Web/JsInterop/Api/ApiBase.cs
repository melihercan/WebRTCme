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
    public abstract class ApiBase : INativeObjects
    {
        protected ApiBase(IJSRuntime jsRuntime, JsObjectRef jsObjectRef = null)
        {
            JsRuntime = jsRuntime;
            SelfNativeObject = jsObjectRef;
        }

        public bool IsNativeObjectsOwner { get; set; } = true;

        public IJSRuntime JsRuntime { get; }

        private object _selfNativeObject;
        public object SelfNativeObject
        {
            get => _selfNativeObject;
            protected set
            {
                _selfNativeObject = value;
                NativeObjects.Add(value);
            }
        }

        public List<object> NativeObjects { get; set; } = new List<object>();

        public async ValueTask DisposeAsync()
        {
            foreach (var nativeObject in NativeObjects)
            {
                var jsObjectRef = nativeObject as JsObjectRef;
                JsRuntime.DeleteJsObjectRef(jsObjectRef.JsObjectRefId);
            }
        }
    }
}
