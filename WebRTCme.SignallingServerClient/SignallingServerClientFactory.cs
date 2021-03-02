using System;
using System.Threading.Tasks;

namespace WebRTCme.SignallingServerClient
{
    //// TODO: NO NEED FOR FACTORY, REMOVE THIS
    public static class SignallingServerClientFactory
    {
        public static Task<ISignallingServerClient> CreateAsync(SignallingServerClientType client, string signallingServerBaseUrl,
            ISignallingServerCallbacks signallingServerCallbacks) =>
                client switch
                {
                    SignallingServerClientType.WebRtcMe => WebRTCmeClient.CreateAsync(signallingServerBaseUrl, signallingServerCallbacks),
                };
    }
}
