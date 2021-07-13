using System.Threading.Tasks;

namespace WebRTCme.Connection.Signaling.Server.TurnServerProxies
{
    public class StunOnlyProxy : ITurnServerProxy
    {
        public StunOnlyProxy()
        {
        }

        public Task<RTCIceServer[]> GetIceServersAsync()
        {
            // Hard coded STUN servers. 
            return Task.FromResult(new RTCIceServer[] 
            { 
                new RTCIceServer
                {
                    Urls = new string[] 
                    {
                        "stun:stun.stunprotocol.org:3478",
                    },
                },
                new RTCIceServer
                {
                    Urls = new string[]
                    {
                        "stun:stun.l.google.com:19302"
                    },
                }
            });
        }
    }
}
