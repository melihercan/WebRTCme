using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebRTCme.Connection.MediaSoup;

namespace WebRTCme.Connection.MediaSoup.Proxy.Client
{
    public class Device
    {
        readonly Ortc _ortc = new();
        readonly Handler _handler = new();


        ExtendedRtpCapabilities _extendedRtpCapabilities;
        
        bool _loaded;
        RtpCapabilities _rtpCapabilities;
        RtpCapabilities _recvRtpCapabilities;
        SctpCapabilities _sctpCapabilities;
        CanProduceByKind _canProduceByKind = new() { Audio = false, Video = false };

        public string HandlerName => "Generic";
        public bool Loaded => _loaded;

        public RtpCapabilities RtpCapabilities => _rtpCapabilities;

        public SctpCapabilities SctpCapabilities => _sctpCapabilities;

        public async Task LoadAsync(RtpCapabilities routerRtpCapabilities)
        {
            if (_loaded)
                throw new Exception("Already loaded");

            _ortc.ValidateRtpCapabilities(routerRtpCapabilities);

            var nativeRtpCapabilities = await _handler.GetNativeRtpCapabilitiesAsync();
            _ortc.ValidateRtpCapabilities(nativeRtpCapabilities);

            _extendedRtpCapabilities = _ortc.GetExtendedRtpCapabilites(nativeRtpCapabilities, routerRtpCapabilities);

            _canProduceByKind.Audio = _ortc.CanSend(MediaKind.Audio, _extendedRtpCapabilities);
            _canProduceByKind.Video = _ortc.CanSend(MediaKind.Video, _extendedRtpCapabilities);

            _recvRtpCapabilities = _ortc.GetRecvRtpCapabilities(_extendedRtpCapabilities);
            _ortc.ValidateRtpCapabilities(_recvRtpCapabilities);

            _sctpCapabilities = await _handler.GetNativeSctpCapabilitiesAsync();
            _ortc.ValidateSctpCapabilities(_sctpCapabilities);

            _loaded = true;
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
