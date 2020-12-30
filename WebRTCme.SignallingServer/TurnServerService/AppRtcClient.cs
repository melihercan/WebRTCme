using System.Threading.Tasks;

namespace WebRTCme.SignallingServer.TurnServerService
{
    public class AppRtcClient : ITurnServerClient
    {
        public AppRtcClient()
        {
        }

        public Task<RTCIceServer[]> GetIceServersAsync()
        {
            throw new System.NotImplementedException();
        }
    }
}