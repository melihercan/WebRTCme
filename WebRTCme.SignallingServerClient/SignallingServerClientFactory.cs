using System;
using System.Threading.Tasks;

namespace WebRTCme.SignallingServerClient
{
    public static class SignallingServerClientFactory
    {
        public static Task<ISignallingServerClient> CreateAsync(SignallingServerClient client, string signallingServerBaseUrl,
            ISignallingServerCallbacks signallingServerCallbacks) =>
                client switch
                {
                    SignallingServerClient.WebRtcMe => WebRTCmeClient.CreateAsync(signallingServerBaseUrl, signallingServerCallbacks),
                    SignallingServerClient.WebSocket => WebSocketClient.CreateAsync(signallingServerBaseUrl, signallingServerCallbacks)
                };
    }
}
