using System;
using System.Buffers;
using System.Collections.Generic;

namespace SDPLib.Serializers
{
    //Time Zones ("z=")
    class SDPTimezonesSerializer
    {
        private static byte[] HeaderBytes = new byte[] { (byte)'z', (byte)'=' };
        public const byte Identifier = (byte)'z';

        public static readonly SDPTimezonesSerializer Instance = new SDPTimezonesSerializer();

        public IList<SDPTimezoneInfo> ReadValue(ReadOnlySpan<byte>  data)
        {
            throw new NotImplementedException();
        }

        public void WriteValue(IBufferWriter<byte> writer, SDPTimezoneInfo session)
        {
            throw new NotImplementedException();
        }
    }
}
