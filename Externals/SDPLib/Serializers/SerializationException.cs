using System;

namespace SDPLib.Serializers
{
    public class SerializationException : Exception
    {
        public SerializationException(string message, Exception inner) : base(message, inner)
        {

        }

        public SerializationException(string message) : base(message)
        {

        }
    }
}
