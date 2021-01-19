using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace WebRTCme
{
    public class JsonCamelCaseStringEnumConverter : JsonConverterFactory
    {
        private static readonly JsonStringEnumConverter _jsonStringEnumConverter = new(JsonNamingPolicy.CamelCase);

        public override bool CanConvert(Type typeToConvert) =>
            _jsonStringEnumConverter.CanConvert(typeToConvert);

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options) =>
            _jsonStringEnumConverter.CreateConverter(typeToConvert, options);
    }
}
