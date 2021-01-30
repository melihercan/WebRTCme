using AVFoundation;
using CoreGraphics;
using Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UIKit;

namespace WebRTCme
{
    public static class PlatformSupport
    {
        public static UIView CreateCameraView(IMediaStreamTrack track, MediaTrackConstraints constraints = null)
        {
            var nativeCameraVideoCapturer = new Webrtc.RTCCameraVideoCapturer();

            // TODO USE constraints to set the below values
            // Get the selected device by matching RTCMediaStreamTrack.TrackId with AVCaptureDevice.ModelId from
            // RTCCameraVideoCapturer.CaptureDevices list.
            var device = Webrtc.RTCCameraVideoCapturer.CaptureDevices
                .First(capturer => capturer.ModelID == track.Id);
            var format = Webrtc.RTCCameraVideoCapturer.SupportedFormatsForDevice(device)[0];
            var fps = GetFpsByFormat(format);
            nativeCameraVideoCapturer.StartCaptureWithDevice(device, format, fps);

            var nativeCameraPreviewView = new Webrtc.RTCCameraPreviewView();
            nativeCameraPreviewView.CaptureSession = nativeCameraVideoCapturer.CaptureSession;
            return nativeCameraPreviewView;

            int GetFpsByFormat(AVCaptureDeviceFormat fmt)
            {
                const float _frameRateLimit = 30.0f;

                var maxSupportedFps = 0d;
                foreach (var fpsRange in fmt.VideoSupportedFrameRateRanges)
                    maxSupportedFps = Math.Max(maxSupportedFps, fpsRange.MaxFrameRate);

                return (int)Math.Min(maxSupportedFps, _frameRateLimit);
            }
        }

        public static UIView CreateRoomView(IMediaStreamTrack track, MediaTrackConstraints constraints = null)
        {
            var remoteView = new RemoteView();
            //    new Webrtc.RTCEAGLVideoView
            //{
            //    Delegate = new VideoViewDelegate()
            //};



            var nativeVideoTrack = track.NativeObject as Webrtc.RTCVideoTrack;
            nativeVideoTrack.AddRenderer(remoteView);

            return remoteView;
        }

        //internal class VideoViewDelegate : NSObject, Webrtc.IRTCVideoViewDelegate
        //{
        //    void DidChangeVideoSize(Webrtc.IRTCVideoRenderer videoView, CGSize size)
        //    {

        //    }
        //}

        internal class RemoteView : Webrtc.RTCEAGLVideoView, Webrtc.IRTCVideoViewDelegate
        {
            public RemoteView()
            {
                Delegate = this;
            }

            [Export("videoView:didChangeVideoSize:")]
            public void DidChangeVideoSize(Webrtc.IRTCVideoRenderer videoView, CGSize size)
            {
            }

        }
    }
}

///// !!!!AddRenderer will not work with RTCCameraPreviewView as it is not derived from RTCVideoRenderer
/// !!!! SO USE IT WITH REMOTE 
//var renderer = (UIView)_nativeCameraPreviewView;    ////????
//((RTCVideoTrack)NativeObject).AddRenderer((IRTCVideoRenderer)renderer);

/// TODO: If local, RTCCameraVideoCapturer or RTCFileVideoCapturer???
/// CURENTLY Camera is assumed.

//void AddMp4VideoStreamTrack()

//void AddRemoteVideoStreamTrack()

