using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme
{
    public interface IMediaStreamEvent
    {
        IMediaStream MediaStream { get; }
    }
}
