using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace WebRTCme
{
    public interface IRTCSessionDescription
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        RTCSdpType Type { get; set; }
        
        string Sdp { get; set; }

    }
}
