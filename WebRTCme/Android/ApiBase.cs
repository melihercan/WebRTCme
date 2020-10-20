using System;
using System.Collections.Generic;
using System.Text;
using WebRTCme;

namespace WebRtc.Android
{
    public abstract class ApiBase : INativeObject
    {
        protected ApiBase(object nativeObject = null) => NativeObject = nativeObject;

        public object NativeObject { get; }

        public void Dispose()
        {
            if(NativeObject != null && NativeObject is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }
}
