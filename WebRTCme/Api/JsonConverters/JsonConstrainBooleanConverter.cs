using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace WebRTCme
{
    public class JsonConstrainBooleanConverter : JsonConverter<ConstrainBoolean>
    {
        public override ConstrainBoolean Read(ref Utf8JsonReader reader, Type typeToConvert, 
            JsonSerializerOptions options)
        {
            var constrainBoolean = new ConstrainBoolean();

            if (reader.TokenType == JsonTokenType.True || reader.TokenType == JsonTokenType.False)
            {
                constrainBoolean.Value = reader.GetBoolean();
            }
            else if (reader.TokenType == JsonTokenType.StartObject)
            {
                constrainBoolean.Object = JsonSerializer.Deserialize<ConstrainBooleanParameters>(ref reader, options);
            }
            else
            {
                throw new JsonException("Invalid JSON format for ConstrainBoolean object.");
            }
            return constrainBoolean;
        }

        public override void Write(Utf8JsonWriter writer, ConstrainBoolean value, JsonSerializerOptions options)
        {
            if (value.Value != null)
            {
                writer.WriteBooleanValue((bool)value.Value);
            }
            else if (value.Object != null)
            {
                JsonSerializer.Serialize(writer, value.Object, options);
            }
            else
            {
                throw new JsonException("Invalid ContraintBoolean object.");
            }
        }
    }
}
