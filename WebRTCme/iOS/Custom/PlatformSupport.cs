﻿using AVFoundation;
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
        public static void SetCameraCapturer(IMediaStreamTrack videoTrack)
        {
            var nativeVideoTrack = videoTrack.NativeObject as Webrtc.RTCVideoTrack;
            var nativeVideoCapturer = new Webrtc.RTCFileVideoCapturer();
            nativeVideoCapturer.Delegate = nativeVideoTrack.Source;
        }

#if false
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
#endif

        public static UIView CreateCameraView(IMediaStreamTrack track, MediaTrackConstraints constraints = null)
        {
            var nativeVideoTrack = track.NativeObject as Webrtc.RTCVideoTrack;
            var nativeCameraVideoCapturer = new Webrtc.RTCCameraVideoCapturer();
            nativeCameraVideoCapturer.Delegate = nativeVideoTrack.Source;

            // TODO USE constraints to set the below values
            // Get the selected device by matching RTCMediaStreamTrack.TrackId with AVCaptureDevice.ModelId from
            // RTCCameraVideoCapturer.CaptureDevices list.
            var device = Webrtc.RTCCameraVideoCapturer.CaptureDevices
                .First(capturer => capturer.ModelID == track.Id);
            var format = Webrtc.RTCCameraVideoCapturer.SupportedFormatsForDevice(device)[0];
            var fps = 30;// GetFpsByFormat(format);
            nativeCameraVideoCapturer.StartCaptureWithDevice(device, format, fps);

            var videoView = new VideoView();
            nativeVideoTrack.AddRenderer(videoView);

            return videoView;            
        }


        //public static UIView GetCameraView()
        //{
        //    Webrtc.RTCEAGLVideoView _localRenderView;
        //    UIView _localView;

        //    _localRenderView = new Webrtc.RTCEAGLVideoView();
        //    _localRenderView.Delegate = this;
        //    _localView = new UIView();
        //    _localView.AddSubview(_localRenderView);

        //}


        //public static UIView CreateRoomView(IMediaStreamTrack track, MediaTrackConstraints constraints = null)
        //{
        //    var remoteView = new RemoteView();
        //    //    new Webrtc.RTCEAGLVideoView
        //    //{
        //    //    Delegate = new VideoViewDelegate()
        //    //};



        //    var nativeVideoTrack = track.NativeObject as Webrtc.RTCVideoTrack;
        //    nativeVideoTrack.AddRenderer(remoteView);

        //    return remoteView;
        //}


        public static UIView CreateRoomView(IMediaStreamTrack track, MediaTrackConstraints constraints = null)
        {
            var videoView = new VideoView();

            var nativeVideoTrack = track.NativeObject as Webrtc.RTCVideoTrack;
            nativeVideoTrack.AddRenderer(videoView);

            return videoView;
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

        internal class VideoView : Webrtc.RTCEAGLVideoView, Webrtc.IRTCVideoViewDelegate
        {
            public VideoView()
            {
                Delegate = this;
            }

            [Export("videoView:didChangeVideoSize:")]
            public void DidChangeVideoSize(Webrtc.IRTCVideoRenderer videoView, CGSize size)
            {
                //if (videoView is Webrtc.RTCEAGLVideoView rendererView &&
                //    rendererView.Superview is UIView parentView)
                //{
                //    var constraints = parentView.Constraints
                //        .Where(lc => lc.SecondAttribute == NSLayoutAttribute.Width ||
                //                     lc.SecondAttribute == NSLayoutAttribute.Height)
                //        .ToArray();
                //    parentView.RemoveConstraints(constraints);

                //    var isLandscape = size.Width > size.Height;

                //    if (isLandscape)
                //    {
                //        parentView.AddConstraints(new[]
                //        {
                //            rendererView.WithSameWidth(parentView),
                //            rendererView.Height()
                //                .EqualTo()
                //                .WidthOf(parentView)
                //                .WithMultiplier(size.Height / size.Width)
                //        });
                //    }
                //    else
                //    {
                //        parentView.AddConstraints(new[]
                //        {
                //            rendererView.Width()
                //                .EqualTo()
                //                .HeightOf(parentView)
                //                .WithMultiplier(size.Width / size.Height),
                //            rendererView.WithSameHeight(parentView)
                //        });
                //    }
                //}
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
