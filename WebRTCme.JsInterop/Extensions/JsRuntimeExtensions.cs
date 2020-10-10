using Microsoft.JSInterop;
using System;
using System.Linq;
using System.Threading.Tasks;
using WebRtcJsInterop.Interops;

namespace WebRtcJsInterop.Extensions
{
    // If parentObject is null, 'window' will be used as parent object on JS side.

    public static class JsRuntimeExtensions
    {
        public static async ValueTask<JsObjectRef> CreateJsObject(this IJSRuntime jsRuntime, JsObjectRef parentObject, 
            string interface_, params object[] args)
        {
            var jsObjectRef = await jsRuntime.InvokeAsync<JsObjectRef>(
                "objectRef.create",
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
                "objectRef.delete",
                new object[]
                {
                    id,
                }).ConfigureAwait(false);
        }

        public static async ValueTask<JsObjectRef> GetJsProperty(this IJSRuntime jsRuntime, JsObjectRef parentObject,
            string property)
        {
            var jsObjectRef = await jsRuntime.InvokeAsync<JsObjectRef>(
                "objectRef.get",
                new object[]
                {
                    parentObject,
                    property
                }).ConfigureAwait(false);
            return jsObjectRef;
        }

        public static async ValueTask SetJsProperty(this IJSRuntime jsRuntime, object parentObject,
            string property, object value)
        {
            await jsRuntime.InvokeVoidAsync(
                "objectRef.set",
                new object[]
                {
                    parentObject,
                    property,
                    value
                }).ConfigureAwait(false);
        }

        //public static async ValueTask SetJsProperty(this IJSRuntime jsRuntime, JsObjectRef parentObject,
        //    string property, object value)
        //{
        //    await jsRuntime.InvokeVoidAsync(
        //        "objectRef.set",
        //        new object[]
        //        {
        //            parentObject,
        //            property,
        //            value
        //        }).ConfigureAwait(false);
        //}

        // If property is null, whole parentObject content will be returned.
        public static async ValueTask<T> GetJsContent<T>(this IJSRuntime jsRuntime, JsObjectRef parentObject,
            string property, object contentSpec)
        {
            var content = await jsRuntime.InvokeAsync<T>(
                "objectRef.content",
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
                "objectRef.call",
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
                "objectRef.call",
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
                "objectRef.callAsync",
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
                "objectRef.callAsync",
                new object[]
                {
                    parentObject,
                    method
                }.Concat(args).ToArray()).ConfigureAwait(false);
            return jsObjectRef;
        }


    }
}