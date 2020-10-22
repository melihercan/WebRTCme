using AVFoundation;
using System;
using System.Collections.Generic;
using System.Text;
using WebRTCme;
using WebRTCme.Bindings.Xamarin.iOS;

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
                    //Kind = MediaDeviceInfoKind.
                    Label = cameraVideoCaptureDevice.Description
                });
            }

            var avCaptureDevices = AVCaptureDevice.Devices;
            foreach (var avCaptureDevice in avCaptureDevices)
            {
                mediaDeviceInfoList.Add(new MediaDeviceInfo
                {
                    DeviceId = avCaptureDevice.UniqueID,
                    GroupId = avCaptureDevice.ModelID,
                    //Kind = MediaDeviceInfoKind.
                    Label = avCaptureDevice.Description
                }); ;
            }



            return mediaDeviceInfoList;
        }

        public IMediaStream GetUserMedia(MediaStreamConstraints constraints)
        {
            return null;
        }
    }
}
