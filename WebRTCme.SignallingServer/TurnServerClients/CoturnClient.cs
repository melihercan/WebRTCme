using System.Threading.Tasks;

namespace WebRTCme.SignallingServer.TurnServerClients
{
    internal class CoturnClient : ITurnServerClient
    {
        public CoturnClient()
        {
        }

        public Task<RTCIceServer[]> GetIceServers()
        {
            throw new System.NotImplementedException();
        }
    }
}