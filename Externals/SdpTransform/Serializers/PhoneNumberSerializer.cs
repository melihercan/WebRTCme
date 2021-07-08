using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using Utilme.SdpTransform;

namespace UtilmeSdpTransform.Serializers
{
    class PhoneNumberSerializer
    {
        private static byte[] HeaderBytes = new byte[] { (byte)'p', (byte)'=' };
        public const byte Identifier = (byte)'p';
        public static char[] ReservedChars = new char[] { (char)SdpSerializer.ByteLF };

        public static readonly PhoneNumberSerializer Instance = new PhoneNumberSerializer();

        public DeserializationState ReadValue(ReadOnlySpan<byte> data, DeserializationSession session)
        {
            if (session.ParsedValue.MediaDescriptions != null && session.ParsedValue.MediaDescriptions.Any())
            {
                throw new DeserializationException("Phone number field MUST be specified before the first media field");
            }

            var remainingSlice = data;

            //header
            SerializationHelpers.ParseRequiredHeader("Phone number field", remainingSlice, HeaderBytes);
            remainingSlice = remainingSlice.Slice(HeaderBytes.Length);

            //uri
            var phoneString =
                SerializationHelpers.ParseRequiredString("Phone number field",
                SerializationHelpers.NextRequiredField("Phone number field", remainingSlice));

            session.ParsedValue.PhoneNumbers = session.ParsedValue.PhoneNumbers ?? new List<string>();
            session.ParsedValue.PhoneNumbers.Add(phoneString);

            return OptionalValueDeSerializer.Instance.ReadValue;
        }

        public void WriteValue(IBufferWriter<byte> writer, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return;

#if NETSTANDARD2_0
            SerializationHelpers.CheckForReserverdChars("Phone number field", value.AsSpan(), ReservedChars);
#else
            SerializationHelpers.CheckForReserverdChars("Phone number field", value, ReservedChars);
#endif

            var field = $"p={value}{SdpSerializer.CRLF}";
            writer.WriteString(field);
        }
    }
}
