using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebRTCme;
using WebRTCme.Platforms.MacCatalyst.Custom;

namespace WebRTCme.MacCatalyst
{
    internal class Navigator : NativeBase<object>, INavigator
    {
        public static INavigator Create() => new Navigator();

        public Navigator() { }

        public IMediaDevices MediaDevices => new MediaDevices();
    }
}
