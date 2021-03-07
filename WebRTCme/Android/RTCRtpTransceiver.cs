using Webrtc = Org.Webrtc;
using System;
using WebRTCme;
using Org.Webrtc;

namespace WebRTCme.Android
{
    internal class RTCRtpTransceiver : ApiBase, IRTCRtpTransceiver
    {

        public static IRTCRtpTransceiver Create(Webrtc.RtpTransceiver nativeTransceiver) =>
            new RTCRtpTransceiver(nativeTransceiver);

        private RTCRtpTransceiver(RtpTransceiver nativeTransceiver) : base(nativeTransceiver)
        {
        }

        public RTCRtpTransceiverDirection CurrentDirection => 
            ((Webrtc.RtpTransceiver)NativeObject).CurrentDirection.FromNative();

        public RTCRtpTransceiverDirection Direction
        {
            get => ((Webrtc.RtpTransceiver)NativeObject).Direction.FromNative();
            set => ((Webrtc.RtpTransceiver)NativeObject).SetDirection(Direction.ToNative());
        }

        public string Mid => ((Webrtc.RtpTransceiver)NativeObject).Mid;

        public IRTCRtpReceiver Receiver => RTCRtpReceiver.Create(((Webrtc.RtpTransceiver)NativeObject).Receiver);

        public IRTCRtpSender Sender => RTCRtpSender.Create(((Webrtc.RtpTransceiver)NativeObject).Sender);

        public bool Stopped => ((Webrtc.RtpTransceiver)NativeObject).IsStopped;


        public void SetCodecPreferences(RTCRtpCodecCapability[] codecs)
        {
            throw new NotImplementedException();
        }

        public void Stop() => ((Webrtc.RtpTransceiver)NativeObject).Stop();
    }
}