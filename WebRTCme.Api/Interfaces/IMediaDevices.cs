using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace WebRTCme
{
    public interface IMediaDevices : INativeObject
    {
        IMediaStream GetUserMedia(MediaStreamConstraints constraints);
    }

    public interface IMediaDevicesAsync : INativeObjectAsync
    {
        Task<IMediaStreamAsync> GetUserMediaAsync(MediaStreamConstraints constraints);
    }

}
