using System.Threading.Tasks;

namespace WebRTCme.Connection.Signaling.Server.TurnServerProxies
{
    public class CoturnProxy : ITurnServerProxy
    {
        public CoturnProxy()
        {
        }

        public Task<RTCIceServer[]> GetIceServersAsync()
        {
            throw new System.NotImplementedException();
        }
    }
}