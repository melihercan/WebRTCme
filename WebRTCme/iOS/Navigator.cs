using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebRTCme;

namespace WebRtc.iOS
{
    internal class Navigator : ApiBase, INavigator
    {
        public static INavigator Create() => new Navigator();
       
        public IMediaDevices MediaDevices => iOS.MediaDevices.Create();
    }
}
