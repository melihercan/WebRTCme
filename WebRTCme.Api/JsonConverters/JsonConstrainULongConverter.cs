using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace WebRTCme.JsonConverters
{
    public class JsonConstrainULongConverter : JsonConverter<ConstrainULong>
    {
        public override ConstrainULong Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var constrainULong = new ConstrainULong();

            if (reader.TokenType == JsonTokenType.Number)
            {
                constrainULong.Single = reader.GetUInt64();
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
                    var value = reader.GetUInt64();
                    _ = (propertyName switch
                    {
                        nameof(constrainULong.Min) => constrainULong.Min = value,
                        nameof(constrainULong.Max) => constrainULong.Max = value,
                        nameof(constrainULong.Exact) => constrainULong.Exact = value,
                        nameof(constrainULong.Ideal) => constrainULong.Ideal = value,
                        _ => throw new JsonException("Invalid element in ConstrainULong object.")
                    });
                }
            }
            else
            {
                throw new JsonException("Invalid JSON format for ConstrainULong object.");
            }
            return constrainULong;
        }

        public override void Write(Utf8JsonWriter writer, ConstrainULong value, JsonSerializerOptions options)
        {
            if (value.Single != null)
            {
                writer.WriteNumberValue((ulong)value.Single);
            }
            else
            {
                writer.WriteStartObject();
                if (value.Min != null) writer.WriteNumber(nameof(value.Min), (ulong)value.Min);
                if (value.Max != null) writer.WriteNumber(nameof(value.Max), (ulong)value.Max);
                if (value.Exact != null) writer.WriteNumber(nameof(value.Exact), (ulong)value.Exact);
                if (value.Ideal != null) writer.WriteNumber(nameof(value.Ideal), (ulong)value.Ideal);
                writer.WriteEndObject();
            }
        }
    }
}
