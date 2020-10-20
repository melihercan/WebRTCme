using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace WebRTCme
{
    public interface IMediaDevices : IDisposable
    {
        IMediaStream GetUserMedia(MediaStreamConstraints constraints);
    }

    public interface IMediaDevicesAsync : IAsyncDisposable
    {
        Task<IMediaStreamAsync> GetUserMediaAsync(MediaStreamConstraints constraints);
    }

}
