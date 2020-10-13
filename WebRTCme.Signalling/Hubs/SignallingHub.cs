using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebRtcSignalling.Interfaces;

namespace WebRtcSignalling.Hubs
{
    public class SignallingHub : Hub<ISignallingHub>
    {
        public async Task SendMessage(string message)
        {
            // For now just send to all other parties.
            await Clients.Others.ReceiveMessage(message);
        }
    }
}
