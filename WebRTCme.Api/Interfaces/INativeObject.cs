using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme
{
    public interface INativeObject : IDisposable
    {
        object NativeObject { get; }
    }

    public interface INativeObjectAsync : IAsyncDisposable
    {
        object NativeObject { get; }
    }
}
