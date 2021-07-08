using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using Utilme.SdpTransform;

namespace UtilmeSdpTransform.Serializers
{
    //Email Address ("e=")
    class EmailAddressSerializer
    {
        private static byte[] HeaderBytes = new byte[] { (byte)'e', (byte)'=' };
        public const byte Identifier = (byte)'e';
        public static char[] ReservedChars = new char[] { (char)SdpSerializer.ByteLF };

        public static readonly EmailAddressSerializer Instance = new EmailAddressSerializer();

        public DeserializationState ReadValue(ReadOnlySpan<byte> data, DeserializationSession session)
        {
            if (session.ParsedValue.MediaDescriptions != null && session.ParsedValue.MediaDescriptions.Any())
            {
                throw new DeserializationException("Email address MUST be specified before the first media field");
            }

            var remainingSlice = data;

            //header
            SerializationHelpers.ParseRequiredHeader("Email field", remainingSlice, HeaderBytes);
            remainingSlice = remainingSlice.Slice(HeaderBytes.Length);

            //uri
            var emailString =
                SerializationHelpers.ParseRequiredString("Email field",
                SerializationHelpers.NextRequiredField("Email field", remainingSlice));

            session.ParsedValue.EmailNumbers = session.ParsedValue.EmailNumbers ?? new List<string>();
            session.ParsedValue.EmailNumbers.Add(emailString);
            return OptionalValueDeSerializer.Instance.ReadValue;
        }

        public void WriteValue(IBufferWriter<byte> writer, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return;

#if NETSTANDARD2_0
            SerializationHelpers.CheckForReserverdChars("Email field", value.AsSpan(), ReservedChars);
#else
            SerializationHelpers.CheckForReserverdChars("Email field", value, ReservedChars);
#endif

            var field = $"e={value}{SdpSerializer.CRLF}";
            writer.WriteString(field);
        }
    }
}
