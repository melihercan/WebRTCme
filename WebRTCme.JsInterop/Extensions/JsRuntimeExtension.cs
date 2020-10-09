using Microsoft.JSInterop;
using System;
using System.Linq;
using System.Threading.Tasks;
using WebRtcJsInterop.Interops;

namespace WebRtcJsInterop.Extensions
{
    // If parentObject is null, 'window' will be used as root object on JS side.

    public static class JsRuntimeExtension
    {
        public static async ValueTask<JsObjectRef> CreateJsObject(this IJSRuntime jsRuntime, JsObjectRef parentObject, 
            string interface_, params object[] args)
        {
            var jsObjectRef = await jsRuntime.InvokeAsync<JsObjectRef>(
                "webRtcInterop.createObject",
                new object[]
                {
                    parentObject,
                    interface_,
                }.Concat(args).ToArray()).ConfigureAwait(false);
            return jsObjectRef;
        }

        public static async ValueTask DeleteJsObject(this IJSRuntime jsRuntime,  int id)
        {
            await jsRuntime.InvokeAsync<object>(
                "webRtcInterop.deleteObject",
                new object[]
                {
                    id,
                }).ConfigureAwait(false);
        }

        public static async ValueTask<JsObjectRef> GetJsProperty(this IJSRuntime jsRuntime, JsObjectRef parentObject,
            string property)
        {
            var jsObjectRef = await jsRuntime.InvokeAsync<JsObjectRef>(
                "webRtcInterop.getProperty",
                new object[]
                {
                    parentObject,
                    property
                }).ConfigureAwait(false);
            return jsObjectRef;
        }

        public static async ValueTask<T> GetJsPropertyContent<T>(this IJSRuntime jsRuntime, JsObjectRef parentObject,
            string property, object contentSpec)
        {
            var content = await jsRuntime.InvokeAsync<T>(
                "webRtcInterop.getPropertyContent",
                new object[]
                {
                    parentObject,
                    property,
                    contentSpec
                }).ConfigureAwait(false);
            return content;
        }

        public static async ValueTask CallJsMethodVoid(this IJSRuntime jsRuntime, JsObjectRef parentObject,
            string method, params object[] args)
        {
            await jsRuntime.InvokeVoidAsync(
                "webRtcInterop.callMethod",
                new object[]
                {
                    parentObject,
                    method
                }.Concat(args).ToArray()).ConfigureAwait(false);
        }

        public static async ValueTask<JsObjectRef> CallJsMethod(this IJSRuntime jsRuntime, JsObjectRef parentObject,
            string method, params object[] args)
        {
            var jsObjectRef = await jsRuntime.InvokeAsync<JsObjectRef>(
                "webRtcInterop.callMethod",
                new object[]
                {
                    parentObject,
                    method
                }.Concat(args).ToArray()).ConfigureAwait(false);
            return jsObjectRef;
        }

        public static async ValueTask CallJsMethodVoidAsync(this IJSRuntime jsRuntime, JsObjectRef parentObject,
            string method, params object[] args)
        {
            await jsRuntime.InvokeVoidAsync(
                "webRtcInterop.callMethodAsync",
                new object[]
                {
                    parentObject,
                    method
                }.Concat(args).ToArray()).ConfigureAwait(false);
        }

        public static async ValueTask<JsObjectRef> CallJsMethodAsync(this IJSRuntime jsRuntime, JsObjectRef parentObject,
            string method, params object[] args)
        {
            var jsObjectRef = await jsRuntime.InvokeAsync<JsObjectRef>(
                "webRtcInterop.callMethodAsync",
                new object[]
                {
                    parentObject,
                    method
                }.Concat(args).ToArray()).ConfigureAwait(false);
            return jsObjectRef;
        }


    }
}