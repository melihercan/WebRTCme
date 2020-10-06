using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme
{
    public interface INavigator
    {
        IMediaDevices MediaDevices { get; }
    }
}
