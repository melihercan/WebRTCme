using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace WebRTCme
{
    public class RTCSessionDescription
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public RTCSdpType Type { get; set; }
        public string Sdp { get; set; }

    }
}
