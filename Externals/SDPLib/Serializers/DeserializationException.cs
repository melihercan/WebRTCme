using System;

namespace SDPLib.Serializers
{
    class DeserializationException : Exception
    {
        public DeserializationException(string message, Exception inner) : base(message, inner)
        {

        }

        public DeserializationException(string message) : base(message)
        {

        }
    }
}
