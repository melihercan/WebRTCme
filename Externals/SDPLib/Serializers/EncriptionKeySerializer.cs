using System;
using System.Buffers;

namespace SDPLib.Serializers
{
    class EncriptionKeySerializer
    {
        public static readonly char[] ReservedChars = new[] { (char)SDPSerializer.ByteLF, (char)SDPSerializer.ByteSpace };
        private static byte[] HeaderBytes = new byte[] { (byte)'k', (byte)'=' };
        public const byte Identifier = (byte)'k';

        public static readonly EncriptionKeySerializer Instance = new EncriptionKeySerializer();

        public EncriptionKey ReadValue(ReadOnlySpan<byte> data)
        {
            var remainingSlice = data;

            //header
            SerializationHelpers.ParseRequiredHeader("Encription key field", remainingSlice, HeaderBytes);
            remainingSlice = remainingSlice.Slice(HeaderBytes.Length);

            var encKey = new EncriptionKey();

            // Type
            var indexOfEnd = remainingSlice.IndexOf(SDPSerializer.ByteColon);
            // Only method must be present
            if (indexOfEnd == -1)
            {
                encKey.Method = SerializationHelpers.ParseRequiredString("Encription key field: method", remainingSlice);
                return encKey;
            }
            else
            {
                encKey.Method = SerializationHelpers.ParseRequiredString("Encription key field: method", remainingSlice.Slice(0, indexOfEnd));
                encKey.Value = SerializationHelpers.ParseRequiredString("Encription key field: value", remainingSlice.Slice(indexOfEnd + 1));
            }

            return encKey;
        }

        public void WriteValue(IBufferWriter<byte> writer, EncriptionKey value)
        {
            if (value == null)
                return;

            SerializationHelpers.EnsureFieldIsPresent("Encription key field: method", value.Method);
#if NETSTANDARD2_0
            SerializationHelpers.CheckForReserverdChars("Encription key field: method", value.Method.AsSpan(), ReservedChars);
#else
            SerializationHelpers.CheckForReserverdChars("Encription key field: method", value.Method, ReservedChars);
#endif
            writer.WriteString($"k={value.Method}");

            if (value.Value != null)
            {
#if NETSTANDARD2_0
                SerializationHelpers.CheckForReserverdChars("Encription key field: value", value.Value.AsSpan(), ReservedChars);
#else
                SerializationHelpers.CheckForReserverdChars("Encription key field: value", value.Value, ReservedChars);
#endif
                writer.WriteString($":{value.Value}");
            }

            writer.WriteString(SDPSerializer.CRLF);
        }
    }
}
