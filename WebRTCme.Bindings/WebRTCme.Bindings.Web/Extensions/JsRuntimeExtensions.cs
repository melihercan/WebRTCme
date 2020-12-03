using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebRtcBindingsWeb.Interops;

namespace WebRtcBindingsWeb.Extensions
{
    public static class JsRuntimeExtensions
    {
        public static JsObjectRef CreateJsObject(this IJSRuntime jsRuntime, object parent, 
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
            var jsObjectRef = jsRuntime.Invoke<JsObjectRef>("JsInterop.createObject", invokeParams);
            return jsObjectRef;
        }

        public static void DeleteJsObjectRef(this IJSRuntime jsRuntime,  int id)
        {
            jsRuntime.Invoke<object>(
                "JsInterop.deleteObjectRef",
                new object[]
                {
                    id,
                });
        }

        public static JsObjectRef GetJsPropertyObjectRef(this IJSRuntime jsRuntime, 
            object parent,  string property)
        {
            var jsObjectRef = jsRuntime.Invoke<JsObjectRef>(
                "JsInterop.getPropertyObjectRef",
                new object[]
                {
                    parent,
                    property//,
                    //null
                });
            return jsObjectRef;
        }

        public static T GetJsPropertyValue<T>(this IJSRuntime jsRuntime, object parent,
            string property, object valueSpec = null)
        {
            var content = jsRuntime.Invoke<T>(
                "JsInterop.getPropertyValue",
                new object[]
                {
                    parent,
                    property,
                    valueSpec
                });
            return content;
        }

        public static IEnumerable<JsObjectRef> GetJsPropertyArray(this IJSRuntime jsRuntime,
            object parent, string property = null)
        {
            var jsObjectRefs = jsRuntime.Invoke<IEnumerable<JsObjectRef>>(
                "JsInterop.getPropertyArray",
                new object[]
                {
                    parent,
                    property,
                });
            return jsObjectRefs;
        }


        public static void SetJsProperty(this IJSRuntime jsRuntime, object parent,
            string property, object value)
        {
            jsRuntime.InvokeVoid(
                "JsInterop.setProperty",
                new object[]
                {
                    parent,
                    property,
                    value
                });
        }

        public static void CallJsMethodVoid(this IJSRuntime jsRuntime, object parent,
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
            jsRuntime.InvokeVoid("JsInterop.callMethod", invokeParams);
        }

        public static T CallJsMethod<T>(this IJSRuntime jsRuntime, object parent,
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
            var ret = jsRuntime.Invoke<T>("JsInterop.callMethod", invokeParams);
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
            await jsRuntime.InvokeVoidAsync("JsInterop.callMethodAsync", invokeParams)
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
            if(args != null)
            {
                invokeParams = invokeParams.Concat(args).ToArray();
            }
            var ret = await jsRuntime.InvokeAsync<T>("JsInterop.callMethodAsync", invokeParams)
                .ConfigureAwait(false);
            return ret;
        }

        public static IDisposable AddJsEventListener(this IJSRuntime jsRuntime,
            JsObjectRef jsObjectRef, string property, string event_, JsEventHandler callBack)
        {
            var listenerId = jsRuntime.Invoke<int>("JsInterop.addEventListener", jsObjectRef,
                property, event_, callBack);

            return new DisposableAction(() =>
                jsRuntime.InvokeVoid("JsInterop.removeEventListener", jsObjectRef, property,
                    event_, listenerId));
        }

        //public static async ValueTask<IAsyncDisposable> AddJsEventListener(this IJSRuntime jsRuntime,
        //    JsObjectRef jsObjectRef, string property, string event_, JsEventHandler callBack)
        //{
        //    var listenerId = await jsRuntime.InvokeAsync<int>("JsInterop.addEventListener", jsObjectRef,
        //        property, event_, callBack).ConfigureAwait(false);

        //    return new ActionAsyncDisposable(async () =>
        //        await jsRuntime.InvokeVoidAsync("JsInterop.removeEventListener", jsObjectRef, property,
        //            event_, listenerId).ConfigureAwait(false));
        //}



        public static TValue Invoke<TValue>(this IJSRuntime jsRuntime, string identifier, params object[] args)
        {
            var isWasm = jsRuntime is IJSInProcessRuntime;

            if (isWasm)
            {
                return ((IJSInProcessRuntime)jsRuntime).Invoke<TValue>(identifier, args);
            }
            else
            {
                // TODO: INVESTIGATE IF IT IS POSSIBLE TO RUN SERVER SIDE CODE SYNCHRONOUSLY
                //          IF NOT, CREATE ALL AYNC API
                // !!! Blocking UI thread !!!
                //return Task.Run(async () =>
                //{
                //return await jsRuntime.InvokeAsync<TValue>(identifier, args);
                //}).Result;
                throw new NotImplementedException();
            }
        }

        public static void InvokeVoid(this IJSRuntime jsRuntime, string identifier, params object[] args)
        {
            var isWasm = jsRuntime is IJSInProcessRuntime;

            if (isWasm)
            {
                _ = (IJSInProcessRuntime)jsRuntime.Invoke<object>(identifier, args);
            }
            else
            {
                // TODO: INVESTIGATE IF IT IS POSSIBLE TO RUN SERVER SIDE CODE SYNCHRONOUSLY
                //          IF NOT, CREATE ALL AYNC API
                // !!! Blocking UI thread !!!
                //Task.Run(async () =>
                //{
                //await jsRuntime.InvokeVoidAsync(identifier, args);
                //}).Wait();
                throw new NotImplementedException();
            }
        }
    }
}