using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebRTCme.ConnectionServer;

namespace WebRTCme.MediaSoupClient
{
    public class Device
    {
        public RtpCapabilities GetRtpCapabilities()
        {
            throw new NotImplementedException();
        }

        public SctpCapabilities GetSctpCapabilities()
        {
            throw new NotImplementedException();
        }

        public bool IsLoaded()
        {
            throw new NotImplementedException();
        }

        public async Task LoadAsync(RtpCapabilities rtpCapabilities)
        {
            throw new NotImplementedException();
        }

        public bool CanProduce(MediaKind kind)
        {
            throw new NotImplementedException();
        }

        public Transport CreateSendTransport(TransportOptions options)
        {
            throw new NotImplementedException();
        }

        public Transport CreateRecvTransport(TransportOptions options)
        {
            throw new NotImplementedException();
        }

        public event EventHandler OnClose;
        public event EventHandler<Transport> OnNewTransport; 
    }
}
