using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace WebRTCme
{
    public interface IMediaDevices : IAsyncDisposable
    {
        Task<IMediaStream> GetUserMedia(MediaStreamConstraints constraints);
    }
}
