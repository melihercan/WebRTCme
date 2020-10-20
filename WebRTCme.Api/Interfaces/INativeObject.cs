using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme
{
    public interface INativeObject<T> : IDisposable
    {
        T NativeObject { get; }
    }

    public interface INativeObjectAsync<T> : IAsyncDisposable
    {
        IJSRuntime JsRuntime { get; }
        T NativeObject { get; }
    }
}
