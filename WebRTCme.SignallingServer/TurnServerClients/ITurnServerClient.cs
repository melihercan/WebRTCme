using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebRTCme;

namespace WebRTCme.SignallingServer.TurnServerClients
{
    internal interface ITurnServerClient
    {
        Task<RTCIceServer[]> GetIceServers(); 
    }
}
