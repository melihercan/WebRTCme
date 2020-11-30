﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace WebRTCme
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum RTCSdpType
    {
        Answer,
        Offer,
        Pranswer,
        Rollback
    }
}
