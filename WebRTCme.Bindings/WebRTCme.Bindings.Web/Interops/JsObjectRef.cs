using Microsoft.JSInterop;
using System;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using WebRtcBindingsWeb.Extensions;

namespace WebRtcBindingsWeb.Interops
{
    public class JsObjectRef
    {
        [JsonPropertyName("__jsObjectRefId")] 
        public int JsObjectRefId { get; set; }
    }
}