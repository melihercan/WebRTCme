using System;
using System.Collections.Generic;
using System.Text;
using WebRTCme;

namespace WebRtc.Android
{
    public abstract class ApiBase<T> : INativeObject<T> where T : class
    {
        protected ApiBase(T nativeObject = null)
        {
            NativeObject = nativeObject;
        }

        public T NativeObject { get; }

        public void Dispose()
        {
            if(NativeObject != null && NativeObject is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }
}
