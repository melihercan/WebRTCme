using System;
using System.Collections.Generic;
using System.Text;
using WebRTCme;
using Webrtc;
using UIKit;
using AVFoundation;
using System.Linq;

namespace WebRtc.iOS
{
    internal class MediaStreamTrack : ApiBase, IMediaStreamTrack
    {
        const string Audio = "audio";
        const string Video = "video";

        private readonly RTCCameraPreviewView _nativeCameraPreviewView;
        private readonly RTCCameraVideoCapturer _nativeCameraVideoCapturer;
        private readonly RTCFileVideoCapturer _nativeFileVIdeoCapturer;
        private readonly RTCVideoSource _nativeVideoSource;

        private readonly RTCAudioSource _nativeAudioSource;

        public MediaStreamTrack(MediaStreamTrackKind mediaStreamTrackKind, string id)
        {
            //// TODO: Remote flag???? We need to pass constructor this flag.
            /// CURRENTLY LOCAL IS ASSUMED.
            
            /// TODO: If local, RTCCameraVideoCapturer or RTCFileVideoCapturer???
            /// CURENTLY Camera is assumed.


            switch (mediaStreamTrackKind)
            {
                case MediaStreamTrackKind.Audio:
                    _nativeAudioSource = WebRTCme.WebRtc.NativePeerConnectionFactory.AudioSourceWithConstraints(null);
                    NativeObjects.Add(_nativeAudioSource);
                    SelfNativeObject = WebRTCme.WebRtc.NativePeerConnectionFactory
                        .AudioTrackWithSource(_nativeAudioSource, id);
                    break;
                
                case MediaStreamTrackKind.Video:
                    _nativeVideoSource = WebRTCme.WebRtc.NativePeerConnectionFactory.VideoSource;
                    NativeObjects.Add(_nativeVideoSource);
                    
                    _nativeCameraPreviewView = new RTCCameraPreviewView();
                    NativeObjects.Add(_nativeCameraPreviewView);

                    _nativeCameraVideoCapturer = new RTCCameraVideoCapturer(/* nativeVideoSource */);
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


////                    var renderer = (object)_nativeCameraPreviewView;    ////????
    /////                ((RTCVideoTrack)SelfNativeObject).AddRenderer((RTCVideoRenderer)renderer);
                    break;

                    int GetFpsByFormat(AVCaptureDeviceFormat fmt)
                    {
                        const float _frameRateLimit = 30.0f;

                        var maxSupportedFps = 0d;
                        foreach (var fpsRange in fmt.VideoSupportedFrameRateRanges)
                            maxSupportedFps = Math.Max(maxSupportedFps, fpsRange.MaxFrameRate);

                        return (int)Math.Min(maxSupportedFps, _frameRateLimit);
                    }

            }
        }

        public string ContentHint { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public bool Enabled { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public string Id => ((RTCMediaStreamTrack)SelfNativeObject).TrackId;

        public bool Isolated => throw new NotImplementedException();

        public MediaStreamTrackKind Kind => ((RTCMediaStreamTrack)SelfNativeObject).Kind switch
        {
            Audio => MediaStreamTrackKind.Audio,
            Video => MediaStreamTrackKind.Video,
            _ => throw new Exception($"Invalid RTCMediaStreamTrack.Kind: {((RTCMediaStreamTrack)SelfNativeObject).Kind}")
        };

        public string Label => throw new NotImplementedException();

        public bool Muted => throw new NotImplementedException();

        public bool Readonly => throw new NotImplementedException();

        public MediaStreamTrackState ReadyState => throw new NotImplementedException();

        public bool Remote => throw new NotImplementedException();

        public event EventHandler OnMute;
        public event EventHandler OnUnmute;
        public event EventHandler OnEnded;

        public void ApplyConstraints(MediaTrackConstraints contraints)
        {
            throw new NotImplementedException();
        }

        public IMediaStreamTrack Clone()
        {
            throw new NotImplementedException();
        }

        public MediaTrackCapabilities GetCapabilities()
        {
            throw new NotImplementedException();
        }

        public MediaTrackConstraints GetContraints()
        {
            throw new NotImplementedException();
        }

        public MediaTrackSettings GetSettings()
        {
            throw new NotImplementedException();
        }


        public void Stop()
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

        public UIView GetView<UIView>()
        {
            return (UIView)((object)_nativeCameraPreviewView);
        }
    }
}
