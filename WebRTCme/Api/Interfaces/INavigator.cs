using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace WebRTCme
{
    public interface INavigator : IDisposable // INativeObject
    {
        IMediaDevices MediaDevices { get; }
    }
}
