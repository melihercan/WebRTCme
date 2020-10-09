using Microsoft.JSInterop;
using System;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace WebRtcJsInterop.Interops
{
    public class JsObjectRef : IAsyncDisposable
    {
        [JsonPropertyName("__jsObjectRefId")] 
        public int JsObjectRefId { get; set; }

        internal IJSRuntime JsRuntime { get; set; }

        public async ValueTask DisposeAsync() => 
            await JsRuntime.InvokeVoidAsync("webRtcInterop.removeJsObjectRef", JsObjectRefId).ConfigureAwait(false);
    }
}