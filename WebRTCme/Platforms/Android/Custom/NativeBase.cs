using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme.Platforms.Android.Custom
{
    internal abstract class NativeBase<T> : Java.Lang.Object//, INativeObject<T>
    {
        protected NativeBase() { }

        protected NativeBase(T nativeObject) => NativeObject = nativeObject;

        public T NativeObject { get; init; }
        //object INativeObject.NativeObject { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        //public object GetNativeObject() => NativeObject;
    }
}
