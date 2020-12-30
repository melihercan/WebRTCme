using System;

namespace WebRTCme.SignallingServerClient
{
    public static class SignallingServerClientFactory
    {
        public static ISignallingServerClient Create(SignallingServer signallingServer) =>
            signallingServer switch
            {
                SignallingServer.WebRTCme => new WebRTCmeClient(),
                _ => throw new NotSupportedException($"'{signallingServer}' is not supported")
            };
    }
}
