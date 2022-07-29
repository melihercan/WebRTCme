using Foundation;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebRTCme;

namespace WebRTCme.iOS
{
    internal class RTCRtpTransceiver : ApiBase, IRTCRtpTransceiver
    {
        public static IRTCRtpTransceiver Create(Webrtc.IRTCRtpTransceiver nativeRtpTransceiver) => 
            new RTCRtpTransceiver(nativeRtpTransceiver);

        private RTCRtpTransceiver(Webrtc.IRTCRtpTransceiver nativeRtpTransceiver) : base(nativeRtpTransceiver) { }

        public RTCRtpTransceiverDirection CurrentDirection => ((Webrtc.IRTCRtpTransceiver)NativeObject).Direction
            .FromNative();

        public RTCRtpTransceiverDirection Direction 
        { 
            get => ((Webrtc.IRTCRtpTransceiver)NativeObject).Direction.FromNative();
            set => ((Webrtc.IRTCRtpTransceiver)NativeObject).SetDirection(value.ToNative(), out NSError _);
        }

        public string Mid => ((Webrtc.IRTCRtpTransceiver)NativeObject).Mid;

        public IRTCRtpReceiver Receiver => RTCRtpReceiver.Create(((Webrtc.IRTCRtpTransceiver)NativeObject).Receiver);

        public IRTCRtpSender Sender => RTCRtpSender.Create(((Webrtc.IRTCRtpTransceiver)NativeObject).Sender);

        public bool Stopped => ((Webrtc.IRTCRtpTransceiver)NativeObject).IsStopped;


        public void SetCodecPreferences(RTCRtpCodecCapability[] codecs)
        {
            throw new NotImplementedException();
        }

        public void Stop() => ((Webrtc.IRTCRtpTransceiver)NativeObject).StopInternal();

    }
}
