﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace WebRTCme.MediaSoupClient.Models
{
    class OpusParameters
    {
        [JsonPropertyName("useinbandfec")]
        public int? UseInbandFec { get; init; }

        [JsonPropertyName("usedtx")]
        public int? UsedTx { get; init; }
    }
}