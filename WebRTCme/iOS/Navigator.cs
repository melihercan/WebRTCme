using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebRTCme;

namespace WebRtc.iOS
{
    internal class Navigator : ApiBase, INavigator
    {
        public Task<IMediaDevices> MediaDevices => iOS.MediaDevices.CreateAsync();
    }
}
