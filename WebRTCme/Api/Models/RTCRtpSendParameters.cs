using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme
{
    public class RTCRtpSendParameters : RTCRtpParameters
    {
        public RTCRtpEncodingParameters[] Encodings { get; set; }

        public string TransactionId { get; init; }
    }
}
