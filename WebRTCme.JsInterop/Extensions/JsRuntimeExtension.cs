using Microsoft.JSInterop;
using System;
using System.Linq;
using System.Threading.Tasks;
using WebRtcJsInterop.Interops;

namespace WebRtcJsInterop.Extensions
{
    /// <summary>
    /// Extension to the JSRuntime for using Browser API
    /// </summary>
    public static class JsRuntimeExtension
    {
        /// <summary>
        /// Create a WindowInterop instance that can be used for using Browser API
        /// </summary>
        /// <param name="jsRuntime"></param>
        /// <returns></returns>
        public static async ValueTask<WindowInterop> Window(this IJSRuntime jsRuntime)
        {
            var jsObjectRef = await jsRuntime.GetWindowPropertyRef("window").ConfigureAwait(false);
            var wsInterop =
                await jsRuntime.GetInstanceProperty<WindowInterop>(jsObjectRef, "self",
                    WindowInterop.SerializationSpec).ConfigureAwait(false);
            wsInterop.SetJsRuntime(jsRuntime, jsObjectRef);
            return wsInterop;
        }

        /// <summary>
        /// Get the window object property value reference
        /// </summary>
        /// <param name="jsRuntime">current js runtime</param>
        /// <param name="propertyPath">path of the property</param>
        /// <returns></returns>
        public static async ValueTask<JsRuntimeObjectRef> GetWindowPropertyRef(this IJSRuntime jsRuntime,
            string propertyPath)
        {
            return await jsRuntime.InvokeAsync<JsRuntimeObjectRef>("browserInterop.getPropertyRef", propertyPath).ConfigureAwait(false);
        }
        
        /// <summary>
        /// Get the window object property wrapper for interop calls
        /// </summary>
        /// <param name="jsRuntime">current js runtime</param>
        /// <param name="propertyPath">path of the property</param>
        /// <returns></returns>
        public static async ValueTask<T> GetWindowPropertyWrapper<T>(this IJSRuntime jsRuntime,
            string propertyPath, object serializationSpec) where T : JsObjectWrapperBase
        {
            var objectRef = await GetWindowPropertyRef(jsRuntime, propertyPath).ConfigureAwait(false);
            if (objectRef == null)
            {
                return null;
            }
            var objectContent = await GetInstanceContent<T>(jsRuntime, objectRef, serializationSpec).ConfigureAwait(false);
            
           
            objectContent.SetJsRuntime(jsRuntime, objectRef);
            return objectContent;
        }

        /// <summary>
        /// Get the js object property value
        /// </summary>
        /// <param name="jsRuntime">current js runtime</param>
        /// <param name="propertyPath">path of the property</param>
        /// <param name="jsObjectRef">Ref to the js object from which we'll get the property</param>
        /// <param name="serializationSpec">
        /// An object specifying the member we'll want from the JS object.
        /// "new { allChild = "*", onlyMember = true, ignore = false }" will get all the fields in allChild,
        /// the value of "onlyMember" and will ignore "ignore"
        /// "true" or null will get everything, false will get nothing
        /// </param>        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static async ValueTask<T> GetInstanceProperty<T>(this IJSRuntime jsRuntime,
            JsRuntimeObjectRef jsObjectRef, string propertyPath, object serializationSpec = null)
        {
            return await jsRuntime.InvokeAsync<T>("browserInterop.getInstancePropertySerializable", jsObjectRef,
                propertyPath, serializationSpec).ConfigureAwait(false);
        }

        /// <summary>
        /// Get the js object property value and initialize its js object reference
        /// </summary>
        /// <param name="jsRuntime">current js runtime</param>
        /// <param name="propertyPath">path of the property</param>
        /// <param name="jsObjectRef">Ref to the js object from which we'll get the property</param>
        /// <param name="serializationSpec">
        /// An object specifying the member we'll want from the JS object.
        /// "new { allChild = "*", onlyMember = true, ignore = false }" will get all the fields in allChild,
        /// the value of "onlyMember" and will ignore "ignore"
        /// "true" or null will get everything, false will get nothing
        /// </param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static async ValueTask<T> GetInstancePropertyWrapper<T>(this IJSRuntime jsRuntime,
            JsRuntimeObjectRef jsObjectRef, string propertyPath, object serializationSpec = null)
            where T : JsObjectWrapperBase
        {
            var taskContent = GetInstanceProperty<T>(jsRuntime, jsObjectRef, propertyPath, serializationSpec);
            var taskRef = GetInstancePropertyRef(jsRuntime, jsObjectRef, propertyPath);
            var res = await taskContent.ConfigureAwait(false);
            var jsRuntimeObjectRef = await taskRef.ConfigureAwait(false);
            if (res == null)
            {
                return null;
            }
            res.SetJsRuntime(jsRuntime, jsRuntimeObjectRef);
            return res;
        }

        /// <summary>
        /// Set the js object property value
        /// </summary>
        /// <param name="jsRuntime"></param>
        /// <param name="jsObjectRef">The JS object you want to change</param>
        /// <param name="propertyPath">The object property name</param>
        /// <param name="value">The new value (can be a JsRuntimeObjectRef)</param>
        /// <returns></returns>
        public static async ValueTask SetInstanceProperty(this IJSRuntime jsRuntime, JsRuntimeObjectRef jsObjectRef,
            string propertyPath, object value)
        {
            await jsRuntime.InvokeVoidAsync("browserInterop.setInstanceProperty", jsObjectRef, propertyPath, value).ConfigureAwait(false);
        }

        /// <summary>
        /// Return a reference to the JS instance located on the given property 
        /// </summary>
        /// <param name="jsRuntime">Current JS runtime</param>
        /// <param name="jsObjectRef">Reference to the parent instance</param>
        /// <param name="propertyPath">property path</param>
        /// <returns></returns>
        public static async ValueTask<JsRuntimeObjectRef> GetInstancePropertyRef(this IJSRuntime jsRuntime,
            JsRuntimeObjectRef jsObjectRef, string propertyPath)
        {
            var jsRuntimeObjectRef =
                await jsRuntime.InvokeAsync<JsRuntimeObjectRef>("browserInterop.getInstancePropertyRef", jsObjectRef,
                    propertyPath).ConfigureAwait(false);
            jsRuntimeObjectRef.JsRuntime = jsRuntime;
            return jsRuntimeObjectRef;
        }


        /// <summary>
        /// Call the method on the js instance
        /// </summary>
        /// <param name="jsRuntime">Current JS Runtime</param>
        /// <param name="windowObject">Reference to the JS instance</param>
        /// <param name="methodName">Method name/path </param>
        /// <param name="arguments">method arguments</param>
        /// <returns></returns>
        public static async ValueTask InvokeInstanceMethod(this IJSRuntime jsRuntime, JsRuntimeObjectRef windowObject,
            string methodName, params object[] arguments)
        {
            await jsRuntime.InvokeVoidAsync("browserInterop.callInstanceMethod",
                new object[] {windowObject, methodName}.Concat(arguments).ToArray()).ConfigureAwait(false);
        }

        /// <summary>
        /// Call the method on the js instance and return the result
        /// </summary>
        /// <param name="jsRuntime">Current JS Runtime</param>
        /// <param name="windowObject">Reference to the JS instance</param>
        /// <param name="methodName">Method name/path </param>
        /// <param name="arguments">method arguments</param>
        /// <returns></returns>
        public static async ValueTask<T> InvokeInstanceMethod<T>(this IJSRuntime jsRuntime,
            JsRuntimeObjectRef windowObject, string methodName, params object[] arguments)
        {
            if (jsRuntime is null) throw new ArgumentNullException(nameof(jsRuntime));

            if (windowObject is null) throw new ArgumentNullException(nameof(windowObject));

            return await jsRuntime.InvokeAsync<T>("browserInterop.callInstanceMethod",
                new object[] {windowObject, methodName}.Concat(arguments).ToArray()).ConfigureAwait(false);
        }

        /// <summary>
        /// Get the js object content
        /// </summary>
        /// <param name="jsRuntime">Current JS Runtime</param>
        /// <param name="jsObject">Reference to the JS instance</param>
        /// <param name="serializationSpec">
        /// An object specifying the member we'll want from the JS object.
        /// "new { allChild = "*", onlyMember = true, ignore = false }" will get all the fields in allChild,
        /// the value of "onlyMember" and will ignore "ignore"
        /// "true" or null will get everything, false will get nothing
        /// </param>
        /// <returns></returns>
        public static async ValueTask<T> GetInstanceContent<T>(this IJSRuntime jsRuntime, JsRuntimeObjectRef jsObject,
            object serializationSpec)
        {
            return await jsRuntime.InvokeAsync<T>("browserInterop.returnInstance", jsObject, serializationSpec).ConfigureAwait(false);
        }

        /// <summary>
        /// Get the js object content updated
        /// </summary>
        /// <param name="jsRuntime">Current JS Runtime</param>
        /// <param name="jsObject">The JS object for which you want the content updated</param>
        /// <param name="serializationSpec"></param>
        /// <returns></returns>
        public static async ValueTask<T> GetInstanceContent<T>(this IJSRuntime jsRuntime, T jsObject,
            object serializationSpec = null) where T : JsObjectWrapperBase
        {
            if (jsObject is null) throw new ArgumentNullException(nameof(jsObject));

            var res = await GetInstanceContent<T>(jsRuntime, jsObject.JsObjectRef, serializationSpec).ConfigureAwait(false);
            res.SetJsRuntime(jsRuntime, jsObject.JsObjectRef);
            return res;
        }


        /// <summary>
        /// Call the method on the js instance and return the reference to the js object
        /// </summary>
        /// <param name="jsRuntime">Current JS Runtime</param>
        /// <param name="windowObject">Reference to the JS instance</param>
        /// <param name="methodName">Method name/path </param>
        /// <param name="arguments">method arguments</param>
        /// <returns></returns>
        public static async ValueTask<JsRuntimeObjectRef> InvokeInstanceMethodGetRef(this IJSRuntime jsRuntime,
            JsRuntimeObjectRef windowObject, string methodName, params object[] arguments)
        {
            if (jsRuntime is null) throw new ArgumentNullException(nameof(jsRuntime));

            var jsRuntimeObjectRef = await jsRuntime.InvokeAsync<JsRuntimeObjectRef>(
                "browserInterop.callInstanceMethodGetRef",
                new object[] {windowObject, methodName}.Concat(arguments).ToArray()).ConfigureAwait(false);
            jsRuntimeObjectRef.JsRuntime = jsRuntime;
            return jsRuntimeObjectRef;
        }

        public static async ValueTask<bool> HasProperty(this IJSRuntime jsRuntime, JsRuntimeObjectRef jsObject,
            string propertyPath)
        {
            return await jsRuntime.InvokeAsync<bool>("browserInterop.hasProperty", jsObject, propertyPath).ConfigureAwait(false);
        }

        /// <summary>
        /// Add an event listener to the given property and event Type
        /// </summary>
        /// <param name="jsRuntime"></param>
        /// <param name="jsRuntimeObject"></param>
        /// <param name="propertyName"></param>
        /// <param name="eventName"></param>
        /// <param name="callBack"></param>
        /// <returns></returns>
        //public static async ValueTask<IAsyncDisposable> AddEventListener(this IJSRuntime jsRuntime,
        //    JsRuntimeObjectRef jsRuntimeObject, string propertyName, string eventName, CallBackInteropWrapper callBack)
        //{
        //    var listenerId = await jsRuntime.InvokeAsync<int>("browserInterop.addEventListener", jsRuntimeObject,
        //        propertyName, eventName, callBack).ConfigureAwait(false);

        //    return new ActionAsyncDisposable(async () =>
        //        await jsRuntime.InvokeVoidAsync("browserInterop.removeEventListener", jsRuntimeObject, propertyName,
        //            eventName, listenerId).ConfigureAwait(false));
        //}

        /// <summary>
        /// Invoke the specified method with JSInterop and returns default(T) if the timeout is reached
        /// </summary>
        /// <param name="jsRuntime">js runtime on which we'll execute the query</param>
        /// <param name="identifier">method identifier</param>
        /// <param name="timeout">timeout until e return default(T)</param>
        /// <param name="args">method arguments</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static async ValueTask<T> InvokeOrDefault<T>(this IJSRuntime jsRuntime, string identifier,
            TimeSpan timeout, params object[] args)
        {
            try
            {
                return await jsRuntime.InvokeAsync<T>(identifier,
                    timeout,
                    args).ConfigureAwait(false);
            }
            catch (TaskCanceledException)
            {
                //when timeout is reached, it raises an exception
                return await Task.FromResult(default(T)).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Return the value of a DOMHighResTimeStamp to TimeSpan
        /// </summary>
        /// <param name="timeStamp">value of a DOMHighResTimeStamp</param>
        /// <returns></returns>
        public static TimeSpan HighResolutionTimeStampToTimeSpan(this double timeStamp)
        {
            return TimeSpan.FromTicks((long) timeStamp * 10000);
        }
    }
}