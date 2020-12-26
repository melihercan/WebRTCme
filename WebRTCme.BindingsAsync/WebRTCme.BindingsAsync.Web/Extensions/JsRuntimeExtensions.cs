using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebRtcJsInterop.Interops;

namespace WebRtcJsInterop.Extensions
{
    public static class JsRuntimeExtensions
    {
        public static async ValueTask<JsObjectRef> CreateJsObject(this IJSRuntime jsRuntime, object parent,
            string interface_, params object[] args)
        {
            var invokeParams = new object[]
            {
                parent,
                interface_
            };
            if (args != null)
            {
                invokeParams = invokeParams.Concat(args).ToArray();
            }
            var jsObjectRef = await jsRuntime.InvokeAsync<JsObjectRef>("DotNetInterop.createObject", invokeParams)
                .ConfigureAwait(false);
            return jsObjectRef;
        }

        public static async ValueTask DeleteJsObjectRef(this IJSRuntime jsRuntime, int id)
        {
            await jsRuntime.InvokeAsync<object>(
                "DotNetInterop.deleteObjectRef",
                new object[]
                {
                    id,
                }).ConfigureAwait(false);
        }

        public static async ValueTask<JsObjectRef> GetJsPropertyObjectRef(this IJSRuntime jsRuntime,
            object parent, string property)
        {
            var jsObjectRef = await jsRuntime.InvokeAsync<JsObjectRef>(
                "DotNetInterop.getPropertyObjectRef",
                new object[]
                {
                    parent,
                    property,
                    null
                }).ConfigureAwait(false);
            return jsObjectRef;
        }

        public static async ValueTask<T> GetJsPropertyValue<T>(this IJSRuntime jsRuntime, object parent,
            string property, object valueSpec)
        {
            var content = await jsRuntime.InvokeAsync<T>(
                "DotNetInterop.getPropertyValue",
                new object[]
                {
                    parent,
                    property,
                    valueSpec
                }).ConfigureAwait(false);
            return content;
        }

        public static async ValueTask<IEnumerable<JsObjectRef>> GetJsPropertyArray(this IJSRuntime jsRuntime,
            object parent, string property = null)
        {
            var jsObjectRefs = await jsRuntime.InvokeAsync<IEnumerable<JsObjectRef>>(
                "DotNetInterop.getPropertyArray",
                new object[]
                {
                    parent,
                    property,
                }).ConfigureAwait(false);
            return jsObjectRefs;
        }


        public static async ValueTask SetJsProperty(this IJSRuntime jsRuntime, object parent,
            string property, object value)
        {
            await jsRuntime.InvokeVoidAsync(
                "DotNetInterop.setProperty",
                new object[]
                {
                    parent,
                    property,
                    value
                }).ConfigureAwait(false);
        }

        public static async ValueTask CallJsMethodVoid(this IJSRuntime jsRuntime, object parent,
            string method, params object[] args)
        {
            var invokeParams = new object[]
            {
                parent,
                method
            };
            if (args != null)
            {
                invokeParams = invokeParams.Concat(args).ToArray();
            }
            await jsRuntime.InvokeVoidAsync("DotNetInterop.callMethod", invokeParams)
                .ConfigureAwait(false);
        }

        public static async ValueTask<T> CallJsMethod<T>(this IJSRuntime jsRuntime, object parent,
            string method, params object[] args)
        {
            var invokeParams = new object[]
            {
                parent,
                method
            };
            if (args != null)
            {
                invokeParams = invokeParams.Concat(args).ToArray();
            }
            var ret = await jsRuntime.InvokeAsync<T>("DotNetInterop.callMethod", invokeParams)
                .ConfigureAwait(false);
            return ret;
        }

        public static async ValueTask CallJsMethodVoidAsync(this IJSRuntime jsRuntime, object parent,
            string method, params object[] args)
        {
            var invokeParams = new object[]
            {
                parent,
                method
            };
            if (args != null)
            {
                invokeParams = invokeParams.Concat(args).ToArray();
            }
            await jsRuntime.InvokeVoidAsync("DotNetInterop.callMethodAsync", invokeParams)
                .ConfigureAwait(false);
        }

        public static async ValueTask<T> CallJsMethodAsync<T>(this IJSRuntime jsRuntime,
            object parent, string method, params object[] args)
        {
            var invokeParams = new object[]
            {
                parent,
                method
            };
            if (args != null)
            {
                invokeParams = invokeParams.Concat(args).ToArray();
            }
            var ret = await jsRuntime.InvokeAsync<T>("DotNetInterop.callMethodAsync", invokeParams)
                .ConfigureAwait(false);
            return ret;
        }

        public static async ValueTask<IAsyncDisposable> AddJsEventListener(this IJSRuntime jsRuntime,
            JsObjectRef jsObjectRef, string property, string event_, JsEventHandler callBack)
        {
            var listenerId = await jsRuntime.InvokeAsync<int>("DotNetInterop.addEventListener", jsObjectRef,
                property, event_, callBack).ConfigureAwait(false);

            return new ActionAsyncDisposable(async () =>
                await jsRuntime.InvokeVoidAsync("DotNetInterop.removeEventListener", jsObjectRef, property,
                    event_, listenerId).ConfigureAwait(false));
        }
    }
}
