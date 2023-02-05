using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using WebRTCme.Bindings.Blazor.Interops;
using WebRTCme;
using WebRTCme.Platforms.Blazor.Custom;

namespace WebRTCme.Blazor
{
    internal class RTCIdentityAssertion : NativeBase, IRTCIdentityAssertion
    {
        public RTCIdentityAssertion(IJSRuntime jsRuntime, JsObjectRef jsObjectRef) : base(jsRuntime, jsObjectRef) { }

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
