using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebRTCme;
using WebRTCme.Platforms.Android.Custom;

namespace WebRTCme.Android
{

    internal class Navigator : NativeBase<object>, INavigator
    {
        public static INavigator Create() => new Navigator();

        public Navigator() { }

        public IMediaDevices MediaDevices => new MediaDevices();
    }
}
