using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace WebRTCme
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum RTCDataChannelState
    {
        Connecting,
        Open,
        Closing,
        Closed
    }
}
