using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using WebRTCme.Bindings.Blazor.Interops;
using WebRTCme;

namespace WebRTCme.Bindings.Blazor.Api
{
    internal class RTCIdentityAssertion : ApiBase, IRTCIdentityAssertion
    {
        internal static IRTCIdentityAssertion Create(IJSRuntime jsRuntime, JsObjectRef jsObjectRefIdentityAssertion) => 
            new RTCIdentityAssertion(jsRuntime, jsObjectRefIdentityAssertion);

        private RTCIdentityAssertion(IJSRuntime jsRuntime, JsObjectRef jsObjectRef) : base(jsRuntime, jsObjectRef) { }

        public string Idp 
        {
            get => GetNativeProperty<string>("idp");
            set => SetNativeProperty("idp", value);
        }
        
        public string Name 
        {
            get => GetNativeProperty<string>("name");
            set => SetNativeProperty("name", value);
        }
    }
}
