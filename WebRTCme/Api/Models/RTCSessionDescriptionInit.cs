using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme
{
    public record RTCSessionDescriptionInit
    {
        public RTCSdpType Type { set;  get; }

        public string Sdp { set;  get; }
    }
}
