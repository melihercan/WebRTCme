using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebRTCme.Connection.MediaSoup;

namespace WebRTCme.Connection.MediaSoup.Proxy.Client
{
    public class Device
    {
        bool _loaded;
        RtpCapabilities _rtpCapabilities;
        SctpCapabilities _sctpCapabilities;

        public string HandlerName => "Generic";
        public bool Loaded => _loaded;

        public RtpCapabilities RtpCapabilities => _rtpCapabilities;

        public SctpCapabilities SctpCapabilities => _sctpCapabilities;

        public async Task LoadAsync(RtpCapabilities routerRtpCapabilities)
        {
            if (_loaded)
                throw new Exception("Already loaded");

            Ortc.ValidateRtpCapabilities(routerRtpCapabilities);

            var handler = new Handler();
            var nativeRtpCapabilities = await handler.GetNativeRtpCapabilitiesAsync();
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
