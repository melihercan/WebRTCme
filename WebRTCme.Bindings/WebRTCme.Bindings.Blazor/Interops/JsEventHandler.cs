using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace WebRTCme.Bindings.Blazor.Interops
{
    public class JsEventHandler
    {
        [JsonPropertyName("__jsEventRefHandler")]
        public string JsEventRefHandler { get; set; } = string.Empty;

        public object ContentSpec { get; set; }

        public bool GetJsObjectRef { get; set; }

        public object CallbackRef { get; set; }

        private JsEventHandler() { }

        public static JsEventHandler Create(Action callback)
        {
            return new JsEventHandler
            {
                CallbackRef = DotNetObjectReference.Create(new JsInteropActionHandler(callback)),
                ContentSpec = null,
                GetJsObjectRef = false
            };
        }

        public static JsEventHandler CreateForValue<T>(Action<T> callback, object contentSpec = null)
        {
            return new JsEventHandler
            {
                CallbackRef = DotNetObjectReference.Create(new JsInteropActionHandler<T>(callback)),
                ContentSpec = contentSpec,
                GetJsObjectRef = false
            };
        }

        public static JsEventHandler CreateForObjectRef<T>(Action<T> callback)
        {
            return new JsEventHandler
            {
                CallbackRef = DotNetObjectReference.Create(new JsInteropActionHandler<T>(callback)),
                ContentSpec = null,
                GetJsObjectRef = true
            };
        }

        private class JsInteropActionHandler
        {
            private readonly Action _action;

            internal JsInteropActionHandler(Action action)
            {
                _action = action;
            }
            [JSInvokable]
            public Task Invoke()
            {
                _action.Invoke();
                return Task.CompletedTask;
            }
        }

        private class JsInteropActionHandler<T>
        {
            private readonly Action<T> _action;

            internal JsInteropActionHandler(Action<T> action)
            {
                _action = action;
            }
            [JSInvokable]
            public Task Invoke(T arg)
            {
                _action.Invoke(arg);
                return Task.CompletedTask;
            }
        }

    }
}
