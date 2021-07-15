using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace WebRTCme.Connection.MediaSoup
{
    public class VP8Parameters
    {
        [JsonPropertyName("profile-id")]
        public int? ProfileId { get; set; }

        [JsonPropertyName("x-google-start-bitrate")]
        public int? XGoogleStartBitrate { get; set; }

        [JsonPropertyName("x-google-max-bitrate")]
        public int? XGoogleMaxBitrate { get; set; }

        [JsonPropertyName("x-google-min-bitrate")]
        public int? XGoogleMinBitrate { get; set; }

    }
}
