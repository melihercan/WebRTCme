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
                new JsonStringEnumConverter(JsonNamingPolicy.CamelCase),
                new JsonStringEnumMemberConverter()
            }
        };
    }
}
