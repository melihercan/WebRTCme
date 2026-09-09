using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme
{
    public interface IBlobEvent
    {
        IBlob Data { get; }

        double Timecode { get; }
    }
}
