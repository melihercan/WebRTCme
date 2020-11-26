using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme
{
    public interface INativeObjects : IDisposable
    {
        ////bool IsNativeObjectsOwner { get; set; }
        
        object SelfNativeObject { get; }

        List<object> NativeObjects { get; set; }
    }
}
