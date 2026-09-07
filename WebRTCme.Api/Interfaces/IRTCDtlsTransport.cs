using System;

namespace WebRTCme
{
    public interface IRTCDtlsTransport : IDisposable // INativeObject
    {
        IRTCIceTransport IceTransport { get; }

        RTCDtlsTransportState State { get; }

        /// <summary>The remote certificate chain, in DER form, leaf first.</summary>
        byte[][] GetRemoteCertificates();

        event EventHandler OnStateChange;
        event EventHandler<IRTCErrorEvent> OnError;
    }
}