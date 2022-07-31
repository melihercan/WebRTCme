using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebRTCme;
using WebRTCme.Platforms.iOS.Custom;

namespace WebRTCme.iOS
{
    internal class Window : NativeBase<object>, IWindow
    {
        public INavigator Navigator() => iOS.Navigator.Create();

        public IMediaStream MediaStream() => new iOS.MediaStream();

        public IRTCPeerConnection RTCPeerConnection(RTCConfiguration configuration) => 
            new iOS.RTCPeerConnection(configuration);

        public IMediaRecorder MediaRecorder(IMediaStream stream, MediaRecorderOptions options = null)
        {
            throw new NotImplementedException();
        }
    }
}
