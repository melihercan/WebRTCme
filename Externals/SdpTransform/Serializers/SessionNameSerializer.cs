using System;
using System.Buffers;
using Utilme.SdpTransform;

namespace UtilmeSdpTransform.Serializers
{
    class SessionNameSerializer
    {
        public static readonly SessionNameSerializer Instance = new SessionNameSerializer();
        private static readonly byte[] HeaderBytes = new byte[] { (byte)'s', (byte)'=' };
        public static readonly byte[] ReservedBytes = new byte[] { SdpSerializer.ByteLF };

        public DeserializationState ReadValue(ReadOnlySpan<byte> data, DeserializationSession session)
        {
            var remainingSlice = data;
            //header
            SerializationHelpers.ParseRequiredHeader("Session name", data, HeaderBytes);
            remainingSlice = remainingSlice.Slice(HeaderBytes.Length);

            //value
            session.ParsedValue.SessionName = SerializationHelpers.NextRequiredField("Session name", remainingSlice).ToArray();
            return OptionalValueDeSerializer.Instance.ReadValue;
        }

        public void WriteValue(IBufferWriter<byte> writer, ReadOnlySpan<byte> value)
        {
            if (value == null || value.IsEmpty)
            {
                writer.WriteString($"s= {SdpSerializer.CRLF}");
            }
            else
            {
                SerializationHelpers.CheckForReserverdBytes("Session Name", value, ReservedBytes);
                writer.WriteString("s=");
                writer.Write(value);
                writer.WriteString(SdpSerializer.CRLF);
            }
        }
    }
}
