using System;
using System.Collections.Generic;
using System.Text;
using WebRTCme;

namespace WebRtc.Android
{
    internal class Navigator : ApiBase<object>, INavigator
    {
        public IMediaDevices MediaDevices() => new MediaDevices();
    }
}
