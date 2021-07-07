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

        public async Task<RtpCapabilities> GetNativeRtpCapabilities()
        {
            IRTCPeerConnection pc = _window.RTCPeerConnection(new RTCConfiguration 
            { 
                IceTransportPolicy = RTCIceTransportPolicy.All,
                BundlePolicy = RTCBundlePolicy.MaxBundle,
                RtcpMuxPolicy = RTCRtcpMuxPolicy.Require,
            });
            pc.AddTransceiver(MediaStreamTrackKind.Audio);
            pc.AddTransceiver(MediaStreamTrackKind.Video);
            var offer = await pc.CreateOffer();
            pc.Close();


            return null;
        }


    }
}
