using System;
using System.Buffers;
using Utilme.SdpTransform;

namespace UtilmeSdpTransform.Serializers
{
    //Connection Data ("c=")
    class ConnectionDataSerializer
    {
        private static byte[] HeaderBytes = new byte[] { (byte)'c', (byte)'=' };
        public const byte Identifier = (byte)'c';
        public static readonly ConnectionDataSerializer Instance = new ConnectionDataSerializer();
        public static readonly char[] ReservedChars = new[] { (char)SdpSerializer.ByteLF, (char)SdpSerializer.ByteSpace };

        public ConnectionData ReadValue(ReadOnlySpan<byte> data)
        {
            var remainingSlice = data;

            // header
            SerializationHelpers.ParseRequiredHeader("Connection Data field", remainingSlice, HeaderBytes);
            remainingSlice = remainingSlice.Slice(HeaderBytes.Length);

            var connData = new ConnectionData();

            // nettype
            connData.Nettype =
                SerializationHelpers.ParseRequiredString("Connection Data field: nettype",
                SerializationHelpers.NextRequiredDelimitedField("Connection Data field: nettype", SdpSerializer.ByteSpace, remainingSlice, out var consumed));
            remainingSlice = remainingSlice.Slice(consumed + 1);

            // addrtype
            connData.AddrType =
                SerializationHelpers.ParseRequiredString("Connection Data field: addrtype",
                SerializationHelpers.NextRequiredDelimitedField("Connection Data field: addrtype", SdpSerializer.ByteSpace, remainingSlice, out consumed));
            remainingSlice = remainingSlice.Slice(consumed + 1);

            // connection-address
            connData.ConnectionAddress =
                SerializationHelpers.ParseRequiredString("Connection Data field: connection-address", remainingSlice);

            return connData;
        }

        public void WriteValue(IBufferWriter<byte> writer, ConnectionData value)
        {
            if (value == null)
                return;

            SerializationHelpers.EnsureFieldIsPresent("Connection Data nettype", value.Nettype);
#if NETSTANDARD2_0
            SerializationHelpers.CheckForReserverdChars("Connection Data nettype", value.Nettype.AsSpan(), ReservedChars);
#else
            SerializationHelpers.CheckForReserverdChars("Connection Data nettype", value.Nettype, ReservedChars);
#endif

            SerializationHelpers.EnsureFieldIsPresent("Connection Data addrtype", value.AddrType);
#if NETSTANDARD2_0
            SerializationHelpers.CheckForReserverdChars("Connection Data addrtype", value.AddrType.AsSpan(), ReservedChars);
#else
            SerializationHelpers.CheckForReserverdChars("Connection Data addrtype", value.AddrType, ReservedChars);
#endif

            SerializationHelpers.EnsureFieldIsPresent("Connection Data unicast address", value.ConnectionAddress);
#if NETSTANDARD2_0
            SerializationHelpers.CheckForReserverdChars("Connection Data unicast address", value.ConnectionAddress.AsSpan(), ReservedChars);
#else
            SerializationHelpers.CheckForReserverdChars("Connection Data unicast address", value.ConnectionAddress, ReservedChars);
#endif

            var field = $"c={value.Nettype} {value.AddrType} {value.ConnectionAddress}{SdpSerializer.CRLF}";
            writer.WriteString(field);
        }
    }
}
