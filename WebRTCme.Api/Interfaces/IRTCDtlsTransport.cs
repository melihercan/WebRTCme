using System;

namespace WebRTCme
{
    public interface IRTCDtlsTransport : INativeObject
    {
        IRTCIceTransport IceTransport { get; }

        RTCDtlsTransportState State { get; }
    }
}