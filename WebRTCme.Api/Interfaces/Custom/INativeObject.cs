using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme
{
    public interface INativeObject : IDisposable
    {
        object NativeObject { get; set; }
        object GetNativeObject();
    }

    public interface INativeObject<T> : INativeObject
    {
        new T NativeObject { get; init; }
    }
}
