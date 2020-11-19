using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace WebRTCme
{
    public interface IMediaDevices : INativeObjects
    {
        Task<IEnumerable<MediaDeviceInfo>> EnumerateDevices();

        Task<MediaTrackSupportedConstraints> GetSupportedConstraints();

        Task<IMediaStream> GetDisplayMedia(MediaStreamConstraints constraints);

        Task<IMediaStream> GetUserMedia(MediaStreamConstraints constraints);

        public event EventHandler<IMediaStreamTrackEvent> OnDeviceChange;
    }
}
