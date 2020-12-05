using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme
{
    public class RTCRtpSendParameters : RTCRtpParameters
    {
        public RTCRtpEncoodingParameters Encodings { get; set; }

        public string TransactionId { get; set; }
    }
}
