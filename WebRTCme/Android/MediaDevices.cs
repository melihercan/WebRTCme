using System;
using System.Collections.Generic;
using System.Text;
using WebRTCme;

namespace WebRtc.Android
{
    internal class MediaDevices : ApiBase, IMediaDevices
    {
        public IEnumerable<IMediaDeviceInfo> EnumerateDevices()
        {
            throw new NotImplementedException();
        }

        public IMediaStream GetUserMedia(MediaStreamConstraints constraints)
        {
            throw new NotImplementedException();
        }
    }
}
