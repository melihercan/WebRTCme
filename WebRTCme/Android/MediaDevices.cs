using System;
using System.Collections.Generic;
using System.Text;
using WebRTCme;

namespace WebRtc.Android
{
    internal class MediaDevices : ApiBase<object>, IMediaDevices
    {
        public IMediaStream GetUserMedia(MediaStreamConstraints constraints)
        {
            throw new NotImplementedException();
        }
    }
}
