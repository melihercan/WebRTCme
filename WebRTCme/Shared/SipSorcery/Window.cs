using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebRTCme.Shared.SipSorcery
{
    public class Window : IWindow
    {
        public void Dispose()
        {
        }

        public IMediaRecorder MediaRecorder(IMediaStream stream, MediaRecorderOptions options = null)
        {
            throw new NotImplementedException();
        }

        public IMediaStream MediaStream()
        {
#if WINDOWS
            return new WebRTCme.Windows.MediaStream();
#else
            throw new NotImplementedException();
#endif
        }

        public INavigator Navigator()
        {
#if WINDOWS
            return WebRTCme.Windows.Navigator.Create();
#else
            throw new NotImplementedException();
#endif
        }

        public IRTCPeerConnection RTCPeerConnection(RTCConfiguration configuration) =>
            new RTCPeerConnection(configuration);
    }

}
