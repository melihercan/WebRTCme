using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace WebRTCme
{
    public interface INavigator : INativeObject
    {
        IMediaDevices MediaDevices();
    }

    public interface INavigatorAsync : INativeObjectAsync
    {
        Task<IMediaDevicesAsync> MediaDevicesAsync();
    }
}
