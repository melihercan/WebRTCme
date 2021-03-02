using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace WebRTCme
{
    public class JsonConstrainDOMStringConverter : JsonConverter<ConstrainDOMString>
    {
        public override ConstrainDOMString Read(ref Utf8JsonReader reader, Type typeToConvert, 
            JsonSerializerOptions options)
        {
            var constrainDOMString = new ConstrainDOMString();

            if (reader.TokenType == JsonTokenType.String)
            {
                constrainDOMString.Value = reader.GetString();
            }
            else if (reader.TokenType == JsonTokenType.StartArray)
            {
                var list = new List<string>();
                while (reader.Read())
                {
                    if(reader.TokenType == JsonTokenType.EndArray)
                    {
                        break;
                    }
                    var value = reader.GetString();
                    list.Add(value);
                }
                constrainDOMString.Array = list.ToArray();
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
                    switch (propertyName)
                    {
                        case nameof(constrainDOMString.Exact):
                            constrainDOMString.Exact = new ConstrainDOMStringUnion();
                            if (reader.TokenType == JsonTokenType.String)
                            {
                                constrainDOMString.Exact.Value = reader.GetString();
                            }
                            else if (reader.TokenType == JsonTokenType.StartArray)
                            {
                                var list = new List<string>();
                                while (reader.Read())
                                {
                                    if (reader.TokenType == JsonTokenType.EndArray)
                                    {
                                        break;
                                    }
                                    var value = reader.GetString();
                                    list.Add(value);
                                }
                                constrainDOMString.Exact.Array = list.ToArray();
                            }
                            break;
                        case nameof(constrainDOMString.Ideal):
                            constrainDOMString.Ideal = new ConstrainDOMStringUnion();
                            if (reader.TokenType == JsonTokenType.String)
                            {
                                constrainDOMString.Ideal.Value = reader.GetString();
                            }
                            else if (reader.TokenType == JsonTokenType.StartArray)
                            {
                                var list = new List<string>();
                                while (reader.Read())
                                {
                                    if (reader.TokenType == JsonTokenType.EndArray)
                                    {
                                        break;
                                    }
                                    var value = reader.GetString();
                                    list.Add(value);
                                }
                                constrainDOMString.Ideal.Array = list.ToArray();
                            }
                            break;

                        default:
                            throw new JsonException("Invalid element in ConstrainDOMString object.");
                    }
                }
            }
            else
            {
                throw new JsonException("Invalid JSON format for ConstrainDOMString object.");
            }
            return constrainDOMString;
        }

        public override void Write(Utf8JsonWriter writer, ConstrainDOMString value, JsonSerializerOptions options)
        {
            if (value.Value != null)
            {
                writer.WriteStringValue(value.Value);
            }
            else if (value.Array != null)
            {
                writer.WriteStartArray();
                foreach (var item in value.Array)
                {
                    writer.WriteStringValue(item);
                }
                writer.WriteEndArray();
            }
            else
            {
                writer.WriteStartObject();

                if (value.Exact != null)
                {
                    if (value.Exact.Value != null)
                    {
                        writer.WriteString(nameof(value.Exact), value.Exact.Value);
                    }
                    else if (value.Exact.Array != null)
                    {
                        writer.WriteStartArray(nameof(value.Exact));
                        foreach (var item in value.Exact.Array)
                        {
                            writer.WriteStringValue(item);
                        }
                        writer.WriteEndArray();
                    }
                }
                
                if (value.Ideal != null)
                {
                    if (value.Ideal.Value != null)
                    {
                        writer.WriteString(nameof(value.Ideal), value.Ideal.Value);
                    }
                    else if (value.Ideal.Array != null)
                    {
                        writer.WriteStartArray(nameof(value.Ideal));
                        foreach (var item in value.Ideal.Array)
                        {
                            writer.WriteStringValue(item);
                        }
                        writer.WriteEndArray();
                    }
                }

                writer.WriteEndObject();
            }
        }
    }
}
