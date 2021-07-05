using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebRTCme.MediaSoupClient.Models;
using WebRTCme;

namespace WebRTCme.MediaSoupClient.Api
{
    public class Handler
    {
        IRTCPeerConnection _pc;

        public Handler()
        {
            Name = "Generic";
        }

        public string Name { get; }

        public void Close()
        {
            _pc?.Close();
        }

        public Task<RtpCapabilities> GetNativeRtpCapabilities()
        {
            //IRTCPeerConnection pc =  
            throw new NotImplementedException();
        }


    }
}
