using System;

namespace WebRTCme
{
    public interface IRTCDtlsTransport : IDisposable // INativeObject
    {
        IRTCIceTransport IceTransport { get; }

        RTCDtlsTransportState State { get; }
    }
}