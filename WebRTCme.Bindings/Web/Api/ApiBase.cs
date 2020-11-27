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
    public abstract class ApiBase : INativeObject
    {
        protected ApiBase(IJSRuntime jsRuntime, JsObjectRef jsObjectRef = null)
        {
            JsRuntime = jsRuntime;
            NativeObject = jsObjectRef;
        }

        ////public bool IsNativeObjectsOwner { get; set; } = true;

        public IJSRuntime JsRuntime { get; }
        public object NativeObject { get; set; }

        //private object _selfNativeObject;
        //public object NativeObject// { get; protected set; }
        //{
        //  get => _selfNativeObject;
        //set
        //{
        //  _selfNativeObject = value;
        //        NativeObjects.Add(value);
        //}
        //}

        //public List<object> NativeObjects { get; set; } = new List<object>();

        //public void Dispose()
        //{
        //    foreach (var nativeObject in NativeObjects)
        //    {
        //        var jsObjectRef = nativeObject as JsObjectRef;
        //        JsRuntime.DeleteJsObjectRef(jsObjectRef.JsObjectRefId);
        //    }
        //}
    }
}
