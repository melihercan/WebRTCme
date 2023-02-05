using Microsoft.JSInterop;
using WebRTCme.Bindings.Blazor.Extensions;
using WebRTCme.Bindings.Blazor.Interops;
using WebRTCme.Platforms.Blazor.Custom;

namespace WebRTCme.Blazor
{
    internal class Blob : NativeBase, IBlob
    {
        public Blob(IJSRuntime jsRuntime, JsObjectRef jsObjectRef) : base(jsRuntime, jsObjectRef)
        { 
        }

        public int Size => GetNativeProperty<int>("size");

        public string Type => GetNativeProperty<string>("type");

        public async Task<byte[]> ArrayBuffer()
        {
            
            //var arrayBuffer = await JsRuntime.CallJsMethodAsync<object>(NativeObject, "arrayBuffer");
            //var uint8Array = new Uint8Array(arrayBuffer);
            //var bytes = uint8Array.ToArray();
            //return bytes;

            ////var arrayBufferJsObject = await JsRuntime.CallJsMethodAsync<JsObjectRef>(NativeObject, "arrayBuffer");
            ////var uint8JsObject = JsRuntime.CreateJsObject("window", "Uint8Array", arrayBufferJsObject);
            ////var bytes = JsRuntime.GetByteArray("Array", "from", uint8JsObject);
            //return null;
            ////return bytes;
            throw new NotImplementedException("TODO: How to get byte[] from JS ArrayBuffer???");

        }

        public IBlob Slice(int start = 0, int end = 0, string contentType = "")
        {
            if (end == 0) 
                end = Size;
            return new Blob(JsRuntime, JsRuntime.CallJsMethod<JsObjectRef>(NativeObject, "slice", start, end, contentType));
        }

        public async Task<string> Text()
        {
            ////var bytes = await ArrayBuffer();
            ////return Encoding.UTF8.GetString(bytes);
            ///
            return await JsRuntime.CallJsMethodAsync<string>(NativeObject, "text");
        }

        public object GetNativeObject()
        {
            return NativeObject;
        }
    }
}
