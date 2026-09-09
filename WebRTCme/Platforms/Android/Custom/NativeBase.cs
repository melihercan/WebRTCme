using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme.Platforms.Android.Custom
{
    internal abstract class NativeBase<T>//, INativeObject<T>
    {
        readonly Func<T> _provider;
        T _nativeObject;

        protected NativeBase() { }

        protected NativeBase(T nativeObject) => NativeObject = nativeObject;

        /// <summary>
        /// Derives the native object on every access instead of holding one.
        /// </summary>
        /// <remarks>
        /// libwebrtc disposes the Java peers it handed out earlier whenever the transceiver,
        /// sender or receiver lists are re-enumerated. A wrapper built this way re-reads its
        /// native from whatever is live now, so a reference a caller is still holding keeps
        /// working across an enumeration instead of throwing on a disposed object.
        /// </remarks>
        protected NativeBase(Func<T> provider) => _provider = provider;

        public T NativeObject
        {
            get => _provider is null ? _nativeObject : _provider();
            init => _nativeObject = value;
        }

        /// <summary>
        /// Points this wrapper at a freshly enumerated native peer, replacing the disposed one.
        /// </summary>
        private protected void RebindNativeObject(T nativeObject) => _nativeObject = nativeObject;
    }
}
