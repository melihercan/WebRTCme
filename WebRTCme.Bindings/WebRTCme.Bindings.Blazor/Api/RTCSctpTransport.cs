using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using WebRTCme.Bindings.Blazor.Interops;
using WebRTCme.Bindings.Blazor.Extensions;
using WebRTCme;

namespace WebRTCme.Bindings.Blazor.Api
{
    internal class RTCSctpTransport : ApiBase, IRTCSctpTransport
    {
        internal static IRTCSctpTransport Create(IJSRuntime jsRuntime, JsObjectRef jsObjectRefSctpTransport)
        {
            return new RTCSctpTransport(jsRuntime, jsObjectRefSctpTransport);
        }

        private RTCSctpTransport(IJSRuntime jsRuntime, JsObjectRef jsObjectRef) : base(jsRuntime, jsObjectRef) 
        {
            AddNativeEventListenerForValue<RTCSctpTransportState>("statechange", (s, e) => OnStateChange?.Invoke(s, e));
        }

        public int MaxChannels => GetNativeProperty<int>("maxChannels");

        public int MaxMessageSize => GetNativeProperty<int>("maxMessageSize");

        public RTCSctpTransportState State => GetNativeProperty<RTCSctpTransportState>("state");

        public IRTCSctpTransport Transport =>
            RTCSctpTransport.Create(JsRuntime, JsRuntime.GetJsPropertyObjectRef(NativeObject, "transport"));

        public event EventHandler<RTCSctpTransportState> OnStateChange;
    }
}
