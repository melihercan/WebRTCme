using Webrtc = Org.Webrtc;
using System;
using WebRTCme;
using Org.Webrtc;

namespace WebRtc.Android
{
    internal class RTCRtpTransceiver : ApiBase, IRTCRtpTransceiver
    {

        public static IRTCRtpTransceiver Create(Webrtc.RtpTransceiver nativeTransceiver) =>
            new RTCRtpTransceiver(nativeTransceiver);

        private RTCRtpTransceiver(RtpTransceiver nativeTransceiver) : base(nativeTransceiver)
        {
        }

        public RTCRtpTransceiverDirection CurrentDirection => throw new NotImplementedException();

        public RTCRtpTransceiverDirection Direction { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public string Mid => throw new NotImplementedException();

        public IRTCRtpReceiver Receiver => throw new NotImplementedException();

        public IRTCRtpSender Sender => throw new NotImplementedException();

        public bool Stopped => throw new NotImplementedException();


        public void SetCodecPreferences(RTCRtpCodecCapability[] codecs)
        {
            throw new NotImplementedException();
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }
    }
}