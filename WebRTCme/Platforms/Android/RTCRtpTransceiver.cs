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

        public RTCRtpTransceiver(Webrtc.RtpTransceiver nativeTransceiver,
            Webrtc.PeerConnection nativePeerConnection = null) : base(nativeTransceiver)
        {
            _nativePeerConnection = nativePeerConnection;
        }

        public RTCRtpTransceiverDirection CurrentDirection =>
            NativeObject.CurrentDirection.FromNative();

        public RTCRtpTransceiverDirection Direction
        {
            get => NativeObject.Direction.FromNative();
            set => NativeObject.SetDirection(Direction.ToNative());
        }

        public string Mid => NativeObject.Mid;

        public IRTCRtpReceiver Receiver => new RTCRtpReceiver(NativeObject.Receiver, _nativePeerConnection);

        public IRTCRtpSender Sender => new RTCRtpSender(NativeObject.Sender, _nativePeerConnection);


        public void SetCodecPreferences(RTCRtpCodec[] codecs)
        {
            throw new NotImplementedException();
        }

        public void Stop() => NativeObject.Stop();
    }
}