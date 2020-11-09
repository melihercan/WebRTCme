using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebRtc.iOS;
using Webrtc;

namespace WebRTCme
{
    internal class WebRtc : IWebRtc
    {
        private static RTCPeerConnectionFactory _nativePeerConnectionFactory;
        public static RTCPeerConnectionFactory NativePeerConnectionFactory =>
            _nativePeerConnectionFactory ?? (_nativePeerConnectionFactory = new RTCPeerConnectionFactory(
                new RTCDefaultVideoEncoderFactory(),
                new RTCDefaultVideoDecoderFactory()));

        public void Initialize()
        {
            ////CFunctions.InitFieldTrialDictionary(new Dictionary<string, string>());
            ////CFunctionsRTCSetupInternalTracer();
            CFunctions.RTCInitializeSSL();
        }

        public void Cleanup()
        {
            ////CFunctions.RTCShutdownInternalTracer();
            CFunctions.RTCCleanupSSL();
        }

        public IWindow Window => new Window();

        public Task<IWindowAsync> WindowAsync(IJSRuntime jsRuntime) => throw new NotImplementedException();
    }
}
