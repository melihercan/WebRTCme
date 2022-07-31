using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace WebRTCme
{
    public interface IMediaDevices : IDisposable // INativeObject
    {
        public event EventHandler<IMediaStreamTrackEvent> OnDeviceChange;

        Task<MediaDeviceInfo[]> EnumerateDevices();

        MediaTrackSupportedConstraints GetSupportedConstraints();

        Task<IMediaStream> GetDisplayMedia(MediaStreamConstraints constraints);

        Task<IMediaStream> GetUserMedia(MediaStreamConstraints constraints);
    }
}
