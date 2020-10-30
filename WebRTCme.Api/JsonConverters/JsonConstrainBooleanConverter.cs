using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace WebRTCme.JsonConverters
{
    public class JsonConstrainBooleanConverter : JsonConverter<ConstrainBoolean>
    {
        public override ConstrainBoolean Read(ref Utf8JsonReader reader, Type typeToConvert, 
            JsonSerializerOptions options)
        {
            var constrainBoolean = new ConstrainBoolean();

            if (reader.TokenType == JsonTokenType.True || reader.TokenType == JsonTokenType.False)
            {
                constrainBoolean.Single = reader.GetBoolean();
            }
            else if (reader.TokenType == JsonTokenType.StartObject)
            {
                while (reader.Read())
                {
                    if (reader.TokenType == JsonTokenType.EndObject)
                    {
                        break;
                    }
                    var propertyName = reader.GetString();
                    reader.Read();
                    var value = reader.GetBoolean();
                    _ = propertyName switch
                    {
                        nameof(constrainBoolean.Exact) => constrainBoolean.Exact = value,
                        nameof(constrainBoolean.Ideal) => constrainBoolean.Ideal = value,
                        _ => throw new JsonException("Invalid element in ConstrainBoolean object.")
                    };
                }
            }
            else
            {
                throw new JsonException("Invalid JSON format for ConstrainBoolean object.");
            }
            return constrainBoolean;
        }

        public override void Write(Utf8JsonWriter writer, ConstrainBoolean value, JsonSerializerOptions options)
        {
            if (value.Single != null)
            {
                writer.WriteBooleanValue((bool)value.Single);
            }
            else
            {
                writer.WriteStartObject();
                if (value.Exact != null) writer.WriteBoolean(nameof(value.Exact), (bool)value.Exact);
                if (value.Ideal != null) writer.WriteBoolean(nameof(value.Ideal), (bool)value.Ideal);
                writer.WriteEndObject();
            }
        }
    }
}
