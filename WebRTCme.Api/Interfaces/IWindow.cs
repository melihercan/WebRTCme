using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace WebRTCme
{
    public interface IWindow : IDisposable
    {
        INavigator Navigator { get; }

        IRTCPeerConnection RTCPeerConnection(RTCConfiguration configuration);
    }

    public interface IWindowAsync : IAsyncDisposable
    {
        Task<INavigatorAsync> NavigatorAsync();


        Task<IRTCPeerConnectionAsync> RTCPeerConnectionAsync(RTCConfiguration configuration);
    }

}
