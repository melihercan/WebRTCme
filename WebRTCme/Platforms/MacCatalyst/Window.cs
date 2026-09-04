using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebRTCme;
using WebRTCme.Platforms.MacCatalyst.Custom;

namespace WebRTCme.MacCatalyst
{
    internal class Window : NativeBase<object>, IWindow
    {
        public INavigator Navigator() => global::WebRTCme.MacCatalyst.Navigator.Create();

        public IMediaStream MediaStream() => new global::WebRTCme.MacCatalyst.MediaStream();

        public IRTCPeerConnection RTCPeerConnection(RTCConfiguration configuration) => 
            new global::WebRTCme.MacCatalyst.RTCPeerConnection(configuration);

        public IMediaRecorder MediaRecorder(IMediaStream stream, MediaRecorderOptions options = null)
        {
            throw new NotImplementedException();
        }
    }
}
