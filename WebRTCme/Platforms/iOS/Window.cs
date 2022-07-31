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
        public static IWindow Create() => new Window();

        public INavigator Navigator() => iOS.Navigator.Create();

        public IMediaStream MediaStream() => iOS.MediaStream.Create();

        public IRTCPeerConnection RTCPeerConnection(RTCConfiguration configuration) => 
            iOS.RTCPeerConnection.Create(configuration);

        public IMediaRecorder MediaRecorder(IMediaStream stream, MediaRecorderOptions options = null)
        {
            throw new NotImplementedException();
        }
    }
}
