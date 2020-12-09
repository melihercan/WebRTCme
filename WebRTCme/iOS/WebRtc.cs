using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
//using Webrtc;
using WebRtc.iOS;

namespace WebRTCme
{
    internal class WebRtc : IWebRtc
    {
        private static Webrtc.RTCPeerConnectionFactory _nativePeerConnectionFactory;
        public static Webrtc.RTCPeerConnectionFactory NativePeerConnectionFactory =>
            _nativePeerConnectionFactory ?? (_nativePeerConnectionFactory = new Webrtc.RTCPeerConnectionFactory(
                new Webrtc.RTCDefaultVideoEncoderFactory(),
                new Webrtc.RTCDefaultVideoDecoderFactory()));

        public static IWebRtc Create() => new WebRtc();

        private WebRtc()
        {
            ////CFunctions.InitFieldTrialDictionary(new Dictionary<string, string>());
            ////CFunctionsRTCSetupInternalTracer();
            Webrtc.CFunctions.RTCInitializeSSL();
        }

        public void Cleanup()
        {
            ////CFunctions.RTCShutdownInternalTracer();
            Webrtc.CFunctions.RTCCleanupSSL();
        }

        public IWindow Window(IJSRuntime jsRuntime) => global::WebRtc.iOS.Window.Create();
    }
}
