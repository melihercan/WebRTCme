using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace WebRTCme
{
    public static class JsonHelper
    {
        // Property and value both in camel case, ignoring null and, string enum and enum member converters.
        public static JsonSerializerOptions WebRtcJsonSerializerOptions => new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters =
            {
                // Order is important. The first converter that can convert will be chosen.
                new JsonStringEnumMemberConverter(),
                new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
            }
        };
    }
}
