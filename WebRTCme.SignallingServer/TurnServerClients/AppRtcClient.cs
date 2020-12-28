using System.Threading.Tasks;

namespace WebRTCme.SignallingServer.TurnServerClients
{
    internal class AppRtcClient : ITurnServerClient
    {
        public AppRtcClient()
        {
        }

        public Task<RTCIceServer[]> GetIceServers()
        {
            throw new System.NotImplementedException();
        }
    }
}