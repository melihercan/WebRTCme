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
            throw new NotImplementedException();
        }

        public IMediaRecorder MediaRecorder(IMediaStream stream, MediaRecorderOptions options = null)
        {
            throw new NotImplementedException();
        }

        public IMediaStream MediaStream()
        {
            throw new NotImplementedException();
        }

        public INavigator Navigator()
        {
            throw new NotImplementedException();
        }

        public IRTCPeerConnection RTCPeerConnection(RTCConfiguration configuration) =>
            new RTCPeerConnection(configuration);
    }

}
