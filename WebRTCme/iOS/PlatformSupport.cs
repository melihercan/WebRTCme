using AVFoundation;
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
            //// TODO: HOW TO GET AVCaptureDevice from track??? from TrackId or Source????
            /// FOUND: match TrackID with AVCaptureDevice.ModelId ( Webrtc.RTCCameraVideoCapturer.CaptureDevices)





            ////            var nativeTrack = (Webrtc.RTCMediaStreamTrack)track.NativeObject;
            ////            var nativeCaptureDevices = Webrtc.RTCCameraVideoCapturer.CaptureDevices;




            ////            var nativeVideoSource = ((Webrtc.RTCVideoTrack)track.NativeObject).Source;
            var nativeCameraVideoCapturer = new Webrtc.RTCCameraVideoCapturer(/****nativeVideoSource****/);


            // TODO USE constraints to set the below values

            // Get the selected device by matching RTCMediaStreamTrack.TrackId with AVCaptureDevice.ModelId from
            // RTCCameraVideoCapturer.CaptureDevices list.

            //var videoDevices = AVCaptureDevice.DevicesWithMediaType(AVMediaType.Video);
            //var cameraPosition = AVCaptureDevicePosition.Front;// AVCaptureDevicePosition.Back;
            //var device = videoDevices.FirstOrDefault(d => d.Position == cameraPosition);
            var device = Webrtc.RTCCameraVideoCapturer.CaptureDevices
                .FirstOrDefault(capturer => capturer.ModelID == ((Webrtc.RTCMediaStreamTrack)track.NativeObject).TrackId);
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
    }
}

///// !!!!AddRenderer will not work with RTCCameraPreviewView as it is no derived from RTCVideoRenderer
/// !!!! SO USE IT WITH REMOTE 
//var renderer = (UIView)_nativeCameraPreviewView;    ////????
//((RTCVideoTrack)NativeObject).AddRenderer((IRTCVideoRenderer)renderer);

/// TODO: If local, RTCCameraVideoCapturer or RTCFileVideoCapturer???
/// CURENTLY Camera is assumed.

//void AddMp4VideoStreamTrack()

//void AddRemoteVideoStreamTrack()

