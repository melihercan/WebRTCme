using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme.Middleware
{
    public class Metadata<T> : IMetadata<T>
    {
        public Metadata(T t)
        {
            Data = t;
        }

        public T Data { get; init; }

        public Type Type => typeof(T);

        object IMetadata.Data => Data;
    }
}
