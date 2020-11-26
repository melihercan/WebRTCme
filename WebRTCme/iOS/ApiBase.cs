using Foundation;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebRTCme;

namespace WebRtc.iOS
{
    public abstract class ApiBase : NSObject, INativeObjects
    {
        ////public bool IsNativeObjectsOwner { get; set; } = true;

        private object _selfNativeObject;
        public object SelfNativeObject
        {
            get => _selfNativeObject;
            protected set
            {
                _selfNativeObject = value;
                NativeObjects.Add(value);
            }
        }

        public List<object> NativeObjects { get; set; } = new List<object>();

        public void Dispose()
        {
            ////if (IsNativeObjectsOwner)
            {
                foreach (var nativeObject in NativeObjects)
                {
                    if (nativeObject is IDisposable disposable)
                    {
                        disposable.Dispose();
                    }
                }
            }
        }
    }
}
