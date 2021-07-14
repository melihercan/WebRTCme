using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebRTCme;
using UtilmeSdpTransform;
using WebRTCme.Connection.MediaSoup;
using Utilme.SdpTransform;

namespace WebRTCme.Connection.MediaSoup.Proxy.Client
{
    public class Handler
    {
        IWindow _window;
        IRTCPeerConnection _pc;

        public Handler()
        {
            Name = "Generic";
            _window = Registry.WebRtc.Window(Registry.JsRuntime);
        }

        public string Name { get; }

        public void Close()
        {
            _pc?.Close();
        }

        public async Task<RtpCapabilities> GetNativeRtpCapabilitiesAsync()
        {
            IRTCPeerConnection pc = _window.RTCPeerConnection(new RTCConfiguration 
            { 
                IceTransportPolicy = RTCIceTransportPolicy.All,
                BundlePolicy = RTCBundlePolicy.Balanced,// .MaxBundle,
                RtcpMuxPolicy = RTCRtcpMuxPolicy.Require,
            });
            pc.AddTransceiver(MediaStreamTrackKind.Audio);
            pc.AddTransceiver(MediaStreamTrackKind.Video);
            var offer = await pc.CreateOffer();
            pc.Close();

            var sdpObject = SdpSerializer.ReadSdp(Encoding.UTF8.GetBytes(offer.Sdp));
            var nativeRtpCapabilities = SdpCommonUtils.ExtractRtpCapabilities(sdpObject);

            return nativeRtpCapabilities;
        }
    }
}
