using System.Threading.Tasks;

namespace WebRTCme.SignallingServer.TurnServerClients
{
    internal class XirsysClient : ITurnServerClient
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