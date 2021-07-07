using SDPLib.Serializers;
using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
[assembly: InternalsVisibleTo("TestSDPLib")]

namespace SDPLib
{
    public class SDPSerializer
    {
        public const byte ByteCR = (byte)'\r';
        public const byte ByteLF = (byte)'\n';
        public const byte ByteColon = (byte)':';
        public const byte ByteSpace = (byte)' ';
        public const string CRLF = "\r\n";
        public static readonly byte[] CharsetAttributePrefix = Encoding.UTF8.GetBytes("charset:");

        public static async Task<SDP> ReadSDP(PipeReader reader)
        {
            var session = new DeserializationSession();

            DeserializationState stateFn = Serializer.Instance.ReadValue;

            while (true)
            {
                ReadResult result = await reader.ReadAsync();
                ReadOnlySequence<byte> buffer = result.Buffer;
                SequencePosition? position = null;

                do
                {
                    // Look for a EOL in the buffer
                    position = buffer.PositionOf(ByteLF);

                    if (position != null)
                    {
                        stateFn = stateFn(TrimSDPLineCR(buffer.Slice(0, position.Value)), session);

                        buffer = buffer.Slice(buffer.GetPosition(1, position.Value));
                    }
                }
                while (position != null);

                reader.AdvanceTo(buffer.Start, buffer.End);

                if (result.IsCompleted)
                {
                    if (!buffer.IsEmpty)
                        stateFn = stateFn(buffer.Slice(buffer.Start, buffer.End).ToSpan(), session);

                    break;
                }
            }

            EnsureWasParsed(stateFn);

            // Mark the PipeReader as complete
            reader.Complete();

            return session.ParsedValue;
        }

        public static SDP ReadSDP(ReadOnlySpan<byte> data)
        {
            var session = new DeserializationSession();
            DeserializationState stateFn = Serializer.Instance.ReadValue;
            var remainingSlice = data;
            int position = -1;

            while ((position = remainingSlice.IndexOf(ByteLF)) != -1)
            {
                stateFn = stateFn(TrimSDPLineCR(remainingSlice.Slice(0, position)), session);
                remainingSlice = remainingSlice.Slice(position + 1);
            }

            if (remainingSlice.Length > 0)
            {
                stateFn = stateFn(remainingSlice, session);
            }

            EnsureWasParsed(stateFn);
            return session.ParsedValue;
        }

        private static void EnsureWasParsed(DeserializationState stateFn)
        {
            if (
                !(stateFn.Target.GetType() == typeof(OptionalValueDeSerializer)
                || stateFn.Target.GetType() == typeof(MediaOptioalValueDeserializer))
                )
            {
                throw new DeserializationException("Invalid EOF required fields where not deserialized");
            }
        }

        private static ReadOnlySpan<byte> TrimSDPLineCR(ReadOnlySpan<byte> line)
        {
            //Check for CR
            if (line.Length > 1 && line[line.Length - 1] == ByteCR)
            {
                line = line.Slice(0, line.Length - 1);
            }

            return line;
        }

        private static ReadOnlySpan<byte> TrimSDPLineCR(ReadOnlySequence<byte> seq)
        {
            var line = seq.ToSpan();

            return TrimSDPLineCR(line);
        }

        public static void WriteSDP(PipeWriter writer, SDP value)
        {
            Serializer.Instance.WriteValue(writer, value);
        }

        public static byte[] WriteSDP(SDP value)
        {
            var pipe = new Pipe();
            WriteSDP(pipe.Writer, value);
            pipe.Writer.Complete();
            var serialized = pipe.Reader.ReadAsync().Result.Buffer.ToArray();
            return serialized;
        }

    }
}
