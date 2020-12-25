using Foundation;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebRTCme;

namespace WebRtc.iOS
{
    internal class RTCRtpTransceiver : ApiBase, IRTCRtpTransceiver
    {
        public static IRTCRtpTransceiver Create(Webrtc.RTCRtpTransceiver nativeRtpTransceiver) => 
            new RTCRtpTransceiver(nativeRtpTransceiver);

        private RTCRtpTransceiver(Webrtc.RTCRtpTransceiver nativeRtpTransceiver) : base(nativeRtpTransceiver) { }

        public RTCRtpTransceiverDirection CurrentDirection => ((Webrtc.RTCRtpTransceiver)NativeObject).Direction
            .FromNative();

        public RTCRtpTransceiverDirection Direction 
        { 
            get => ((Webrtc.RTCRtpTransceiver)NativeObject).Direction.FromNative();
            set => ((Webrtc.RTCRtpTransceiver)NativeObject).SetDirection(value.ToNative(), out NSError _);
        }

        public string Mid => ((Webrtc.RTCRtpTransceiver)NativeObject).Mid;

        public IRTCRtpReceiver Receiver => RTCRtpReceiver.Create(((Webrtc.RTCRtpTransceiver)NativeObject).Receiver);

        public IRTCRtpSender Sender => RTCRtpSender.Create(((Webrtc.RTCRtpTransceiver)NativeObject).Sender);

        public bool Stopped => ((Webrtc.RTCRtpTransceiver)NativeObject).IsStopped;


        public void SetCodecPreferences(RTCRtpCodecCapability[] codecs)
        {
            throw new NotImplementedException();
        }

        public void Stop() => ((Webrtc.RTCRtpTransceiver)NativeObject).StopInternal();

    }
}
