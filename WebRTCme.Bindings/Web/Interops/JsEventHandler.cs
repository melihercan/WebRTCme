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

        public static JsEventHandler Create(Func<ValueTask> callback, object contentSpec = null,
            bool getJsObjectRef = false)
        {
            return new JsEventHandler
            {
                CallbackRef = DotNetObjectReference.Create(new JsInteropActionHandler(callback)),
                ContentSpec = contentSpec,
                GetJsObjectRef = getJsObjectRef
            };
        }

        public static JsEventHandler Create<T>(Func<T, ValueTask> callback, object contentSpec = null,
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
            private readonly Func<ValueTask> _func;

            internal JsInteropActionHandler(Func<ValueTask> func)
            {
                _func = func;
            }
            [JSInvokable]
            public async Task Invoke()
            {
                await _func.Invoke();
            }
        }

        private class JsInteropActionHandler<T>
        {
            private readonly Func<T, ValueTask> _func;

            internal JsInteropActionHandler(Func<T, ValueTask> func)
            {
                _func = func;
            }
            [JSInvokable]
            public async Task Invoke(T arg)
            {
                await _func.Invoke(arg);
            }
        }

    }
}
