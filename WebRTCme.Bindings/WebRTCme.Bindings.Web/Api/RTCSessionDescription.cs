using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using WebRTCme;

namespace WebRtcJsInterop.Api
{
    internal class RTCSessionDescription : IRTCSessionDescription
    {
        public RTCSdpType Type { get; set; }

        public string Sdp { get; set; }
    }
}
