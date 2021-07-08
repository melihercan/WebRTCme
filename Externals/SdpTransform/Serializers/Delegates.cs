using System;

namespace UtilmeSdpTransform.Serializers
{
    delegate DeserializationState DeserializationState(ReadOnlySpan<byte> data, DeserializationSession session);
}
