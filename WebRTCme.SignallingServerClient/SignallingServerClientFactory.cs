using System;
using System.Threading.Tasks;
using WebRTCme.Middleware;

namespace WebRTCme.SignallingServerClient
{
    public static class SignallingServerClientFactory
    {
        //public static string SignallingServerBaseUrl { get; private set; }

        //public static void Initialize(string signallingServerBaseUrl)
        //{
          //  SignallingServerBaseUrl = signallingServerBaseUrl;
        //}

        public static ISignallingServerClient Create(string signallingServerBaseUrl) =>
                //signallingServer switch
                //{
                /*SignallingServer.WebRTCme =>*/ new WebRTCmeClient(signallingServerBaseUrl);//,
                                                                      //_ => throw new NotSupportedException($"'{signallingServer}' is not supported")
                                                                      //};

        //public static void Cleanup() { }
    }
}
