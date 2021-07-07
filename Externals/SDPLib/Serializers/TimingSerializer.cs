using System;
using System.Buffers;
using System.Buffers.Text;
using System.Text;

namespace SDPLib.Serializers
{
    //Timing ("t=")
    class TimingSerializer
    {
        private static readonly byte[] HeaderBytes = new byte[] { (byte)'t', (byte)'=' };
        public const byte Identifier = (byte)'t';

        public static readonly TimingSerializer Instance = new TimingSerializer();

        public TimingInfo ReadValue(ReadOnlySpan<byte>  data)
        {
            var remainingSlice = data;

            //header
            SerializationHelpers.ParseRequiredHeader("Timing field", data, HeaderBytes);
            remainingSlice = remainingSlice.Slice(HeaderBytes.Length);

            var tim = new TimingInfo();

            // StartTime
            tim.StartTime =
                SerializationHelpers.ParseLong("Timing field: startTime",
                SerializationHelpers.NextRequiredDelimitedField("Timing field: startTime", SDPSerializer.ByteSpace, remainingSlice, out var consumed));
            remainingSlice = remainingSlice.Slice(consumed + 1);

            // StopTime
            tim.StopTime =
                SerializationHelpers.ParseLong("Timing field: stoptime",
                SerializationHelpers.NextRequiredField("StopTime field: stoptime", remainingSlice));

            return tim;
        }

        public void WriteValue(IBufferWriter<byte> stream, TimingInfo value)
        {
            if (value == null)
                throw new SerializationException("Timing must have value");

            var field = $"t={value.StartTime} {value.StopTime}{SDPSerializer.CRLF}";
            stream.WriteString(field);
        }
    }
}
