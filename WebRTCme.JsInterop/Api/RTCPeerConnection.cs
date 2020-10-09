using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebRTCme;

namespace WebRtcJsInterop
{
    internal class RTCPeerConnection : IRTCPeerConnection
    {
        public void Do()
        {
            Console.WriteLine("I am WEB");
        }

        internal static Task<IRTCPeerConnection> New(IJSRuntime jsRuntime)
        {
            throw new NotImplementedException();
        }
    }
}
