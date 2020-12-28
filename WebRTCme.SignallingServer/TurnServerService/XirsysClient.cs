using System.Threading.Tasks;

namespace WebRTCme.SignallingServer.TurnServerService
{
    public class XirsysClient : ITurnServerClient
    {
        public XirsysClient()
        {
        }

        public Task<RTCIceServer[]> GetIceServers()
        {
            throw new System.NotImplementedException();
        }
    }
}