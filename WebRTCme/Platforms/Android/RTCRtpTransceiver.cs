using Webrtc = Org.Webrtc;
using System;
using WebRTCme;
using Org.Webrtc;
using WebRTCme.Platforms.Android.Custom;

namespace WebRTCme.Android
{
    internal class RTCRtpTransceiver : NativeBase<Webrtc.RtpTransceiver>, IRTCRtpTransceiver
    {
        public void Dispose() { }
        private readonly Webrtc.PeerConnection _nativePeerConnection;

        string _mid;

        public RTCRtpTransceiver(Webrtc.RtpTransceiver nativeTransceiver,
            Webrtc.PeerConnection nativePeerConnection = null) : base(nativeTransceiver)
        {
            _nativePeerConnection = nativePeerConnection;
        }

        /// <summary>
        /// The mid last read from the native peer, or null if it has never been negotiated.
        /// </summary>
        /// <remarks>
        /// Used to match this wrapper to its native peer after a re-enumeration, when the
        /// native it was built on has already been disposed and cannot be asked for its mid.
        /// </remarks>
        internal string LastKnownMid => _mid;

        /// <summary>
        /// Points this wrapper at the freshly enumerated native for the same m-line.
        /// </summary>
        internal void Rebind(Webrtc.RtpTransceiver nativeTransceiver) =>
            RebindNativeObject(nativeTransceiver);

        public RTCRtpTransceiverDirection CurrentDirection =>
            NativeObject.CurrentDirection.FromNative();

        public RTCRtpTransceiverDirection Direction
        {
            get => NativeObject.Direction.FromNative();
            set => NativeObject.SetDirection(Direction.ToNative());
        }

        // Cached once negotiated: a mid never changes, and caching it keeps the value readable
        // after the native this wrapper was built on has been disposed by a re-enumeration.
        public string Mid => _mid ??= NativeObject.Mid;

        // Resolved through this wrapper rather than captured, so they follow a Rebind.
        public IRTCRtpReceiver Receiver => new RTCRtpReceiver(() => NativeObject.Receiver, _nativePeerConnection);

        public IRTCRtpSender Sender => new RTCRtpSender(() => NativeObject.Sender, _nativePeerConnection);


        public void SetCodecPreferences(RTCRtpCodec[] codecs)
        {
            throw new NotImplementedException();
        }

        public void Stop() => NativeObject.Stop();
    }
}
