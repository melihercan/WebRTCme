using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme
{
    public interface IMediaStreamTrack : IDisposable
    {
    }

    public interface IMediaStreamTrackAsync : IAsyncDisposable
    {
    }
}
