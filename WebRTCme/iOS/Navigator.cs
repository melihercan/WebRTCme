using System;
using System.Collections.Generic;
using System.Text;
using WebRTCme;

namespace WebRtc.iOS
{
    internal class Navigator : ApiBase, INavigator
    {
        public IMediaDevices MediaDevices() => new MediaDevices();
    }
}
