using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace WebRTCme.MediaSoupClient.Models
{
    public class NumSctpStreams
    {
        [JsonPropertyName("OS")]
        public int Os { get; init; }

        [JsonPropertyName("MIS")]
        public int Mis { get; init; }
    }
}
