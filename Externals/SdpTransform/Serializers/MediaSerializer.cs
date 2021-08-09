using System;
using System.Buffers;
using System.Linq;
using System.Text;
using Utilme.SdpTransform;

namespace UtilmeSdpTransform.Serializers
{
    class MediaSerializer
    {
        public static readonly char[] ReservedChars = new[] { (char)SdpSerializer.ByteLF, (char)SdpSerializer.ByteSpace };
        private static byte[] HeaderBytes = new byte[] { (byte)'m', (byte)'=' };
        public const byte Identifier = (byte)'m';

        public static readonly MediaSerializer Instance = new MediaSerializer();

        public MediaDescription ReadValue(ReadOnlySpan<byte> data)
        {
            var remainingSlice = data;

            //header
            SerializationHelpers.ParseRequiredHeader("Media field", remainingSlice, HeaderBytes);
            remainingSlice = remainingSlice.Slice(HeaderBytes.Length);

            var mDescr = new MediaDescription();

            // Media
            mDescr.Media =
                 SerializationHelpers.ParseRequiredString("Media field: Media",
                 SerializationHelpers.NextRequiredDelimitedField("Media field: Media", SdpSerializer.ByteSpace, remainingSlice, out var consumed)).EnumFromDisplayName<MediaType>();
            remainingSlice = remainingSlice.Slice(consumed + 1);

            // port
            mDescr.Port = int.Parse(
                 SerializationHelpers.ParseRequiredString("Media field: Port",
                 SerializationHelpers.NextRequiredDelimitedField("Media field: Port", SdpSerializer.ByteSpace, remainingSlice, out consumed)));
            remainingSlice = remainingSlice.Slice(consumed + 1);

            // Proto
            mDescr.Proto =
                SerializationHelpers.ParseRequiredString("Media field: Proto",
                SerializationHelpers.NextRequiredDelimitedField("Media field: Proto", SdpSerializer.ByteSpace, remainingSlice, out consumed));
            remainingSlice = remainingSlice.Slice(consumed + 1);

            // fmt
            if (remainingSlice.Length == 0)
            {
                throw new DeserializationException("Invalid Media field: fmt, expected required values");
            }
            else
            {
#if NETSTANDARD2_0
                mDescr.Fmts = Encoding.UTF8.GetString(remainingSlice.ToArray()).Split((char)SdpSerializer.ByteSpace);
#else
                mDescr.Fmts = Encoding.UTF8.GetString(remainingSlice).Split((char)SdpSerializer.ByteSpace);
#endif
            }

            return mDescr;
        }

        public void WriteValue(IBufferWriter<byte> writer, MediaDescription value)
        {
            if (value == null)
                throw new SerializationException("Media field must have value");

            SerializationHelpers.EnsureFieldIsPresent("Media field: Media", value.Media.DisplayName());
#if NETSTANDARD2_0
            SerializationHelpers.CheckForReserverdChars("Media field: Media", value.Media.DisplayName().AsSpan(), ReservedChars);
#else
            SerializationHelpers.CheckForReserverdChars("Media field: Media", value.Media.DisplayName(), ReservedChars);
#endif

            SerializationHelpers.EnsureFieldIsPresent("Media field: Port", value.Port.ToString());
#if NETSTANDARD2_0
            SerializationHelpers.CheckForReserverdChars("Media field: Port", value.Port.ToString().AsSpan(), ReservedChars);
#else
            SerializationHelpers.CheckForReserverdChars("Media field: Port", value.Port.ToString(), ReservedChars);
#endif


            SerializationHelpers.EnsureFieldIsPresent("Media field: Proto", value.Proto);
#if NETSTANDARD2_0
            SerializationHelpers.CheckForReserverdChars("Media field: Proto", value.Proto.AsSpan(), ReservedChars);
#else
            SerializationHelpers.CheckForReserverdChars("Media field: Proto", value.Proto, ReservedChars);
#endif

            if (value.Fmts == null || !value.Fmts.Any())
            {
                throw new SerializationException("Invalid Media field: fmt, expected required values");
            }

            var field = $"m={value.Media} {value.Port} {value.Proto}";
            writer.WriteString(field);

            foreach(var fmt in value.Fmts)
            {
                SerializationHelpers.EnsureFieldIsPresent("Media field: fmt", fmt);
#if NETSTANDARD2_0
                SerializationHelpers.CheckForReserverdChars("Media field: fmt", fmt.AsSpan(), ReservedChars);
#else
                SerializationHelpers.CheckForReserverdChars("Media field: fmt", fmt, ReservedChars);
#endif
                writer.WriteString($" {fmt}");
            }

            writer.WriteString(SdpSerializer.CRLF);
        }
    }
}
