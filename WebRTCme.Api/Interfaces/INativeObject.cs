using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme
{
    public interface INativeObject : IDisposable
    {
        ////bool IsNativeObjectsOwner { get; set; }
        
        object NativeObject { get; set; }

        ////List<object> NativeObjects { get; set; }
    }
}
