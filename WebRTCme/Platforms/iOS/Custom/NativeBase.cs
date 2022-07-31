using Foundation;
using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme.Platforms.iOS.Custom
{
    internal abstract class NativeBase<T> : NSObject
    {
        protected NativeBase() { }

        protected NativeBase(T nativeObject) => NativeObject = nativeObject;

        public T NativeObject { get; init; }

    }
}
