using System;
using System.Collections.Generic;
using System.Text;
using WebRTCme;

namespace WebRtc.Web
{
    internal class Navigator : INavigator
    {
        public IMediaDevices MediaDevices => new MediaDevices();
    }
}
