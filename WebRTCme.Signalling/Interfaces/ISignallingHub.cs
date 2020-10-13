using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebRtcSignalling.Interfaces
{
    public interface ISignallingHub
    {
        Task ReceiveMessage(string message);
    }
}
