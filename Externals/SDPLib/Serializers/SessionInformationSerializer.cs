using System;
using System.Buffers;

namespace SDPLib.Serializers
{
    //Session Information ("i=")
    class SessionInformationSerializer
    {
        public readonly byte[] HeaderBytes = new byte[] { (byte)'i', (byte)'=' };
        public const byte Identifier = (byte)'i';
        public static readonly byte[] ReservedBytes = new byte[] { SDPSerializer.ByteLF };

        public static readonly SessionInformationSerializer Instance = new SessionInformationSerializer();

        public DeserializationState ReadValue(ReadOnlySpan<byte> data, DeserializationSession session)
        {
            var remainingSlice = data;
            //header
            SerializationHelpers.ParseRequiredHeader("Session information", data, HeaderBytes);
            remainingSlice = remainingSlice.Slice(HeaderBytes.Length);

            //value
            session.ParsedValue.SessionInformation = SerializationHelpers.NextRequiredField("Session information", remainingSlice).ToArray();

            return OptionalValueDeSerializer.Instance.ReadValue;
        }

        public void WriteValue(IBufferWriter<byte> writer, ReadOnlySpan<byte> value)
        {
            if(value == null || value.IsEmpty)
                return;

            SerializationHelpers.CheckForReserverdBytes("Session information", value, ReservedBytes);
            writer.WriteString("i=");
            writer.Write(value);
            writer.WriteString(SDPSerializer.CRLF);
        }
    }
}