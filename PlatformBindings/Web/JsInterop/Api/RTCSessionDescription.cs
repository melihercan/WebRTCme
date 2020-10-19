using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;
using WebRTCme;

namespace WebRtcJsInterop.Api
{
    internal class RTCSessionDescription : IRTCSessionDescription
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public RTCSdpType Type { get; set; }

        public string Sdp { get; set; }
    }
}
