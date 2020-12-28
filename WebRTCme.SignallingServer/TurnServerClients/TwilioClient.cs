using System.Threading.Tasks;

namespace WebRTCme.SignallingServer.TurnServerClients
{
    internal class TwilioClient : ITurnServerClient
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