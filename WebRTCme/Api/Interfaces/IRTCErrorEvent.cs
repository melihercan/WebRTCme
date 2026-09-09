using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme
{
    public interface IRTCErrorEvent : IDisposable // INativeObject
    {
        IRTCError Error { get; }
    }
}
