using System.Threading.Tasks;

namespace WebRTCme.Connection.Signaling.Server.TurnServerProxies
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