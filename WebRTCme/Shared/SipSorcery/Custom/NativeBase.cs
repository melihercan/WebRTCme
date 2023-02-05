using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebRTCme.Shared.SipSorcery.Custom
{
    internal abstract class NativeBase<T>
    {
        protected NativeBase() { }

        protected NativeBase(T nativeObject) => NativeObject = nativeObject;

        public T NativeObject { get; init; }

    }

}
