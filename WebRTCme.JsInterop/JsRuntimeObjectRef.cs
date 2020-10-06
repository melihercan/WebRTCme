using Microsoft.JSInterop;
using System;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace WebRtcJsInterop
{
    /// <summary>
    /// Represents a js object reference, send it to the js interop api and it will be seen as an instance instead of a serialized/deserialized object
    /// </summary>
    public class JsRuntimeObjectRef : IAsyncDisposable
    {
        internal IJSRuntime JsRuntime { get; set; }

        [JsonPropertyName("__jsObjectRefId")] public int JsObjectRefId { get; set; }

        public async ValueTask DisposeAsync()
        {
            await JsRuntime.InvokeVoidAsync("browserInterop.removeObjectRef", JsObjectRefId).ConfigureAwait(false);
        }
    }
}