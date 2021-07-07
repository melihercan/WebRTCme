using System;
using System.Buffers;
using System.Runtime.CompilerServices;

namespace SDPLib
{
    internal static class Extensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<byte> ToSpan(this ReadOnlySequence<byte> buffer)
        {
            if (buffer.IsSingleSegment)
            {
                return buffer.First.Span;
            }
            return buffer.ToArray();
        }
    }
}
