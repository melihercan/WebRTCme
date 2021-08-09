using System;
using System.Buffers;
using Utilme.SdpTransform;

namespace UtilmeSdpTransform.Serializers
{
    class EncriptionKeySerializer
    {
        public static readonly char[] ReservedChars = new[] { (char)SdpSerializer.ByteLF, (char)SdpSerializer.ByteSpace };
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
            var indexOfEnd = remainingSlice.IndexOf(SdpSerializer.ByteColon);
            // Only method must be present
            if (indexOfEnd == -1)
            {
                encKey.Method = SerializationHelpers.ParseRequiredString("Encription key field: method", remainingSlice).EnumFromDisplayName<EncryptionKeyMethod>();
                return encKey;
            }
            else
            {
                encKey.Method = SerializationHelpers.ParseRequiredString("Encription key field: method", remainingSlice.Slice(0, indexOfEnd)).EnumFromDisplayName<EncryptionKeyMethod>();
                encKey.Value = SerializationHelpers.ParseRequiredString("Encription key field: value", remainingSlice.Slice(indexOfEnd + 1));
            }

            return encKey;
        }

        public void WriteValue(IBufferWriter<byte> writer, EncriptionKey value)
        {
            if (value == null)
                return;

            SerializationHelpers.EnsureFieldIsPresent("Encription key field: method", value.Method.DisplayName());
#if NETSTANDARD2_0
            SerializationHelpers.CheckForReserverdChars("Encription key field: method", value.Method.DisplayName().AsSpan(), ReservedChars);
#else
            SerializationHelpers.CheckForReserverdChars("Encription key field: method", value.Method.DisplayName(), ReservedChars);
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

            writer.WriteString(SdpSerializer.CRLF);
        }
    }
}
