using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;

namespace SDPLib.Serializers
{
    class RepeatTimeSerializer
    {
        private static byte[] HeaderBytes = new byte[] { (byte)'r', (byte)'=' };
        public const byte Identifier = (byte)'r';

        public static readonly RepeatTimeSerializer Instance = new RepeatTimeSerializer();

        public RepeatTime ReadValue(ReadOnlySpan<byte> data)
        {
            var remainingSlice = data;

            //header
            SerializationHelpers.ParseRequiredHeader("Repeat Time field", remainingSlice, HeaderBytes);
            remainingSlice = remainingSlice.Slice(HeaderBytes.Length);

            var rt = new RepeatTime()
            {
                OffsetsFromStartTime = new List<TimeSpan>()
            };

            //repeat interval
            rt.RepeatInterval =
                SerializationHelpers.ParseRequiredTimespan("Repeat Time field: repeat interval",
                SerializationHelpers.NextRequiredDelimitedField("Repeat Time field: repeat interval", SDPSerializer.ByteSpace, remainingSlice, out var consumed));
            remainingSlice = remainingSlice.Slice(consumed + 1);

            //active duration
            rt.ActiveDuration =
                SerializationHelpers.ParseRequiredTimespan("Repeat Time field: active duration",
                SerializationHelpers.NextRequiredDelimitedField("Repeat Time field: active duration", SDPSerializer.ByteSpace, remainingSlice, out consumed));
            remainingSlice = remainingSlice.Slice(consumed + 1);

            //offsets from start-time
            ReadOnlySpan<byte> token;
            while ((token = SerializationHelpers.NextDelimitedField(SDPSerializer.ByteSpace, remainingSlice, out consumed)) != null)
            {
                rt.OffsetsFromStartTime.Add(SerializationHelpers.ParseRequiredTimespan("Repeat Time field: offsets from start-time are required", token));

                //There is no remaining peace left
                if (remainingSlice.Length <= consumed + 1)
                    break;

                remainingSlice = remainingSlice.Slice(consumed + 1);
            }

            return rt;
        }

        public void WriteValue(IBufferWriter<byte> writer, RepeatTime value)
        {
            if (value.OffsetsFromStartTime == null || !value.OffsetsFromStartTime.Any())
            {
                throw new SerializationException("Repeat Time field: offsets from start-time are required");
            }

            var field = $"r={value.RepeatInterval.TotalSeconds} {value.ActiveDuration.TotalSeconds}";
            writer.WriteString(field);

            foreach (var offset in value.OffsetsFromStartTime)
            {
                writer.WriteString(" " + offset.TotalSeconds.ToString());
            }

            writer.WriteString(SDPSerializer.CRLF);
        }
    }
}
