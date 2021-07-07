using System;
using System.Buffers;

namespace SDPLib.Serializers
{
    class MediaTitleSerializer
    {
        public static readonly byte[] ReservedBytes = new byte[] { SDPSerializer.ByteLF };
        private static byte[] HeaderBytes = new byte[] { (byte)'i', (byte)'=' };
        public const byte Identifier = (byte)'i';

        public static readonly MediaTitleSerializer Instance = new MediaTitleSerializer();

        public byte[] ReadValue(ReadOnlySpan<byte> data)
        {
            var remainingSlice = data;

            //header
            SerializationHelpers.ParseRequiredHeader("Media title field", data, HeaderBytes);
            remainingSlice = remainingSlice.Slice(HeaderBytes.Length);

            //value
            return SerializationHelpers.NextRequiredField("Media title", remainingSlice).ToArray();
        }

        public void WriteValue(IBufferWriter<byte> writer, ReadOnlySpan<byte> value)
        {
            if (value == null || value.IsEmpty)
                return;

            SerializationHelpers.CheckForReserverdBytes("media title field", value, ReservedBytes);
            writer.WriteString("i=");
            writer.Write(value);
            writer.WriteString(SDPSerializer.CRLF);
        }
    }
}
