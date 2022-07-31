using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebRTCme;
using WebRTCme.Platforms.iOS.Custom;

namespace WebRTCme.iOS
{
    internal class Navigator : NativeBase<object>, INavigator
    {
        public static INavigator Create() => new Navigator();

        public Navigator() { }

        public IMediaDevices MediaDevices => new iOS.MediaDevices();
    }
}
