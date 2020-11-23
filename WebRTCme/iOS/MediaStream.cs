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
        ////private List<IMediaStreamTrack> _mediaStreamTracks = new List<IMediaStreamTrack>();



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

        public static Task<IMediaStream> CreateAsync(MediaStreamConstraints constraints)
        {
            var ret = new MediaStream();
            return ret.InitializeAsync(constraints);
        }

        private async Task<IMediaStream> InitializeAsync(MediaStreamConstraints constraints, 
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

            await AddTrack(await MediaStreamTrack.CreateAsync(MediaStreamTrackKind.Audio, audioDevice?.UniqueID ?? 
                "MyAudio"));
            await AddTrack(await MediaStreamTrack.CreateAsync(MediaStreamTrackKind.Video, videoDevice?.UniqueID ?? 
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

        public Task<bool> Active
        {
            get
            {
                return Task.FromResult(false);
            }
        }
//        public Task<bool> Active => //Task.FromResult(
  //          await GetTracks()).Select(async track => await track.ReadyState).ToArray()))
    //        .All(state => state == MediaStreamTrackState.Live);


            //(await GetTracks()).Select(async AppTrackingTransparency => )

        ////public Task<bool> Ended => throw new NotImplementedException();

        public Task<string> Id => Task.FromResult(((RTCMediaStream)SelfNativeObject).StreamId);



        public Task<IMediaStream> Clone()
        {
            throw new NotImplementedException();
        }

        public async Task<List<IMediaStreamTrack>> GetTracks() =>
            (await GetVideoTracks()).Concat(await GetAudioTracks()).ToList();

        public async Task<IMediaStreamTrack> GetTrackById(string id) =>
            (await Task.WhenAll((await GetTracks()).Select(async track => 
                new { Id = await track.Id, Track = track }).ToArray()))
            .FirstOrDefault(a => a.Id == id)?.Track;

        public async Task<List<IMediaStreamTrack>> GetVideoTracks() =>
                (await Task.WhenAll(((RTCMediaStream)SelfNativeObject).VideoTracks.Select(async nativeVideoTrack =>
                    await MediaStreamTrack.CreateAsync(nativeVideoTrack)).ToArray()))
                .ToList();

        public async Task<List<IMediaStreamTrack>> GetAudioTracks() =>
                (await Task.WhenAll(((RTCMediaStream)SelfNativeObject).AudioTracks.Select(async nativeAudioTrack =>
                    await MediaStreamTrack.CreateAsync(nativeAudioTrack)).ToArray()))
                .ToList();

        public async Task AddTrack(IMediaStreamTrack track)
        {
            if (await GetTrackById(await track.Id) is null)
            {
                switch (await track.Kind)
                {
                    case MediaStreamTrackKind.Audio:
                        ((RTCMediaStream)SelfNativeObject).AddAudioTrack((RTCAudioTrack)track.SelfNativeObject);
                        break;
                    case MediaStreamTrackKind.Video:
                        ((RTCMediaStream)SelfNativeObject).AddVideoTrack((RTCVideoTrack)track.SelfNativeObject);
                        break;
                }
            };
        }

        public async Task RemoveTrack(IMediaStreamTrack track)
        {
            if (await GetTrackById(await track.Id) != null)
            {
                switch (await track.Kind)
                {
                    case MediaStreamTrackKind.Audio:
                        ((RTCMediaStream)SelfNativeObject).RemoveAudioTrack((RTCAudioTrack)track.SelfNativeObject);
                        break;
                    case MediaStreamTrackKind.Video:
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
