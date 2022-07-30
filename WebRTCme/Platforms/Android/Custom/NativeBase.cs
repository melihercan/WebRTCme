using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme.Platforms.Android.Custom
{
    internal abstract class NativeBase<T> : Java.Lang.Object
    {
        protected NativeBase() { }

        protected NativeBase(T nativeObject) => NativeObject = nativeObject;

        public T NativeObject { get; init; }
    }
}
