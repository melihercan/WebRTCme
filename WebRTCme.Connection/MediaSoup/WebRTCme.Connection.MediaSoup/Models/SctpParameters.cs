using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace WebRTCme.Connection.MediaSoup
{
    public class SctpParameters
    {
        public int Port { get; init; }

        [JsonPropertyName("OS")]
        public int Os { get; init; }

        [JsonPropertyName("MIS")]
        public int Mis { get; init; }

        public int MaxMessageSize { get; init; }


    }
}
