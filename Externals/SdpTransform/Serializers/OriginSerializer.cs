using System;
using System.Buffers;
using Utilme.SdpTransform;

namespace UtilmeSdpTransform.Serializers
{
    class OriginSerializer
    {
        public static readonly OriginSerializer Instance = new OriginSerializer();
        public static readonly byte[] HeaderBytes = new byte[] { (byte)'o', (byte)'=' };
        public static readonly char[] ReservedChars = new[] { (char) SdpSerializer.ByteLF, (char) SdpSerializer.ByteSpace};

    public DeserializationState ReadValue(ReadOnlySpan<byte> data, DeserializationSession session)
        {
            var remainingSlice = data;

            //header
            SerializationHelpers.ParseRequiredHeader("Origin field", remainingSlice, HeaderBytes);
            remainingSlice = remainingSlice.Slice(HeaderBytes.Length);

            session.ParsedValue.Origin = new Origin();

            //username
            session.ParsedValue.Origin.UserName =
                SerializationHelpers.ParseRequiredString("Origin field: username",
                SerializationHelpers.NextRequiredDelimitedField("Origin field: username", SdpSerializer.ByteSpace, remainingSlice, out var consumed));
            remainingSlice = remainingSlice.Slice(consumed + 1);

            //sess-id
            session.ParsedValue.Origin.SessionId = SerializationHelpers.ParseLong("Origin field: sess-id",
                SerializationHelpers.NextRequiredDelimitedField("Origin field: sess-id", SdpSerializer.ByteSpace, remainingSlice, out consumed));
            remainingSlice = remainingSlice.Slice(consumed + 1);

            //sess-version
            session.ParsedValue.Origin.SessionVersion = SerializationHelpers.ParseLong("Origin field: sess-version",
                SerializationHelpers.NextRequiredDelimitedField("Origin field: sess-version", SdpSerializer.ByteSpace, remainingSlice, out consumed));
            remainingSlice = remainingSlice.Slice(consumed + 1);

            //nettype
            session.ParsedValue.Origin.Nettype =
                SerializationHelpers.ParseRequiredString("Origin field: nettype",
                SerializationHelpers.NextRequiredDelimitedField("Origin field: nettype", SdpSerializer.ByteSpace, remainingSlice, out consumed));
            remainingSlice = remainingSlice.Slice(consumed + 1);

            //addrtype
            session.ParsedValue.Origin.AddrType =
                SerializationHelpers.ParseRequiredString("Origin field: addrtype",
                SerializationHelpers.NextRequiredDelimitedField("Origin field: addrtype", SdpSerializer.ByteSpace, remainingSlice, out consumed));
            remainingSlice = remainingSlice.Slice(consumed + 1);

            // unicast-address
            session.ParsedValue.Origin.UnicastAddress =
                SerializationHelpers.ParseRequiredString("Origin field: unicast-address", remainingSlice);

            return SessionNameSerializer.Instance.ReadValue;
        }

        public void WriteValue(IBufferWriter<byte> writer, Origin value)
        {
            if (value == null)
                throw new SerializationException("Origin must have value");

            var userName = value.UserName;
            //it is "-" if the originating host does not support the concept of user IDs
            if (value.UserName == null)
                userName = "-";
            else
            {
#if NETSTANDARD2_0
                SerializationHelpers.CheckForReserverdChars("Origin userame", value.UserName.AsSpan(), ReservedChars);
#else
                SerializationHelpers.CheckForReserverdChars("Origin userame", value.UserName, ReservedChars);
#endif
            }

            SerializationHelpers.EnsureFieldIsPresent("Origin nettype", value.Nettype);
#if NETSTANDARD2_0
            SerializationHelpers.CheckForReserverdChars("Origin nettype", value.Nettype.AsSpan(), ReservedChars);
#else
            SerializationHelpers.CheckForReserverdChars("Origin nettype", value.Nettype, ReservedChars);
#endif
            SerializationHelpers.EnsureFieldIsPresent("Origin addrtype", value.AddrType);
#if NETSTANDARD2_0
            SerializationHelpers.CheckForReserverdChars("Origin addrtype", value.AddrType.AsSpan(), ReservedChars);
#else
            SerializationHelpers.CheckForReserverdChars("Origin addrtype", value.AddrType, ReservedChars);
#endif
            SerializationHelpers.EnsureFieldIsPresent("Origin unicast address", value.UnicastAddress);
#if NETSTANDARD2_0
            SerializationHelpers.CheckForReserverdChars("Origin unicast address", value.UnicastAddress.AsSpan(), ReservedChars);
#else
            SerializationHelpers.CheckForReserverdChars("Origin unicast address", value.UnicastAddress, ReservedChars);
#endif

            var field = $"o={userName} {value.SessionId} {value.SessionVersion} {value.Nettype} {value.AddrType} {value.UnicastAddress}{SdpSerializer.CRLF}";
            writer.WriteString(field);
        }
    }
}
