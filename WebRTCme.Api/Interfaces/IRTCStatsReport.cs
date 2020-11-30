using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace WebRTCme
{
    public interface IRTCStatsReport
    {
        string Id { get; set; }

        double Timestamp { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        RTCStatsType Type { get; set; }
    }
}
