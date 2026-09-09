using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace WebRTCme
{
    public class JsonConstrainDoubleConverter : JsonConverter<ConstrainDouble>
    {
        public override ConstrainDouble Read(ref Utf8JsonReader reader, Type typeToConvert, 
            JsonSerializerOptions options)
        {
            var constrainDouble = new ConstrainDouble();

            if (reader.TokenType == JsonTokenType.Number)
            {
                constrainDouble.Value = reader.GetDouble();
            }
            else if (reader.TokenType == JsonTokenType.StartObject)
            {
                constrainDouble.Object = JsonSerializer.Deserialize<ConstrainDoubleRange>(ref reader, options);
            }
            else
            {
                throw new JsonException("Invalid JSON format for ConstrainDouble object.");
            }
            return constrainDouble;
        }

        public override void Write(Utf8JsonWriter writer, ConstrainDouble value, JsonSerializerOptions options)
        {
            if (value.Value != null)
            {
                writer.WriteNumberValue((double)value.Value);
            }
            else if (value.Object != null)
            {
                JsonSerializer.Serialize(writer, value.Object, options);
            }
            else
            {
                throw new JsonException("Invalid ContraintDouble object.");
            }
        }
    }
}
