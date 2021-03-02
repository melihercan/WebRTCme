using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebRtc.iOS;

namespace WebRTCme
{
    internal class WebRtc : IWebRtc
    {
        public static Webrtc.RTCPeerConnectionFactory NativePeerConnectionFactory { get; private set; }

        private static int _id = 1000;
        public static int Id => Interlocked.Increment(ref _id);

        public static IWebRtc Create() => new WebRtc();

        private WebRtc()
        {
            NativePeerConnectionFactory = new Webrtc.RTCPeerConnectionFactory(
                new Webrtc.RTCDefaultVideoEncoderFactory(),
                new Webrtc.RTCDefaultVideoDecoderFactory());


            ////CFunctions.InitFieldTrialDictionary(new Dictionary<string, string>());
            ////CFunctionsRTCSetupInternalTracer();
            //Webrtc.CFunctions.RTCInitializeSSL();
            Webrtc.CFunctions.RTCSetMinDebugLogLevel(Webrtc.RTCLoggingSeverity./*Verbose*/Error/*Warning*/);
        }

        public void Cleanup()
        {
            ////CFunctions.RTCShutdownInternalTracer();
            //Webrtc.CFunctions.RTCCleanupSSL();
        }

        public IWindow Window(IJSRuntime jsRuntime) => global::WebRtc.iOS.Window.Create();
    }
}
