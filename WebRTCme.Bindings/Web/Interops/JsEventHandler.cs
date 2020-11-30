using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace WebRtcJsInterop.Interops
{
    public class JsEventHandler
    {
        [JsonPropertyName("__jsEventRefHandler")]
        public string JsEventRefHandler { get; set; } = string.Empty;

        public object ContentSpec { get; set; }

        public bool GetJsObjectRef { get; set; }

        public object CallbackRef { get; set; }

        private JsEventHandler() { }

        public static JsEventHandler Create(Action callback, object contentSpec = null,
            bool getJsObjectRef = false)
        {
            return new JsEventHandler
            {
                CallbackRef = DotNetObjectReference.Create(new JsInteropActionHandler(callback)),
                ContentSpec = contentSpec,
                GetJsObjectRef = getJsObjectRef
            };
        }

        public static JsEventHandler Create<T>(Action<T> callback, object contentSpec = null,
            bool getJsObjectRef = false)
        {
            return new JsEventHandler
            {
                CallbackRef = DotNetObjectReference.Create(new JsInteropActionHandler<T>(callback)),
                ContentSpec = contentSpec,
                GetJsObjectRef = getJsObjectRef
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
