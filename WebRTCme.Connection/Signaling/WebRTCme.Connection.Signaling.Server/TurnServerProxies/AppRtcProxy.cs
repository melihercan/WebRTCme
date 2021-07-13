using System.Threading.Tasks;

namespace WebRTCme.Connection.Signaling.Server.TurnServerProxies
{
    public class AppRtcProxy : ITurnServerProxy
    {
        public AppRtcProxy()
        {
        }

        public Task<RTCIceServer[]> GetIceServersAsync()
        {
            throw new System.NotImplementedException();
        }
    }
}