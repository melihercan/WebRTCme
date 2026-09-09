using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme
{
    public class RTCOfferOptions : RTCOfferAnswerOptions
    {
        public bool? IceRestart { get; set; }
    }
}
