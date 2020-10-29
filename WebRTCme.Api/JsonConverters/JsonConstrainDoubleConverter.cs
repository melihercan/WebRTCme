using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace WebRTCme.JsonConverters
{
    public class JsonConstrainDoubleConverter : JsonConverter<ConstrainDouble>
    {
        public override ConstrainDouble Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var constrainDouble = new ConstrainDouble();

            if (reader.TokenType == JsonTokenType.Number)
            {
                constrainDouble.Single = reader.GetDouble();
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
                    var value = reader.GetDouble();
                    _ = propertyName switch
                    {
                        nameof(constrainDouble.Min) => constrainDouble.Min = value,
                        nameof(constrainDouble.Max) => constrainDouble.Max = value,
                        nameof(constrainDouble.Exact) => constrainDouble.Exact = value,
                        nameof(constrainDouble.Ideal) => constrainDouble.Ideal = value,
                        _ => throw new JsonException("Invalid element in ConstrainDouble object.")
                    };
                }
            }
            else
            {
                throw new JsonException("Invalid JSON format for ConstrainDouble object.");
            }
            return constrainDouble;
        }

        public override void Write(Utf8JsonWriter writer, ConstrainDouble value, JsonSerializerOptions options)
        {
            if (value.Single != null)
            {
                writer.WriteNumberValue((double)value.Single);
            }
            else
            {
                writer.WriteStartObject();
                if (value.Min != null) writer.WriteNumber(nameof(value.Min), (double)value.Min);
                if (value.Max != null) writer.WriteNumber(nameof(value.Max), (double)value.Max);
                if (value.Exact != null) writer.WriteNumber(nameof(value.Exact), (double)value.Exact);
                if (value.Ideal != null) writer.WriteNumber(nameof(value.Ideal), (double)value.Ideal);
                writer.WriteEndObject();
            }
        }
    }
}
