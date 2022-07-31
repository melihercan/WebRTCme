using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme
{
    public interface IRTCRtpTransceiver : IDisposable // INativeObject 
    {
        RTCRtpTransceiverDirection CurrentDirection { get; }

        RTCRtpTransceiverDirection Direction { get; set; }

        string Mid { get; }

        IRTCRtpReceiver Receiver { get; }

        IRTCRtpSender Sender { get; }

        bool Stopped { get; }

        void SetCodecPreferences(RTCRtpCodecCapability[] codecs);

        void Stop();
    }
}
