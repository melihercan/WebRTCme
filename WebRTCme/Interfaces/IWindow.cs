using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme
{
    public interface IWindow
    {
        INavigator Navigator { get; }

        IRTCPeerConnection NewRTCPeerConnection();
    }
}
