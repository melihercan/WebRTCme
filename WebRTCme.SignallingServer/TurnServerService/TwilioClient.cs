using System.Threading.Tasks;

namespace WebRTCme.SignallingServer.TurnServerService
{
    public class TwilioClient : ITurnServerClient
    {
        public TwilioClient()
        {
        }

        public Task<RTCIceServer[]> GetIceServers()
        {
            throw new System.NotImplementedException();
        }
    }
}