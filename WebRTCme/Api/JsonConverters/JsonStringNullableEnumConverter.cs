using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace WebRTCme
{
    public class JsonStringNullableEnumConverter<T> : JsonConverter<T>
    {
        private readonly Type _underlyingType;

        public JsonStringNullableEnumConverter()
        {
            _underlyingType = Nullable.GetUnderlyingType(typeof(T));
        }

        public override bool CanConvert(Type typeToConvert)
        {
            return typeof(T).IsAssignableFrom(typeToConvert);
        }

        public override T Read(ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            string value = reader.GetString();
            if (String.IsNullOrEmpty(value)) return default;

            try
            {
                return (T)Enum.Parse(_underlyingType, value);
            }
            catch (ArgumentException ex)
            {
                throw new JsonException("Invalid value.", ex);
            }
        }

        public override void Write(Utf8JsonWriter writer,
            T value,
            JsonSerializerOptions options)
        {
            writer.WriteStringValue(value?.ToString());
        }
    }
}
