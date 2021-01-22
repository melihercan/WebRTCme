using System;
using System.Threading.Tasks;

namespace WebRTCme.SignallingServerClient
{
    public static class SignallingServerClientFactory
    {
        public static Task<ISignallingServerClient> CreateAsync(string signallingServerBaseUrl,
            ISignallingServerCallbacks signallingServerCallbacks) =>
                WebRTCmeClient.CreateAsync(signallingServerBaseUrl, signallingServerCallbacks);
    }
}
