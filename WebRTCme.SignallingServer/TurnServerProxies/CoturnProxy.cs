using System.Threading.Tasks;

namespace WebRTCme.SignallingServer.TurnServerService
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