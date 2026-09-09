using System;
using System.Collections.Generic;
using System.Text;

namespace WebRTCme
{
    public interface IRTCPeerConnectionIceErrorEvent : IDisposable // INativeObject
    {
        string Address { get; }

        ushort? Port { get; }

        string Url { get; }

        ushort ErrorCode { get; }

        string ErrorText { get; }
    }
}
