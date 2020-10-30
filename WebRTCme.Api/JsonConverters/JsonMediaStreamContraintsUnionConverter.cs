using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace WebRTCme.JsonConverters
{
    public class JsonMediaStreamContraintsUnionConverter : JsonConverter<MediaStreamContraintsUnion>
    {
        public override MediaStreamContraintsUnion Read(ref Utf8JsonReader reader, Type typeToConvert, 
            JsonSerializerOptions options)
        {
            var mediaStreamConstraintsUnion = new MediaStreamContraintsUnion();

            if (reader.TokenType == JsonTokenType.True || reader.TokenType == JsonTokenType.False)
            {
                mediaStreamConstraintsUnion.Value = reader.GetBoolean();
            }
            else if (reader.TokenType == JsonTokenType.StartObject)
            {
                mediaStreamConstraintsUnion.Object = null;
            }
            else
            {
                throw new JsonException("Invalid JSON format for MediaStreamContraintsUnion object.");
            }
            return mediaStreamConstraintsUnion;
        }

        public override void Write(Utf8JsonWriter writer, MediaStreamContraintsUnion value, 
            JsonSerializerOptions options)
        {
            if (value.Value != null)
            {
                writer.WriteBooleanValue((bool)value.Value);
            }
            else if (value.Object != null)
            {
                writer.WriteStringValue(JsonSerializer.Serialize(value.Object));
            }
            else
            {
                throw new JsonException("Invalid MediaStreamContraintsUnion object.");
            }
        }
    }
}
