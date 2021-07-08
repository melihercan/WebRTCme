using System;
using System.Buffers;
using System.Buffers.Text;
using System.Collections;
using System.Text;

namespace UtilmeSdpTransform.Serializers
{
    static class SerializationHelpers
    {
        public static void WriteString(this IBufferWriter<byte> writer, string s)
        {
#if NETSTANDARD2_0
            var chunk = writer.GetSpan(Encoding.UTF8.GetByteCount(s)).ToArray();
            var count = Encoding.UTF8.GetBytes(s.AsSpan().ToArray(), 0, s.AsSpan().ToArray().Length, chunk, chunk.Length);
#else
            var chunk = writer.GetSpan(Encoding.UTF8.GetByteCount(s));
            var count = Encoding.UTF8.GetBytes(s.AsSpan(), chunk);
#endif
            writer.Advance(count);
        }

        public static void WriteStringWithEncoding(this IBufferWriter<byte> writer, string s, Encoding encoding)
        {
#if NETSTANDARD2_0
            var chunk = writer.GetSpan(encoding.GetByteCount(s)).ToArray();
            var count = encoding.GetBytes(s.AsSpan().ToArray(), 0, s.AsSpan().ToArray().Length, chunk, chunk.Length);
#else
            var chunk = writer.GetSpan(encoding.GetByteCount(s));
            var count = encoding.GetBytes(s.AsSpan(), chunk);
#endif
            writer.Advance(count);
        }

        public static void CheckForReserverdChars(string fieldName, ReadOnlySpan<char> value, ReadOnlySpan<char> reservedChars)
        {
            if (value.IndexOfAny(reservedChars) != -1)
            {
                throw new SerializationException($"{fieldName} contains reserved characters");
            }
        }

        public static void CheckForReserverdBytes(string fieldName, ReadOnlySpan<byte> value, Span<byte> reservedBytes)
        {
            if (value.IndexOfAny(reservedBytes) != -1)
            {
                throw new SerializationException($"{fieldName} contains reserved characters");
            }
        }

        public static void EnsureFieldIsPresent(string fieldName, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new SerializationException($"{fieldName} is required");
            }
        }

        public static void ParseRequiredHeader(string fieldName, ReadOnlySpan<byte> data, ReadOnlySpan<byte> header)
        {
            if (!data.StartsWith(header))
            {
#if NETSTANDARD2_0
                throw new DeserializationException($"Invalid {fieldName} header expected {Encoding.UTF8.GetString(header.ToArray())}");
#else
                throw new DeserializationException($"Invalid {fieldName} header expected {Encoding.UTF8.GetString(header)}");
#endif
            }
        }

        public static ReadOnlySpan<byte> NextRequiredDelimitedField(string fieldName, byte delimiter, ReadOnlySpan<byte> data, out int consumed)
        {
            var indexOfEnd = data.IndexOf(delimiter);
            if (indexOfEnd == -1 || indexOfEnd == 0)
            {
                throw new DeserializationException($"Invalid {fieldName}, value is required");
            }

            consumed = indexOfEnd;
            return data.Slice(0, indexOfEnd);
        }

        public static ReadOnlySpan<byte> NextDelimitedField(byte delimiter, ReadOnlySpan<byte> data, out int consumed)
        {
            var indexOfEnd = data.IndexOf(delimiter);
            if (indexOfEnd == -1)
            {
                if (data.IsEmpty)
                {
                    consumed = 0;
                    return null;
                }

                consumed = data.Length;
                return data;
            }

            consumed = indexOfEnd;
            return data.Slice(0, indexOfEnd);
        }

        public static ReadOnlySpan<byte> NextRequiredField(string fieldName, ReadOnlySpan<byte> data)
        {
            if (data.Length == 0)
            {
                throw new DeserializationException($"Invalid {fieldName}, value is required");
            }
            return data;
        }

        public static ulong ParseLong(string fieldName, ReadOnlySpan<byte> data)
        {
            if (Utf8Parser.TryParse(data, out long sessId, out var bytesConsumed) && data.Length == bytesConsumed)
            {
                return (ulong)sessId;
            }
            else
            {
                throw new DeserializationException($"Invalid {fieldName}, expected required integer");
            }
        }

        public static string ParseRequiredString(string fieldName, ReadOnlySpan<byte> data)
        {
#if NETSTANDARD2_0
            var str = Encoding.UTF8.GetString(data.ToArray());
#else
            var str = Encoding.UTF8.GetString(data);
#endif
            if (string.IsNullOrWhiteSpace(str))
            {
                throw new DeserializationException($"Invalid {fieldName}, expected required string");
            }

            return str;
        }

        public static TimeSpan ParseRequiredTimespan(string fieldName, ReadOnlySpan<byte> data)
        {
            if (data.Length == 0)
            {
                throw new DeserializationException($"Invalid {fieldName}, expected required time");
            }

            var ts = TimeSpan.Zero;

            switch ((char)data[data.Length - 1])
            {
                case 'd':
                    ts = TimeSpan.FromDays(1);
                    break;
                case 'h':
                    ts = TimeSpan.FromHours(1);
                    break;
                case 'm':
                    ts = TimeSpan.FromMinutes(1);
                    break;
                case 's':
                    ts = TimeSpan.FromSeconds(1);
                    break;
                default:
                    if (Utf8Parser.TryParse(data, out long tSpanInt, out var bytesConsumed) && data.Length == bytesConsumed && tSpanInt >= 0)
                    {
                        return TimeSpan.FromSeconds(tSpanInt);
                    }
                    else
                    {
                        throw new DeserializationException($"Invalid {fieldName}, expected required string");
                    }
            }

            var numberSpan = data.Slice(0, data.Length - 1);
            if (Utf8Parser.TryParse(numberSpan, out long tSpanInt2, out var bytesConsumed2) && numberSpan.Length == bytesConsumed2 && tSpanInt2 >= 0)
            {
                return TimeSpan.FromSeconds((TimeSpan.FromSeconds(tSpanInt2).TotalSeconds * ts.TotalSeconds));
            }
            else
            {
                throw new DeserializationException($"Invalid {fieldName}, expected required string");
            }
        }
    }
}
