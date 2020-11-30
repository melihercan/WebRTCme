using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace WebRTCme
{
    public interface IRTCSctpTransport
    {
        int MaxChannels { get; }

        int MaxMessageSize { get; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        RTCSctpTransportState State { get; }

        IRTCSctpTransport Transport { get; }

        event EventHandler<RTCSctpTransportState> OnStateChange;
    }
}
