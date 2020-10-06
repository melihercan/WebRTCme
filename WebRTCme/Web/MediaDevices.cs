using System;
using System.Collections.Generic;
using System.Text;
using WebRTCme;

namespace WebRtc.Web
{
    internal class MediaDevices : IMediaDevices
    {
        public IMediaStream GetUserMedia(MediaStreamConstraints constraints)
        {
            return new MediaStream();
        }
    }
}
