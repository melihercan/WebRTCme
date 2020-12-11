using System;
using System.Collections.Generic;
using System.Text;
using WebRTCme;
//using Webrtc;
using System.Linq;
using AVFoundation;
using System.Threading.Tasks;

namespace WebRtc.iOS
{
    internal class MediaStream : ApiBase, IMediaStream
    {
        //private List<IMediaStreamTrack> _videoStreamTracks = new List<IMediaStreamTrack>();
        //private List<IMediaStreamTrack> _audioStreamTracks = new List<IMediaStreamTrack>();


        public static IMediaStream Create()
        {
            throw new NotImplementedException();
        }

        public static IMediaStream Create(IMediaStream stream)
        {
            throw new NotImplementedException();
        }

        public static IMediaStream Create(IMediaStreamTrack[] tracks)
        {
            var nativeMediaStream = 
                WebRTCme.WebRtc.NativePeerConnectionFactory.MediaStreamWithStreamId("LocalMediaStream");
            var ret = new MediaStream(nativeMediaStream);
            foreach (var track in tracks) 
                ret.AddTrack(track);
            return ret;
        }

 
        public static IMediaStream Create(MediaStreamConstraints constraints)
        {
            var mediaStreamTracks = new List<IMediaStreamTrack>();
            bool isAudio = (constraints.Audio.Value.HasValue && constraints.Audio.Value == true) ||
                constraints.Audio.Object != null;
            bool isVideo = (constraints.Video.Value.HasValue && constraints.Video.Value == true) ||
                constraints.Video.Object != null;
            if (isAudio)
            {
                var defaultAudioDevice = AVCaptureDevice.GetDefaultDevice(AVMediaTypes.Audio);
                mediaStreamTracks.Add(MediaStreamTrack.Create(MediaStreamTrackKind.Audio, defaultAudioDevice.UniqueID));
            }
            if (isVideo)
            {
                var defaultVideoDevice = AVCaptureDevice.GetDefaultDevice(AVMediaTypes.Video);
                mediaStreamTracks.Add(MediaStreamTrack.Create(MediaStreamTrackKind.Video, defaultVideoDevice.UniqueID));//,
                    //VideoType.Local));
            }

            var nativeMediaStream = 
                WebRTCme.WebRtc.NativePeerConnectionFactory.MediaStreamWithStreamId("LocalMediaStream");
            var ret = new MediaStream(nativeMediaStream);
            foreach (var track in mediaStreamTracks)
                ret.AddTrack(track);
            return ret;

            //var ret = new MediaStream();
            //return ret.Initialize(constraints);
        }

        private MediaStream(Webrtc.RTCMediaStream nativeMediaStream) : base(nativeMediaStream) { }


        //private MediaStream()
        //{
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
//}


#if false
        private IMediaStream Initialize(MediaStreamConstraints constraints, 
            VideoType videoType = VideoType.Local, string videoSource = null)
        {
            // Assume default for now.


            NativeObject = WebRTCme.WebRtc.NativePeerConnectionFactory.MediaStreamWithStreamId("LocalMediaStream");

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

            AddTrack(MediaStreamTrack.Create(MediaStreamTrackKind.Audio, audioDevice?.UniqueID ?? "MyAudio"));
            AddTrack(MediaStreamTrack.Create(MediaStreamTrackKind.Video, videoDevice?.UniqueID ?? 
                "MyVideo", videoType, videoSource));


            //SelfNativeObject = 


            return this;

            
        }
#endif

        public bool Active => GetTracks().All(track => track.ReadyState == MediaStreamTrackState.Live);

        public string Id => ((Webrtc.RTCMediaStream)NativeObject).StreamId;

        public event EventHandler<IMediaStreamTrackEvent> OnAddTrack;
        public event EventHandler<IMediaStreamTrackEvent> OnRemoveTrack;

        public IMediaStream Clone()
        {
            throw new NotImplementedException();
        }

        public List<IMediaStreamTrack> GetTracks() => GetVideoTracks().Concat(GetAudioTracks()).ToList();

        public IMediaStreamTrack GetTrackById(string id) => GetTracks().Find(track => track.Id == id);

        public List<IMediaStreamTrack> GetVideoTracks() => //_videoStreamTracks;
            ((Webrtc.RTCMediaStream)NativeObject).VideoTracks
                .Select(nativeTrack => MediaStreamTrack.Create(nativeTrack))
                .ToList();

        public List<IMediaStreamTrack> GetAudioTracks() => //_audioStreamTracks;
            ((Webrtc.RTCMediaStream)NativeObject).AudioTracks
                .Select(nativeTrack => MediaStreamTrack.Create(nativeTrack))
                .ToList();

        public void AddTrack(IMediaStreamTrack track)
        {
            if (GetTrackById(track.Id) is null)
            {
                switch (track.Kind)
                {
                    case MediaStreamTrackKind.Audio:
                        ((Webrtc.RTCMediaStream)NativeObject).AddAudioTrack((Webrtc.RTCAudioTrack)track.NativeObject);
                        //_audioStreamTracks.Add(track);
                        break;
                    case MediaStreamTrackKind.Video:
                        ((Webrtc.RTCMediaStream)NativeObject).AddVideoTrack((Webrtc.RTCVideoTrack)track.NativeObject);
                        //_videoStreamTracks.Add(track);
                        break;
                }
            };
        }

        public void RemoveTrack(IMediaStreamTrack track)
        {
            if (GetTrackById(track.Id) != null)
            {
                switch (track.Kind)
                {
                    case MediaStreamTrackKind.Audio:
                        //_audioStreamTracks.Remove(track);
                        ((Webrtc.RTCMediaStream)NativeObject).RemoveAudioTrack((Webrtc.RTCAudioTrack)track.NativeObject);
                        break;
                    case MediaStreamTrackKind.Video:
                        //_videoStreamTracks.Remove(track);
                        ((Webrtc.RTCMediaStream)NativeObject).RemoveVideoTrack((Webrtc.RTCVideoTrack)track.NativeObject);
                        break;
                }
            };
        }

        public void SetElementReferenceSrcObject(object media)
        {
            throw new NotImplementedException();
        }
    }
}
