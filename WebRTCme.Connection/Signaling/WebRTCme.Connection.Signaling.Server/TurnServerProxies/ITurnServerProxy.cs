using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebRTCme;

namespace WebRTCme.Connection.Signaling.Server.TurnServerProxies
{
    public interface ITurnServerProxy
    {
        Task<RTCIceServer[]> GetIceServersAsync(); 
    }
}
