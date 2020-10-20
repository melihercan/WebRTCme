using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace WebRTCme
{
    public interface INavigator : IDisposable
    {
        IMediaDevices MediaDevices();
    }

    public interface INavigatorAsync : IAsyncDisposable
    {
        Task<IMediaDevicesAsync> MediaDevicesAsync();
    }
}
