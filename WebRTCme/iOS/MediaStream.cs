using System;
using System.Collections.Generic;
using System.Text;
using WebRTCme;
using Webrtc;
using System.Linq;
using AVFoundation;

namespace WebRtc.iOS
{
    internal class MediaStream : ApiBase, IMediaStream
    {

        private readonly RTCPeerConnectionFactory _nativePeerConnectionFactory;
        private readonly RTCMediaStream _nativeMediaStream;

        private List<IMediaStreamTrack> _mediaStreamTracks = new List<IMediaStreamTrack>();

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
            var = new RTCCameraVideoCapturer();
            //var cameraVideoCapturer.WeakDelegate = videoSource;


            CFunctions.RTCInitializeSSL();
#endif


        }

        public MediaStream(MediaStreamConstraints constraints, VideoType videoType = VideoType.Local, 
            string videoSource = null)
        {
            // Assume default for now.


            SelfNativeObject = WebRTCme.WebRtc.NativePeerConnectionFactory.MediaStreamWithStreamId("LocalMediaStream");

            var videoDevice = AVCaptureDevice.GetDefaultDevice(AVMediaTypes.Video);
            var audioDevice = AVCaptureDevice.GetDefaultDevice(AVMediaTypes.Audio);

            ////            var videoDecoderFactory = new RTCDefaultVideoDecoderFactory();
            //NativeObjects.Add(videoDecoderFactory);
            ////        var videoEncoderFactory = new RTCDefaultVideoEncoderFactory();
            //NativeObjects.Add(videoEncoderFactory);

            ////    _nativePeerConnectionFactory = new RTCPeerConnectionFactory(videoEncoderFactory, videoDecoderFactory);
            //NativeObjects.Add(_peerConnectionFactory);
            ////            var videoSource = _nativePeerConnectionFactory.VideoSource;

            ////_nativeMediaStream = _nativePeerConnectionFactory.MediaStreamWithStreamId("LocalMediaStream");
            //NativeObjects.Add(_mediaStream);

            ////var nativeVideoTrack = _nativePeerConnectionFactory.VideoTrackWithSource(videoSource, videoDevice.UniqueID);
            //NativeObjects.Add(nativeVideoTrack);
            ////AddTrack(new MediaStreamTrack(nativeVideoTrack));
            ///

            AddTrack(new MediaStreamTrack(MediaStreamTrackKind.Audio, audioDevice?.UniqueID ?? "MyAudio"));
            AddTrack(new MediaStreamTrack(MediaStreamTrackKind.Video, videoDevice?.UniqueID ?? "MyVideo", 
                videoType, videoSource));


            //SelfNativeObject = 




            
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

        public List<IMediaStreamTrack> GetTracks() => _mediaStreamTracks;

        public IMediaStreamTrack GetTrackById(string id) => _mediaStreamTracks.Find(track => track.Id == id);

        public List<IMediaStreamTrack> GetVideoTracks() =>
            _mediaStreamTracks.Where(track => track.Kind == MediaStreamTrackKind.Video).ToList();

        public List<IMediaStreamTrack> GetAudioTracks() => 
            _mediaStreamTracks.Where(track => track.Kind == MediaStreamTrackKind.Audio).ToList();


        public void AddTrack(IMediaStreamTrack track)
        {
            if (GetTrackById(track.Id) is null)
            {
                switch (track.Kind)
                {
                    case MediaStreamTrackKind.Audio:
                        ((RTCMediaStream)SelfNativeObject).AddAudioTrack((RTCAudioTrack)track.SelfNativeObject);
                        break;
                    case MediaStreamTrackKind.Video:
                        ((RTCMediaStream)SelfNativeObject).AddVideoTrack((RTCVideoTrack)track.SelfNativeObject);
                        break;
                }

                _mediaStreamTracks.Add(track);
            };
        }

        public void RemoveTrack(IMediaStreamTrack track) => _mediaStreamTracks.Remove(track);

        public void SetElementReferenceSrcObject(object media)
        {
            throw new NotImplementedException();
        }
    }
}
