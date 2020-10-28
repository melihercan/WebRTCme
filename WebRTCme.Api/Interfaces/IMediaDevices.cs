using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace WebRTCme
{
    public interface IMediaDevices : INativeObjects
    {
        IMediaStream GetUserMedia(MediaStreamConstraints constraints);
        IEnumerable<MediaDeviceInfo> EnumerateDevices(); 
    }

    public interface IMediaDevicesAsync : INativeObjectsAsync
    {
        Task<IMediaStreamAsync> GetUserMediaAsync(MediaStreamConstraints constraints);
        Task<IEnumerable<MediaDeviceInfo>> EnumerateDevicesAsync();
    }

}
