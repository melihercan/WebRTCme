using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme
{
    public class RTCRtpTransceiverInit
    {
        public RTCRtpTransceiverDirection? Direction { get; set; } 
        public RTCRtpEncodingParameters[] SendEncodings { get; set; }
        public IMediaStream[] Streams { get; set; }
    }
}
