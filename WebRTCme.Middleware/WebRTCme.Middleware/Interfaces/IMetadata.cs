using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme.Middleware
{
    public interface IMetadata
    {
        Type Type { get; }
        object Data { get; }
    }

    public interface IMetadata<T> : IMetadata
    {
        new T Data { get; }
    }
}
