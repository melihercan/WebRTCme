using Microsoft.JSInterop;
using System;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using WebRTCme.Bindings.Blazor.Extensions;

namespace WebRTCme.Bindings.Blazor.Interops
{
    public class JsObjectRef
    {
        [JsonPropertyName("__jsObjectRefId")] 
        public int JsObjectRefId { get; set; }
    }
}