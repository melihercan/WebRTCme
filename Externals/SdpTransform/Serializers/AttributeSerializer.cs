using System;
using System.Buffers;
using Utilme.SdpTransform;

namespace UtilmeSdpTransform.Serializers
{
    class AttributeSerializer
    {
        private static readonly byte[] HeaderBytes = new byte[] { (byte)'a', (byte)'=' };
        public const byte Identifier = (byte)'a';
        public static readonly char[] ReservedChars = new[] { (char)SdpSerializer.ByteLF };

        public static readonly AttributeSerializer Instance = new AttributeSerializer();

        public string ReadValue(ReadOnlySpan<byte> data)
        {
            var remainingSlice = data;

            //header
            SerializationHelpers.ParseRequiredHeader("Attribute field", remainingSlice, HeaderBytes);
            remainingSlice = remainingSlice.Slice(HeaderBytes.Length);

            var value = SerializationHelpers.ParseRequiredString("Attribute field", remainingSlice);
            return value;
        }

        public void WriteValue(IBufferWriter<byte> writer, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new SerializationException("Attribute field must have value");

#if NETSTANDARD2_0
            SerializationHelpers.CheckForReserverdChars("Attribute field", value.AsSpan(), ReservedChars);
#else
            SerializationHelpers.CheckForReserverdChars("Attribute field", value, ReservedChars);
#endif
            var field = $"a={value}{SdpSerializer.CRLF}";
            writer.WriteString(field);
        }
    }
}
