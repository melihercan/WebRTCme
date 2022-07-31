using Android.Views;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebRTCme;
using WebRTCme.Platforms.Android.Custom;

namespace WebRTCme.Android
{
    internal class Window : NativeBase<object>, IWindow
    {
        public static IWindow Create() => new Window();

        public INavigator Navigator() => Android.Navigator.Create();

        public IMediaStream MediaStream() => Android.MediaStream.Create();

        public IRTCPeerConnection RTCPeerConnection(RTCConfiguration configuration) =>
            new Android.RTCPeerConnection(configuration);

        public IMediaRecorder MediaRecorder(IMediaStream stream, MediaRecorderOptions options = null)
        {
            throw new NotImplementedException();
        }
    }
}
