using System.Threading.Tasks;

namespace WebRTCme.SignallingServer.TurnServerService
{
    public class TwilioProxy : ITurnServerProxy
    {
        public TwilioProxy()
        {
        }

        public Task<RTCIceServer[]> GetIceServersAsync()
        {
            throw new System.NotImplementedException();
        }
    }
}