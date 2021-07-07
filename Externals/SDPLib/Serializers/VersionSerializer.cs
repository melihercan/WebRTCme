using System;
using System.Buffers;
using System.Buffers.Text;
using System.Globalization;

namespace SDPLib.Serializers
{
    class VersionSerializer
    {
        public static readonly VersionSerializer Instance = new VersionSerializer();
        public static readonly byte[] HeaderBytes = new byte[] { (byte)'v', (byte)'=' };

        public int ReadValue(ReadOnlySpan<byte> data)
        {
            var remainingSlice = data;

            // header
            SerializationHelpers.ParseRequiredHeader("Protocol version field", data, HeaderBytes);
            remainingSlice = remainingSlice.Slice(HeaderBytes.Length);

            // version
            if (Utf8Parser.TryParse(remainingSlice, out int version, out var bytesConsumed)
                && remainingSlice.Length == bytesConsumed)
            {
                return version;
            }
            else
            {
                throw new DeserializationException($"Invalid protocol version field");
            }
        }

        public void WriteValue(IBufferWriter<byte> writer, int version)
        {
            var val = $"v={version.ToString(CultureInfo.InvariantCulture)}{SDPSerializer.CRLF}";
            writer.WriteString(val);
        }
    }
}