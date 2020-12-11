using AVFoundation;
using System;
using System.Collections.Generic;
using System.Text;
using WebRTCme;
using System.Threading.Tasks;
using System.Linq;

namespace WebRtc.iOS
{
    internal class MediaDevices : ApiBase, IMediaDevices
    {
        public static IMediaDevices Create() => new MediaDevices();

        private MediaDevices() { }

        public event EventHandler<IMediaStreamTrackEvent> OnDeviceChange;

        public Task<MediaDeviceInfo[]> EnumerateDevices()
        {
            var cameraCaptureDevices = Webrtc.RTCCameraVideoCapturer.CaptureDevices
                .Select(device => new MediaDeviceInfo
                {
                    DeviceId = device.UniqueID,
                    GroupId = device.ModelID,
                    Kind = MediaDeviceInfoKind.VideoInput,
                    Label = device.LocalizedName
                });

            //var audioCaptureDevices = AVCaptureDeviceDiscoverySession.Create(new AVCaptureDeviceType[]
            //  { AVCaptureDeviceType.BuiltInMicrophone }, AVMediaType.Audio, AVCaptureDevicePosition.Unspecified)
            //.Devices;
            var audioCaptureDevices = AVCaptureDevice.DevicesWithMediaType(AVMediaType.Audio)
                .Select(device => new MediaDeviceInfo
                {
                    DeviceId = device.UniqueID,
                    GroupId = device.ModelID,
                    Kind = MediaDeviceInfoKind.AudioInput,
                    Label = device.LocalizedName
                });

            return Task.FromResult(cameraCaptureDevices.Concat(audioCaptureDevices).ToArray());

#if false
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



#if false
            var cameraVideoCapturer = new RTCCameraVideoCapturer();
            var device = AVCaptureDevice.GetDefaultDevice(AVMediaTypes.Video);
            var format = RTCCameraVideoCapturer.SupportedFormatsForDevice(device)[0];
            var fps = GetFpsByFormat(format);
            ////cameraVideoCapturer.StartCaptureWithDevice(device, format, fps);





            return mediaDeviceInfoList;

            int GetFpsByFormat(AVCaptureDeviceFormat fmt)
            {
                const float _frameRateLimit = 30.0f;

                var maxSupportedFps = 0d;
                foreach (var fpsRange in fmt.VideoSupportedFrameRateRanges) maxSupportedFps = Math.Max(maxSupportedFps, fpsRange.MaxFrameRate);

                return (int)Math.Min(maxSupportedFps, _frameRateLimit);
            }

#endif
        }

        public MediaTrackSupportedConstraints GetSupportedConstraints()
        {
            throw new NotImplementedException();
        }

        public Task<IMediaStream> GetDisplayMedia(MediaStreamConstraints constraints)
        {
            throw new NotImplementedException();
        }

        public Task<IMediaStream> GetUserMedia(MediaStreamConstraints constraints)
        {
            return Task.FromResult(MediaStream.Create(constraints));
        }
    }
}
