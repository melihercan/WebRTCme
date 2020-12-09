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
