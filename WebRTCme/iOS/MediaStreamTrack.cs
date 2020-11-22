using System;
using System.Collections.Generic;
using System.Text;
using WebRTCme;
using Webrtc;
using UIKit;
using AVFoundation;
using System.Linq;
using System.Threading.Tasks;

namespace WebRtc.iOS
{
    internal class MediaStreamTrack : ApiBase, IMediaStreamTrack
    {
        const string Audio = "audio";
        const string Video = "video";

        private RTCCameraPreviewView _nativeCameraPreviewView;
        private RTCCameraVideoCapturer _nativeCameraVideoCapturer;
        private RTCFileVideoCapturer _nativeFileVIdeoCapturer;
        private RTCVideoSource _nativeVideoSource;

        private RTCAudioSource _nativeAudioSource;

        public static Task<IMediaStreamTrack> CreateAsync(RTCMediaStreamTrack nativeMediaStreamTrack)
        {
            var ret = new MediaStreamTrack();
            ret.SelfNativeObject = nativeMediaStreamTrack;
            ret.IsNativeObjectsOwner = false;
            return Task.FromResult(ret as IMediaStreamTrack);
        }

        public static Task<IMediaStreamTrack> CreateAsync(MediaStreamTrackKind mediaStreamTrackKind, string id,
            VideoType videoType = VideoType.Local, string videoSource = null)
        {
            var ret = new MediaStreamTrack();
            return ret.InitializeAsync(mediaStreamTrackKind, id, videoType, videoSource);
        }

        private MediaStreamTrack() { }


        private Task<IMediaStreamTrack> InitializeAsync(MediaStreamTrackKind mediaStreamTrackKind, string id, 
            VideoType videoType, string videoSource)
        {
            //// TODO: Remote flag???? We need to pass constructor this flag.
            /// CURRENTLY LOCAL IS ASSUMED.
            
            /// TODO: If local, RTCCameraVideoCapturer or RTCFileVideoCapturer???
            /// CURENTLY Camera is assumed.


            switch (mediaStreamTrackKind)
            {
                case MediaStreamTrackKind.Audio:
                    AddAudioStreamTrack();
                    break;
                
                case MediaStreamTrackKind.Video:
                    AddLocalVideoStreamTrack();
                    break;


            }

            return Task.FromResult(this as IMediaStreamTrack);

            void AddAudioStreamTrack()
            {
                _nativeAudioSource = WebRTCme.WebRtc.NativePeerConnectionFactory.AudioSourceWithConstraints(null);
                NativeObjects.Add(_nativeAudioSource);
                SelfNativeObject = WebRTCme.WebRtc.NativePeerConnectionFactory
                    .AudioTrackWithSource(_nativeAudioSource, id);
            }

            void AddLocalVideoStreamTrack()
            {
                _nativeVideoSource = WebRTCme.WebRtc.NativePeerConnectionFactory.VideoSource;
                NativeObjects.Add(_nativeVideoSource);

                _nativeCameraPreviewView = new RTCCameraPreviewView();
                NativeObjects.Add(_nativeCameraPreviewView);

                _nativeCameraVideoCapturer = new RTCCameraVideoCapturer(_nativeVideoSource);
                NativeObjects.Add(_nativeCameraVideoCapturer);

                _nativeCameraPreviewView.CaptureSession = _nativeCameraVideoCapturer.CaptureSession;

                SelfNativeObject = WebRTCme.WebRtc.NativePeerConnectionFactory
                    .VideoTrackWithSource(_nativeVideoSource, id);

                var videoDevices = AVCaptureDevice.DevicesWithMediaType(AVMediaType.Video);
                var cameraPosition = AVCaptureDevicePosition.Front;// AVCaptureDevicePosition.Back;
                var device = videoDevices.FirstOrDefault(d => d.Position == cameraPosition);
                var format = RTCCameraVideoCapturer.SupportedFormatsForDevice(device)[0];
                var fps = GetFpsByFormat(format);
                _nativeCameraVideoCapturer.StartCaptureWithDevice(device, format, fps);

                ///// !!!!AddRenderer will not work with RTCCameraPreviewView as it is no derived from RTCVideoRenderer
                /// !!!! SO USE IT WITH REMOTE 
                //var renderer = (UIView)_nativeCameraPreviewView;    ////????
                //((RTCVideoTrack)SelfNativeObject).AddRenderer((IRTCVideoRenderer)renderer);

            }

            void AddMp4VideoStreamTrack()
            {

            }

            void AddRemoteVideoStreamTrack()
            {

            }

            int GetFpsByFormat(AVCaptureDeviceFormat fmt)
            {
                const float _frameRateLimit = 30.0f;

                var maxSupportedFps = 0d;
                foreach (var fpsRange in fmt.VideoSupportedFrameRateRanges)
                    maxSupportedFps = Math.Max(maxSupportedFps, fpsRange.MaxFrameRate);

                return (int)Math.Min(maxSupportedFps, _frameRateLimit);
            }

        }





        public Task<string> Id => Task.FromResult(((RTCMediaStreamTrack)SelfNativeObject).TrackId);


        public Task<MediaStreamTrackKind> Kind => Task.FromResult(((RTCMediaStreamTrack)SelfNativeObject).Kind switch
        {
            Audio => MediaStreamTrackKind.Audio,
            Video => MediaStreamTrackKind.Video,
            _ => throw new Exception($"Invalid RTCMediaStreamTrack.Kind: {((RTCMediaStreamTrack)SelfNativeObject).Kind}")
        });

        public Task<string> ContentHint { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public Task<bool> Enabled { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public Task<bool> Isolated => throw new NotImplementedException();

        public Task<string> Label => throw new NotImplementedException();

        public Task<bool> Muted => throw new NotImplementedException();

        public Task<bool> Readonly => throw new NotImplementedException();

        public Task<MediaStreamTrackState> ReadyState => throw new NotImplementedException();

        public Task<bool> Remote => throw new NotImplementedException();

        public event EventHandler OnMute;
        public event EventHandler OnUnmute;
        public event EventHandler OnEnded;

        public Task ApplyConstraints(MediaTrackConstraints contraints)
        {
            throw new NotImplementedException();
        }

        public Task<IMediaStreamTrack> Clone()
        {
            throw new NotImplementedException();
        }

        public Task<MediaTrackCapabilities> GetCapabilities()
        {
            throw new NotImplementedException();
        }

        public Task<MediaTrackConstraints> GetContraints()
        {
            throw new NotImplementedException();
        }

        public Task<MediaTrackSettings> GetSettings()
        {
            throw new NotImplementedException();
        }



        //public void Play<TRenderer>(TRenderer renderer)
        //{
        //    switch (Kind)
        //    {
        //        case MediaStreamTrackKind.Audio:
        //            break;

        //        case MediaStreamTrackKind.Video:
        //            if (typeof(TRenderer) != typeof(UIView))
        //            {
        //                throw new Exception("UIView is expected as renderer for video track");
        //            }

        //            var view = renderer as UIView;
        //            view = _nativeCameraPreviewView;

        //            var view = renderer as object;//UIView;
        //            var nativeTrack = SelfNativeObject as RTCVideoTrack;
        //            nativeTrack.AddRenderer((RTCVideoRenderer)view);
        //            break;

        //    }

        //}

        public Task Stop()
        {
            throw new NotImplementedException();
        }

        public Task<UIView> GetView<UIView>()
        {
            return Task.FromResult((UIView)(object)_nativeCameraPreviewView);
        }
    }
}
