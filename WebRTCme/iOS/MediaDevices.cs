using System;
using System.Collections.Generic;
using System.Text;
using WebRTCme;

namespace WebRtc.iOS
{
    internal class MediaDevices : ApiBase<object>, IMediaDevices
    {
        public IMediaStream GetUserMedia(MediaStreamConstraints constraints)
        {
            throw new NotImplementedException();
        }
    }
}
