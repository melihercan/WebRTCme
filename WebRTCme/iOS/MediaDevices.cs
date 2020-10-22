using AVFoundation;
using System;
using System.Collections.Generic;
using System.Text;
using WebRTCme;
//using WebRTCme.Bindings.Xamarin.iOS;
using Webrtc;

namespace WebRtc.iOS
{
    internal class MediaDevices : ApiBase, IMediaDevices
    {
        public IEnumerable<MediaDeviceInfo> EnumerateDevices()
        {
            var mediaDeviceInfoList = new List<MediaDeviceInfo>();

            ////var cameraVideoCapturer = new RTCCameraVideoCapturer();
            var cameraVideoCaptureDevices = RTCCameraVideoCapturer.CaptureDevices;
            foreach (var cameraVideoCaptureDevice in cameraVideoCaptureDevices)
            {
                mediaDeviceInfoList.Add(new MediaDeviceInfo
                {
                    DeviceId = cameraVideoCaptureDevice.UniqueID,
                    GroupId = cameraVideoCaptureDevice.ModelID,
                    Kind = MediaDeviceInfoKind.VideoInput,
                    Label = cameraVideoCaptureDevice.LocalizedName
                });
            }

            var avCaptureDevices = AVCaptureDeviceDiscoverySession.Create(new AVCaptureDeviceType[]
                { AVCaptureDeviceType.BuiltInMicrophone }, AVMediaType.Audio, AVCaptureDevicePosition.Unspecified)
                .Devices;
            foreach (var avCaptureDevice in avCaptureDevices)
            {
                mediaDeviceInfoList.Add(new MediaDeviceInfo
                {
                    DeviceId = avCaptureDevice.UniqueID,
                    GroupId = avCaptureDevice.ModelID,
                    Kind = MediaDeviceInfoKind.AudioInput,
                    Label = avCaptureDevice.LocalizedName
                });
            }


# if false
//// DEPRECATED
            avCaptureDevices = AVCaptureDevice.Devices;
            foreach (var avCaptureDevice in avCaptureDevices)
            {
                mediaDeviceInfoList.Add(new MediaDeviceInfo
                {
                    DeviceId = avCaptureDevice.UniqueID,
                    GroupId = avCaptureDevice.ModelID,
                    Kind = MediaDeviceInfoKind.AudioInput,
                    Label = avCaptureDevice.LocalizedName
                });
            }
#endif

            //// TODO: How to get AudioOutput device list???


            return mediaDeviceInfoList;
        }

        public IMediaStream GetUserMedia(MediaStreamConstraints constraints)
        {
            return null;
        }
    }
}
