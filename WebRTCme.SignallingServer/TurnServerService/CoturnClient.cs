using System.Threading.Tasks;

namespace WebRTCme.SignallingServer.TurnServerService
{
    public class CoturnClient : ITurnServerClient
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