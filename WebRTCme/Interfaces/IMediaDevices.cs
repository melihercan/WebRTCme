using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme
{
    public interface IMediaDevices
    {
        IMediaStream GetUserMedia(MediaStreamConstraints constraints);
    }
}
