using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utilme.SdpTransform;

namespace UtilmeSdpTransform.Serializers
{
    class MediaOptioalValueDeserializer
    {
        public static readonly MediaOptioalValueDeserializer Instance = new MediaOptioalValueDeserializer();

        public DeserializationState ReadValue(ReadOnlySpan<byte> data, DeserializationSession session)
        {
            if (data.Length == 0)
            {
                throw new DeserializationException("Invalid sdp field");
            }

            var media = session.ParsedValue.MediaDescriptions.LastOrDefault();
            if (media == null)
            {
                throw new DeserializationException($"Media fields must start after m= field");
            }

            switch (data[0])
            {
                case MediaTitleSerializer.Identifier:
                    media.Information = Encoding.UTF8.GetString(MediaTitleSerializer.Instance.ReadValue(data));
                    break;
                case ConnectionDataSerializer.Identifier:
                    media.ConnectionData = ConnectionDataSerializer.Instance.ReadValue(data);
                    break;
                case BandwithSerializer.Identifier:
                    media.Bandwidths = media.Bandwidths ?? new List<Bandwidth>();
                    media.Bandwidths.Add(BandwithSerializer.Instance.ReadValue(data));
                    break;
                case EncriptionKeySerializer.Identifier:
                    media.EncriptionKey = EncriptionKeySerializer.Instance.ReadValue(data);
                    break;
                case AttributeSerializer.Identifier:
                    media.Attributes = media.Attributes ?? new List<string>();
                    media.Attributes.Add(AttributeSerializer.Instance.ReadValue(data));
                    break;
                case MediaSerializer.Identifier:
                    session.ParsedValue.MediaDescriptions = session.ParsedValue.MediaDescriptions ?? new List<MediaDescription>();
                    session.ParsedValue.MediaDescriptions.Add(MediaSerializer.Instance.ReadValue(data));
                    return Instance.ReadValue;
                default:
                    throw new DeserializationException($"Unknown field {(char)data[0]}");
            }

            return this.ReadValue;
        }
    }
}
