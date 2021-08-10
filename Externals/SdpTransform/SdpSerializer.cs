using UtilmeSdpTransform.Serializers;
using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UtilmeSdpTransform;

namespace Utilme.SdpTransform
{
    public class SdpSerializer
    {
        public const string AttributeCharacter = "a=";
        public const byte ByteCR = (byte)'\r';
        public const byte ByteLF = (byte)'\n';
        public const byte ByteColon = (byte)':';
        public const byte ByteSpace = (byte)' ';
        public const string CRLF = "\r\n";
        public static readonly byte[] CharsetAttributePrefix = Encoding.UTF8.GetBytes("charset:");


        public static async Task<SdpOld> ReadSdp(PipeReader reader)
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
                        stateFn = stateFn(TrimSdpLineCR(buffer.Slice(0, position.Value)), session);

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

        public static SdpOld ReadSdp(ReadOnlySpan<byte> data)
        {
            var session = new DeserializationSession();
            DeserializationState stateFn = Serializer.Instance.ReadValue;
            var remainingSlice = data;
            int position = -1;

            while ((position = remainingSlice.IndexOf(ByteLF)) != -1)
            {
                stateFn = stateFn(TrimSdpLineCR(remainingSlice.Slice(0, position)), session);
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

        private static ReadOnlySpan<byte> TrimSdpLineCR(ReadOnlySpan<byte> line)
        {
            //Check for CR
            if (line.Length > 1 && line[line.Length - 1] == ByteCR)
            {
                line = line.Slice(0, line.Length - 1);
            }

            return line;
        }

        private static ReadOnlySpan<byte> TrimSdpLineCR(ReadOnlySequence<byte> seq)
        {
            var line = seq.ToSpan();

            return TrimSdpLineCR(line);
        }

        public static void WriteSdp(PipeWriter writer, SdpOld value)
        {
            Serializer.Instance.WriteValue(writer, value);
        }

        public static byte[] WriteSdp(SdpOld value)
        {
            var pipe = new Pipe();
            WriteSdp(pipe.Writer, value);
            pipe.Writer.Complete();
            var serialized = pipe.Reader.ReadAsync().Result.Buffer.ToArray();
            return serialized;
        }

        public static void DumpSdp(SdpOld sdp)
        {
            Console.WriteLine("================================ SDP OBJECT START ================================");
            Console.WriteLine($"Version:{sdp.Version}");
            Console.WriteLine($"Origin.UserName:{sdp.Origin.UserName}");
            Console.WriteLine($"Origin.SessionId:{sdp.Origin.SessionId}");
            Console.WriteLine($"Origin.SessionVersion:{sdp.Origin.SessionVersion}");
            Console.WriteLine($"Origin.Nettype:{sdp.Origin.NetType}");
            Console.WriteLine($"Origin.AddrType:{sdp.Origin.AddrType}");
            Console.WriteLine($"Origin.UnicastAddress:{sdp.Origin.UnicastAddress}");
            Console.WriteLine($"SessionName:{Encoding.UTF8.GetString(sdp.SessionName)}");
            if (sdp.SessionInformation is not null)
                Console.WriteLine($"SessionInformation:{Encoding.UTF8.GetString(sdp.SessionInformation)}");
            if (sdp.ConnectionData is not null)
            {
                Console.WriteLine($"ConnectionData.Nettype:{sdp.ConnectionData.NetType}");
                Console.WriteLine($"ConnectionData.AddrType:{sdp.ConnectionData.AddrType}");
                Console.WriteLine($"ConnectionData.ConnectionAddress:{sdp.ConnectionData.ConnectionAddress}");
            }
            foreach (var item in sdp.Timings)
            {
                Console.WriteLine($"Timing.StartTime:{item.StartTime}");
                Console.WriteLine($"Timing.StopTime:{item.StopTime}");
            }
            Console.WriteLine($"Uri:{sdp.Uri}");
            if (sdp.EmailNumbers is not null)
            {
                foreach (var item in sdp.EmailNumbers)
                {
                    Console.WriteLine($"EmailNumbers:{item}");
                }
            }
            if (sdp.PhoneNumbers is not null)
            {
                foreach (var item in sdp.PhoneNumbers)
                {
                    Console.WriteLine($"PhoneNumbers:{item}");
                }
            }
            if (sdp.BandWiths is not null)
            {
                foreach (var item in sdp.BandWiths)
                {
                    Console.WriteLine($"BandWiths.Type:{item.Type}");
                    Console.WriteLine($"BandWidth.Value:{item.Value}");
                }
            }
            if (sdp.RepeatTimes is not null)
            {
                foreach (var item in sdp.RepeatTimes)
                {
                    Console.WriteLine($"RepeatTime.RepeatInterval:{item.RepeatInterval}");
                    Console.WriteLine($"RepeatTime.ActiveDuration:{item.ActiveDuration}");
                    foreach (var subitem in item.OffsetsFromStartTime)
                    {
                        Console.WriteLine($"RepeatTime.OffsetsFromStartTime:{subitem}");
                    }
                }
            }
            if (sdp.TimeZones is not null)
            {

                foreach (var item in sdp.TimeZones)
                {
                    Console.WriteLine($"TimeZones.AdjustmentTime:{item.AdjustmentTime}");
                    Console.WriteLine($"TimeZones.Offset:{item.Offset}");
                }
            }
            if (sdp.EncriptionKey is not null)
            {
                Console.WriteLine($"EncriptionKey.Method:{sdp.EncriptionKey.Method}");
                Console.WriteLine($"EncriptionKey.Value:{sdp.EncriptionKey.Value}");
            }
            foreach (var item in sdp.Attributes)
            {
                Console.WriteLine($"Attributes:{item}");
            }
            foreach (var item in sdp.MediaDescriptions)
            {
                Console.WriteLine($"-------- MediaDescription");
                Console.WriteLine($"MediaDescriptions.Media:{item.Media}");
                Console.WriteLine($"MediaDescriptions.Port:{item.Port}");
                Console.WriteLine($"MediaDescriptions.Proto:{item.Proto}");
                foreach (var subitem in item.Fmts)
                {
                    Console.WriteLine($"MediaDescriptions.Fmts:{subitem}");
                }
                if (item.Information is not null)
                {
                    Console.WriteLine($"MediaDescriptions.Title:{item.Information}");
                }
                if (item.ConnectionData is not null)
                {
                    Console.WriteLine($"MediaDescriptions.ConnectionInfo.Nettype:{item.ConnectionData.NetType}");
                    Console.WriteLine($"MediaDescriptions.ConnectionInfo.AddrType:{item.ConnectionData.AddrType}");
                    Console.WriteLine($"MediaDescriptions.ConnectionInfo.ConnectionAddress:{item.ConnectionData.ConnectionAddress}");
                }
                if (item.Bandwidths is not null)
                {
                    foreach (var subitem in item.Bandwidths)
                    {
                        Console.WriteLine($"MediaDescriptions.Bandwidths.Type:{subitem.Type}");
                        Console.WriteLine($"MediaDescriptions.Bandwidths.Value:{subitem.Value}");
                    }
                }
                if (item.EncryptionKey is not null)
                {
                    Console.WriteLine($"MediaDescriptions.EncriptionKey.Method:{item.EncryptionKey.Method}");
                    Console.WriteLine($"MediaDescriptions.EncriptionKey.Value:{item.EncryptionKey.Method}");
                }
                foreach (var subitem in item.AttributesOld)
                {
                    Console.WriteLine($"MediaDescriptions.Attributes:{subitem}");
                }
            }
            Console.WriteLine("================================ SDP OBJECT END   ================================");
        }
    }
}
