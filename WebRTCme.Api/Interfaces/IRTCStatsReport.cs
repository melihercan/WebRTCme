using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace WebRTCme
{
    public interface IRTCStatsReport : INativeObject
    {
        string Id { get; set; }

        double Timestamp { get; set; }

        RTCStatsType Type { get; set; }
    }
}
