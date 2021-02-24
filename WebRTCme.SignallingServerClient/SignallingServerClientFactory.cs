using System;
using System.Threading.Tasks;

namespace WebRTCme.SignallingServerClient
{
    public static class SignallingServerClientFactory
    {
        public static Task<ISignallingServerClient> CreateAsync(SignallingServerClientType client, string signallingServerBaseUrl,
            ISignallingServerCallbacks signallingServerCallbacks) =>
                client switch
                {
                    SignallingServerClientType.WebRtcMe => WebRTCmeClient.CreateAsync(signallingServerBaseUrl, signallingServerCallbacks),
                    SignallingServerClientType.WebSocket => WebSocketClient.CreateAsync(signallingServerBaseUrl, signallingServerCallbacks)
                };
    }
}
