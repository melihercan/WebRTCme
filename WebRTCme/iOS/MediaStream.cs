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

        public Task<bool> Active => throw new NotImplementedException();

        public Task<bool> Ended => throw new NotImplementedException();

        public Task<string> Id => throw new NotImplementedException();



        public Task<IMediaStream> Clone()
        {
            throw new NotImplementedException();
        }

        public Task<List<IMediaStreamTrack>> GetTracks() => Task.FromResult(_mediaStreamTracks);

        public Task<IMediaStreamTrack> GetTrackById(string id) =>
            throw new NotImplementedException();
        ////            _mediaStreamTracks.Find(async track => (await track.Id) == id);

        public Task<List<IMediaStreamTrack>> GetVideoTracks() =>
                        throw new NotImplementedException();
        ////_mediaStreamTracks.Where(async track => (await track.Kind) == MediaStreamTrackKind.Video).ToList();

        public Task<List<IMediaStreamTrack>> GetAudioTracks()
        {
            throw new NotImplementedException();
            ////        var x = _mediaStreamTracks.Select(async track => await track.Kind);
            //.Where(kind => kind == MediaStreamTrackKind.Audio)
            ///   _mediaStreamTracks.Where(track => track.Kind == MediaStreamTrackKind.Audio).ToList();
        }

        public Task AddTrack(IMediaStreamTrack track)
        {
            throw new NotImplementedException();
            //if (GetTrackById(track.Id) is null)
            //{
            //    switch (track.Kind)
            //    {
            //        case MediaStreamTrackKind.Audio:
            //            ((RTCMediaStream)SelfNativeObject).AddAudioTrack((RTCAudioTrack)track.SelfNativeObject);
            //            break;
            //        case MediaStreamTrackKind.Video:
            //            ((RTCMediaStream)SelfNativeObject).AddVideoTrack((RTCVideoTrack)track.SelfNativeObject);
            //            break;
            //    }

            //    _mediaStreamTracks.Add(track);
            //};
            //return Task.CompletedTask;
        }

        public Task RemoveTrack(IMediaStreamTrack track) => Task.FromResult(_mediaStreamTracks.Remove(track));


        public Task SetElementReferenceSrcObjectAsync(object media)
        {
            throw new NotImplementedException();
        }
    }
}
