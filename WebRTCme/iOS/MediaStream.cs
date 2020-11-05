using System;
using System.Collections.Generic;
using System.Text;
using WebRTCme;
using Webrtc;

namespace WebRtc.iOS
{
    internal class MediaStream : ApiBase, IMediaStream
    {
        public MediaStream()
        {



#if false

            ////            var video = AVCaptureDevice.GetDefaultDevice(AVMediaTypes.Video);
            ////            var audio = AVCaptureDevice.GetDefaultDevice(AVMediaTypes.Audio);


            var videoDecoderFactory = new RTCDefaultVideoDecoderFactory();
            var videoEncoderFactory = new RTCDefaultVideoEncoderFactory();

            var peerConnectionFactory = new RTCPeerConnectionFactory(videoEncoderFactory, videoDecoderFactory);

            //var videoSource = new RTCVideoSource();



            // if video is camera
            var cameraVideoCapturer = new RTCCameraVideoCapturer();
            //var cameraVideoCapturer.WeakDelegate = videoSource;


            CFunctions.RTCInitializeSSL();
#endif


        }

        public MediaStream(MediaStreamConstraints constraints)
        {

        }

        public MediaStream(IMediaStream stream)
        {

        }

        public MediaStream(IMediaStreamTrack[] tracks)
        {

        }

        public bool Active => throw new NotImplementedException();

        public bool Ended => throw new NotImplementedException();

        public string Id => throw new NotImplementedException();

        public event EventHandler<IMediaStreamTrackEvent> OnAddTrack;
        public event EventHandler<IMediaStreamTrackEvent> OnRemoveTrack;
        public event EventHandler OnActive;
        public event EventHandler OnInactive;


        public IMediaStream Clone()
        {
            throw new NotImplementedException();
        }

        public List<IMediaStreamTrack> GetTracks()
        {
            throw new NotImplementedException();
        }

        public IMediaStreamTrack GetTrackById(string id)
        {
            throw new NotImplementedException();
        }

        public List<IMediaStreamTrack> GetVideoTracks()
        {
            throw new NotImplementedException();
        }

        public List<IMediaStreamTrack> GetAudioTracks()
        {
            throw new NotImplementedException();
        }




        public void AddTrack(IMediaStreamTrack track)
        {
            throw new NotImplementedException();
        }

        public void RemoveTrack(IMediaStreamTrack track)
        {
            throw new NotImplementedException();
        }

        public void SetElementReferenceSrcObject(object media)
        {
            throw new NotImplementedException();
        }
    }
}
