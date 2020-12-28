using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebRTCme;

namespace WebRTCme.SignallingServer.TurnServerService
{
    public interface ITurnServerClient
    {
        Task<RTCIceServer[]> GetIceServers(); 
    }
}
