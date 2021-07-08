using System;

namespace UtilmeSdpTransform.Serializers
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
