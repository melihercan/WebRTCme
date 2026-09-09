using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace WebRTCme
{
    public interface IWindow : IDisposable // INativeObject
    {
        INavigator Navigator();


        IMediaStream MediaStream();
        //IMediaStream MediaStream(IMediaStream stream);
        //IMediaStream MediaStream(IMediaStreamTrack[] tracks);

        
        IRTCPeerConnection RTCPeerConnection(RTCConfiguration configuration = null);

        IMediaRecorder MediaRecorder(IMediaStream stream, MediaRecorderOptions options = null);
    }
}
