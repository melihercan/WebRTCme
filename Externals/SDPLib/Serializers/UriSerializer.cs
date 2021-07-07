using System;
using System.Buffers;
using System.Linq;
using System.Text;

namespace SDPLib.Serializers
{
    class UriSerializer
    {
        private static readonly byte[] HeaderBytes = new byte[] { (byte)'u', (byte)'=' };
        public const byte Identifier = (byte)'u';

        public static readonly UriSerializer Instance = new UriSerializer();

        public DeserializationState ReadValue(ReadOnlySpan<byte> data, DeserializationSession session)
        {
            if (session.ParsedValue.Uri != null)
            {
                throw new DeserializationException("No more than one URI field is allowed per session description");
            }

            if (session.ParsedValue.MediaDescriptions != null && session.ParsedValue.MediaDescriptions.Any())
            {
                throw new DeserializationException($"Uri MUST be specified before the first media field");
            }

            var remainingSlice = data;

            //header
            SerializationHelpers.ParseRequiredHeader("Uri field", remainingSlice, HeaderBytes);
            remainingSlice = remainingSlice.Slice(HeaderBytes.Length);

            //uri
            var uriString =
                SerializationHelpers.ParseRequiredString("Uri field",
                SerializationHelpers.NextRequiredField("Uri field", remainingSlice));

            if (Uri.TryCreate(uriString, UriKind.RelativeOrAbsolute, out var parsedUri))
                session.ParsedValue.Uri = parsedUri;
            else
                throw new DeserializationException($"Invalid Uri field value: {uriString}");

            return OptionalValueDeSerializer.Instance.ReadValue;
        }

        public void WriteValue(IBufferWriter<byte> writer, Uri value)
        {
            if (value == null)
                return;

            if (!value.IsAbsoluteUri)
                throw new SerializationException("Uri field value must be absolute uri");

            var uri = $"u={value.AbsoluteUri}{SDPSerializer.CRLF}";
            writer.WriteString(uri);
        }
    }
}
