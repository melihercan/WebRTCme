using System;
using System.Threading.Tasks;

namespace WebRTCme.SignallingServerClient
{
    public static class SignallingServerClientFactory
    {
        public static ISignallingServerClient Create(string signallingServerBaseUrl) =>
            new WebRTCmeClient(signallingServerBaseUrl);
    }
}
