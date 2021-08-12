using System;
using System.Buffers;
using Utilme.SdpTransform;

namespace UtilmeSdpTransform.Serializers
{
    //Bandwidth ("b=")
    class BandwithSerializer
    {
        private static byte[] HeaderBytes = new byte[] { (byte)'b', (byte)'=' };
        public const byte Identifier = (byte)'b';
        public static readonly char[] ReservedChars = new[] { (char)SdpSerializer.ByteLF, (char)SdpSerializer.ByteSpace };
        public static readonly BandwithSerializer Instance = new BandwithSerializer();

        public Bandwidth ReadValue(ReadOnlySpan<byte> data)
        {
            var remainingSlice = data;

            // header
            SerializationHelpers.ParseRequiredHeader("Bandwith field", remainingSlice, HeaderBytes);
            remainingSlice = remainingSlice.Slice(HeaderBytes.Length);

            var bandwith = new Bandwidth();

            // type
            bandwith.Type =
                SerializationHelpers.ParseRequiredString("Bandwith field: Type",
                SerializationHelpers.NextRequiredDelimitedField("Bandwith field: Type", SdpSerializer.ByteColon, remainingSlice, out var consumed)).EnumFromDisplayName<BandwidthType>();
            remainingSlice = remainingSlice.Slice(consumed + 1);

            // value
            bandwith.Value =
                int.Parse(SerializationHelpers.ParseRequiredString("Bandwith field: value", remainingSlice));

            return bandwith;
        }

        public void WriteValue(IBufferWriter<byte> writer, Bandwidth value)
        {
            if (value == null)
                return;

            SerializationHelpers.EnsureFieldIsPresent("Bandwith field Type", value.Type.DisplayName());
#if NETSTANDARD2_0
            SerializationHelpers.CheckForReserverdChars("Connection Data nettype", value.Type.DisplayName().AsSpan(), ReservedChars);
#else
            SerializationHelpers.CheckForReserverdChars("Connection Data nettype", value.Type.DisplayName(), ReservedChars);
#endif

            SerializationHelpers.EnsureFieldIsPresent("Bandwith field value", value.Value.ToString());
#if NETSTANDARD2_0
            SerializationHelpers.CheckForReserverdChars("Bandwith field value", value.Value.ToString().AsSpan(), ReservedChars);
#else
            SerializationHelpers.CheckForReserverdChars("Bandwith field value", value.Value.ToString(), ReservedChars);
#endif

            var field = $"b={value.Type}:{value.Value}{SdpSerializer.CRLF}";
            writer.WriteString(field);
        }
    }
}
