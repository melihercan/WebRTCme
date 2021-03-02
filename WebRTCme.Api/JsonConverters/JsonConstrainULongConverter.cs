using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace WebRTCme
{
    public class JsonConstrainULongConverter : JsonConverter<ConstrainULong>
    {
        public override ConstrainULong Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var constrainULong = new ConstrainULong();

            if (reader.TokenType == JsonTokenType.Number)
            {
                constrainULong.Value = reader.GetUInt64();
            }
            else if (reader.TokenType == JsonTokenType.StartObject)
            {
                constrainULong.Object = JsonSerializer.Deserialize<ConstrainULongRange>(ref reader, options);
            }
            else
            {
                throw new JsonException("Invalid JSON format for ConstrainULong object.");
            }
            return constrainULong;
        }

        public override void Write(Utf8JsonWriter writer, ConstrainULong value, JsonSerializerOptions options)
        {
            if (value.Value != null)
            {
                writer.WriteNumberValue((ulong)value.Value);
            }
            else if (value.Object != null)
            {
                JsonSerializer.Serialize(writer, value.Object, options);
            }
            else
            {
                throw new JsonException("Invalid ContraintULong object.");
            }
        }
    }
}
