using System.Threading.Tasks;

namespace WebRTCme.SignallingServer.TurnServerService
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