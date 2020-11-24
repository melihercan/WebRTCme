using System;
using System.Collections.Generic;
using System.Text;
using WebRTCme;
using Webrtc;
using System.Linq;
using AVFoundation;
using System.Threading.Tasks;

namespace WebRtc.iOS
{
    internal class MediaStream : ApiBase, IMediaStream
    {
        ////private readonly RTCPeerConnectionFactory _nativePeerConnectionFactory;
        ////private readonly RTCMediaStream _nativeMediaStream;
        private List<IMediaStreamTrack> _videoStreamTracks = new List<IMediaStreamTrack>();
        private List<IMediaStreamTrack> _audioStreamTracks = new List<IMediaStreamTrack>();



        private MediaStream()
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

        public static IMediaStream Create(MediaStreamConstraints constraints)
        {
            var ret = new MediaStream();
            return ret.Initialize(constraints);
        }

        private IMediaStream Initialize(MediaStreamConstraints constraints, 
            VideoType videoType = VideoType.Local, string videoSource = null)
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

            AddTrack(MediaStreamTrack.Create(MediaStreamTrackKind.Audio, audioDevice?.UniqueID ?? "MyAudio"));
            AddTrack(MediaStreamTrack.Create(MediaStreamTrackKind.Video, videoDevice?.UniqueID ?? 
                "MyVideo", videoType, videoSource));


            //SelfNativeObject = 


            return this;

            
        }

        public MediaStream(IMediaStream stream)
        {

        }

        public MediaStream(IMediaStreamTrack[] tracks)
        {

        }

        public bool Active =>
            GetTracks().All(track => track.ReadyState == MediaStreamTrackState.Live);


            //(await GetTracks()).Select(async AppTrackingTransparency => )

        ////public Task<bool> Ended => throw new NotImplementedException();

        public string Id => ((RTCMediaStream)SelfNativeObject).StreamId;



        public IMediaStream Clone()
        {
            throw new NotImplementedException();
        }

        public List<IMediaStreamTrack> GetTracks() =>
            GetVideoTracks().Concat(GetAudioTracks()).ToList();

        public IMediaStreamTrack GetTrackById(string id) =>
            GetTracks().Find(track => track.Id == id);

        public List<IMediaStreamTrack> GetVideoTracks() => _videoStreamTracks;
            /////((RTCMediaStream)SelfNativeObject).VideoTracks
            /////.Select(track => MediaStreamTrack.Create(track))
            /////.ToList();

        public List<IMediaStreamTrack> GetAudioTracks() => _audioStreamTracks;
            ////((RTCMediaStream)SelfNativeObject).AudioTracks
            ////.Select(track => MediaStreamTrack.Create(track))
            ////.ToList();

        public void AddTrack(IMediaStreamTrack track)
        {
            if (GetTrackById(track.Id) is null)
            {
                switch (track.Kind)
                {
                    case MediaStreamTrackKind.Audio:
                        ((RTCMediaStream)SelfNativeObject).AddAudioTrack((RTCAudioTrack)track.SelfNativeObject);
                        _audioStreamTracks.Add(track);
                        break;
                    case MediaStreamTrackKind.Video:
                        ((RTCMediaStream)SelfNativeObject).AddVideoTrack((RTCVideoTrack)track.SelfNativeObject);
                        _videoStreamTracks.Add(track);
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
                        _audioStreamTracks.Remove(track);
                        ((RTCMediaStream)SelfNativeObject).RemoveAudioTrack((RTCAudioTrack)track.SelfNativeObject);
                        break;
                    case MediaStreamTrackKind.Video:
                        _videoStreamTracks.Remove(track);
                        ((RTCMediaStream)SelfNativeObject).RemoveVideoTrack((RTCVideoTrack)track.SelfNativeObject);
                        break;
                }
            };
        }

        public Task SetElementReferenceSrcObjectAsync(object media)
        {
            throw new NotImplementedException();
        }
    }
}
