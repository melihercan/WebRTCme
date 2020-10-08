using Microsoft.JSInterop;
using System;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace WebRtcJsInterop.Interops
{
    /// <summary>
    /// Represents a JS object reference, send it to the js interop api and it will be seen as an instance 
    /// instead of a serialized/deserialized object.
    /// </summary>
    public class JsObjectRef : IAsyncDisposable
    {
        [JsonPropertyName("__jsObjectRefId")] 
        public int JsObjectRefId { get; set; }

        internal IJSRuntime JsRuntime { get; set; }

        public async ValueTask DisposeAsync() => 
            await JsRuntime.InvokeVoidAsync("webRtcInterop.removeJsObjectRef", JsObjectRefId).ConfigureAwait(false);
    }
}