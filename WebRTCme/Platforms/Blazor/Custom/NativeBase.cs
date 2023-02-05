using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using WebRTCme.Bindings.Blazor.Extensions;
using WebRTCme.Bindings.Blazor.Interops;
using WebRTCme;

namespace WebRTCme.Platforms.Blazor.Custom
{
    public abstract class NativeBase
    {
        protected NativeBase(IJSRuntime jsRuntime, JsObjectRef jsObjectRef = null)
        {
            JsRuntime = jsRuntime;
            NativeObject = jsObjectRef;
        }

        [JsonIgnore]
        public IJSRuntime JsRuntime { get; }
        
        private object _nativeObject;
        [JsonIgnore]
        public object NativeObject// { get; protected set; }
        {
            get => _nativeObject;
            set
            { 
                _nativeObject = value;
                JsObjects.Add(value as JsObjectRef);
            }
        }

        [JsonIgnore]
        public List<JsObjectRef> JsObjects { get; set; } = new List<JsObjectRef>();

        [JsonIgnore]
        public List<IDisposable> JsEvents { get; set; } = new List<IDisposable>();
        

        public void Dispose()
        {
            foreach (var jsEvent in JsEvents)
            {
                jsEvent.Dispose();
            }
            foreach (var jsObjectRef in JsObjects)
            {
                JsRuntime.DeleteJsObjectRef(jsObjectRef.JsObjectRefId);
            }

        }

        protected void AddNativeEventListener(string eventName, EventHandler eventHandler)
        {
            JsEvents.Add(JsRuntime.AddJsEventListener(NativeObject as JsObjectRef, null, eventName,
                JsEventHandler.Create(() =>
                {
                    eventHandler?.Invoke(this, EventArgs.Empty);
                })));
        }

        protected void AddNativeEventListenerForValue<T>(string eventName, EventHandler<T> eventHandler, 
            object contentSpec = null)
        {
            JsEvents.Add(JsRuntime.AddJsEventListener(NativeObject as JsObjectRef, null, eventName,
                JsEventHandler.CreateForValue<T>(e =>
                {
                    eventHandler?.Invoke(this, e);
                },
                contentSpec)));
        }

        protected void AddNativeEventListenerForObjectRef<T>(string eventName, EventHandler<T> eventHandler, 
            Func<IJSRuntime, JsObjectRef, T> fromNative)
        {
            JsEvents.Add(JsRuntime.AddJsEventListener(NativeObject as JsObjectRef, null, eventName,
                JsEventHandler.CreateForObjectRef<JsObjectRef>(jsObjectRef =>
                {
                    eventHandler?.Invoke(this, fromNative(JsRuntime, jsObjectRef));
                })));
        }

        protected T GetNativeProperty<T>(string propertyName) => JsRuntime.GetJsPropertyValue<T>(
            NativeObject, propertyName);

        protected void SetNativeProperty(string propertyName, object value) => JsRuntime.SetJsProperty(
            NativeObject, propertyName, value);
    }
}
