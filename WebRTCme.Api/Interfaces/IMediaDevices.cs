using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace WebRTCme
{
    public interface IMediaDevices : INativeObjects
    {
        IEnumerable<MediaDeviceInfo> EnumerateDevices();

        MediaTrackSupportedConstraints GetSupportedConstraints();

        IMediaStream GetDisplayMedia(MediaStreamConstraints constraints);

        IMediaStream GetUserMedia(MediaStreamConstraints constraints);

        public event EventHandler<IMediaStreamTrackEvent> OnDeviceChange;


    }

    public interface IMediaDevicesAsync : INativeObjectsAsync
    {
        Task<IEnumerable<MediaDeviceInfo>> EnumerateDevicesAsync();

        Task<MediaTrackSupportedConstraints> GetSupportedConstraintsAsync();

        Task<IMediaStream> GetDisplayMediaAsync(MediaStreamConstraints constraints);


        Task<IMediaStreamAsync> GetUserMediaAsync(MediaStreamConstraints constraints);

        public event EventHandler<IMediaStreamTrackEvent> OnDeviceChange;
    }
}
