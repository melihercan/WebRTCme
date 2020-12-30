using System.Threading.Tasks;

namespace WebRTCme.SignallingServer.TurnServerService
{
    public class CoturnClient : ITurnServerClient
    {
        public CoturnClient()
        {
        }

        public Task<RTCIceServer[]> GetIceServersAsync()
        {
            throw new System.NotImplementedException();
        }
    }
}