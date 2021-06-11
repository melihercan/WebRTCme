using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebRTCme.Bindings.Blazor.Extensions;
using WebRTCme.Bindings.Blazor.Interops;

namespace WebRTCme.Bindings.Blazor.Api
{
    internal class Blob : ApiBase, IBlob
    {
        //public static IBlob Create(IJSRuntime jsRuntime, byte[] array, BlobPropertyBag options)
        //{
        //    var jsObjectRef = jsRuntime.CreateJsObject("window", "Blob", array, options);
        //    return new Blob(jsRuntime, jsObjectRef);
        //}


        //public static IBlob Create(IJSRuntime jsRuntime, string[] array, BlobPropertyBag options)
        //{
        //    var jsObjectRef = jsRuntime.CreateJsObject("window", "Blob", array, options);
        //    return new Blob(jsRuntime, jsObjectRef);
        //}

        public static IBlob Create(IJSRuntime jsRuntime, JsObjectRef nativeBlobJsObjectRef)
        {
            return new Blob(jsRuntime, nativeBlobJsObjectRef);
        }


        private Blob(IJSRuntime jsRuntime, JsObjectRef jsObjectRef) : base(jsRuntime, jsObjectRef)
        { 
        }


        public int Size => GetNativeProperty<int>("size");

        public string Type => GetNativeProperty<string>("type");

        public async Task<byte[]> ArrayBuffer()
        {
            return await JsRuntime.CallJsMethodAsync<byte[]>(NativeObject, "arrayBuffer");
        }

        public IBlob Slice(int start = 0, int end = 0, string contentType = "")
        {
            if (end == 0) 
                end = Size;
            return Create(JsRuntime, JsRuntime.CallJsMethod<JsObjectRef>(NativeObject, "slice", start, end, contentType));
        }

        public async Task<string> Text()
        {
            ////var bytes = await ArrayBuffer();
            ////return Encoding.UTF8.GetString(bytes);
            ///
            return await JsRuntime.CallJsMethodAsync<string>(NativeObject, "text");
        }
    }
}
