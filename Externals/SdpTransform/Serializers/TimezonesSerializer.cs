using System;
using System.Buffers;
using System.Collections.Generic;
using Utilme.SdpTransform;

namespace UtilmeSdpTransform.Serializers
{
    //Time Zones ("z=")
    class TimezonesSerializer
    {
        private static byte[] HeaderBytes = new byte[] { (byte)'z', (byte)'=' };
        public const byte Identifier = (byte)'z';

        public static readonly TimezonesSerializer Instance = new TimezonesSerializer();

        public IList<TimezoneInfo> ReadValue(ReadOnlySpan<byte>  data)
        {
            throw new NotImplementedException();
        }

        public void WriteValue(IBufferWriter<byte> writer, TimezoneInfo session)
        {
            throw new NotImplementedException();
        }
    }
}
