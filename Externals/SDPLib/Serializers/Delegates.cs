using System;

namespace SDPLib.Serializers
{
    delegate DeserializationState DeserializationState(ReadOnlySpan<byte> data, DeserializationSession session);
}
