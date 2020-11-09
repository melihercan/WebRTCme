using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme
{
    public interface INativeObjects : IDisposable
    {
        object SelfNativeObject { get; }
        List<object> NativeObjects { get; set; }
    }

    public interface INativeObjectsAsync : IAsyncDisposable
    {
        object SelfNativeObject { get; }
        List<object> OtherNativeObjects { get; set; }
    }
}
