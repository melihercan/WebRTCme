using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebRTCme;

namespace WebRtc.Android
{
    internal class MediaDevices : ApiBase, IMediaDevices
    {
        public event EventHandler<IMediaStreamTrackEvent> OnDeviceChange;

        public IEnumerable<MediaDeviceInfo> EnumerateDevices()
        {
            throw new NotImplementedException();
        }

        public IMediaStream GetDisplayMedia(MediaStreamConstraints constraints)
        {
            throw new NotImplementedException();
        }

        public MediaTrackSupportedConstraints GetSupportedConstraints()
        {
            throw new NotImplementedException();
        }

        public IMediaStream GetUserMedia(MediaStreamConstraints constraints)
        {
            throw new NotImplementedException();
        }

        Task<IEnumerable<MediaDeviceInfo>> IMediaDevices.EnumerateDevices()
        {
            throw new NotImplementedException();
        }

        Task<IMediaStream> IMediaDevices.GetDisplayMedia(MediaStreamConstraints constraints)
        {
            throw new NotImplementedException();
        }

        Task<MediaTrackSupportedConstraints> IMediaDevices.GetSupportedConstraints()
        {
            throw new NotImplementedException();
        }

        Task<IMediaStream> IMediaDevices.GetUserMedia(MediaStreamConstraints constraints)
        {
            throw new NotImplementedException();
        }
    }
}
