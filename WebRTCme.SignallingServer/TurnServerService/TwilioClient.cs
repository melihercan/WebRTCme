using System.Threading.Tasks;

namespace WebRTCme.SignallingServer.TurnServerService
{
    public class TwilioClient : ITurnServerClient
    {
        public TwilioClient()
        {
        }

        public Task<RTCIceServer[]> GetIceServersAsync()
        {
            throw new System.NotImplementedException();
        }
    }
}