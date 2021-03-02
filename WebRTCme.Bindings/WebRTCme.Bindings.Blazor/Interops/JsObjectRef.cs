using Microsoft.JSInterop;
using System;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using WebRtcMeBindingsBlazor.Extensions;

namespace WebRtcMeBindingsBlazor.Interops
{
    public class JsObjectRef
    {
        [JsonPropertyName("__jsObjectRefId")] 
        public int JsObjectRefId { get; set; }
    }
}