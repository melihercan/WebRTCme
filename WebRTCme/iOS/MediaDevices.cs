using AVFoundation;
using System;
using System.Collections.Generic;
using System.Text;
using WebRTCme;
//using WebRTCme.Bindings.Xamarin.iOS;
using Webrtc;
using System.Threading.Tasks;
using System.Linq;

namespace WebRtc.iOS
{
    internal class MediaDevices : ApiBase, IMediaDevices
    {

        public static Task<IMediaDevices> CreateAsync()
        {
            var ret = new MediaDevices();
            return ret.InitializeAsync();
        }

        private MediaDevices() { }

        private Task<IMediaDevices> InitializeAsync() => Task.FromResult(this as IMediaDevices);
        
        public event EventHandler<IMediaStreamTrackEvent> OnDeviceChange;

        public Task<IEnumerable<MediaDeviceInfo>> EnumerateDevices()
        {
            var mediaDeviceInfoList = new List<MediaDeviceInfo>();

            //var cameraCaptureDevices = AVCaptureDevice.DevicesWithMediaType(AVMediaType.Video);
            var cameraCaptureDevices = RTCCameraVideoCapturer.CaptureDevices;
            foreach (var cameraCaptureDevice in cameraCaptureDevices)
            {
                mediaDeviceInfoList.Add(new MediaDeviceInfo
                {
                    DeviceId = cameraCaptureDevice.UniqueID,
                    GroupId = cameraCaptureDevice.ModelID,
                    Kind = MediaDeviceInfoKind.VideoInput,
                    Label = cameraCaptureDevice.LocalizedName
                });
            }

            //var audioCaptureDevices = AVCaptureDeviceDiscoverySession.Create(new AVCaptureDeviceType[]
              //  { AVCaptureDeviceType.BuiltInMicrophone }, AVMediaType.Audio, AVCaptureDevicePosition.Unspecified)
                //.Devices;
            var audioCaptureDevices = AVCaptureDevice.DevicesWithMediaType(AVMediaType.Audio);
            foreach (var audioCaptureDevice in audioCaptureDevices)
            {
                mediaDeviceInfoList.Add(new MediaDeviceInfo
                {
                    DeviceId = audioCaptureDevice.UniqueID,
                    GroupId = audioCaptureDevice.ModelID,
                    Kind = MediaDeviceInfoKind.AudioInput,
                    Label = audioCaptureDevice.LocalizedName
                });
            }

             return Task.FromResult(mediaDeviceInfoList.AsEnumerable());

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

        public Task<IMediaStream> GetDisplayMedia(MediaStreamConstraints constraints)
        {
            throw new NotImplementedException();
        }

        public Task<MediaTrackSupportedConstraints> GetSupportedConstraints()
        {
            throw new NotImplementedException();
        }

        public Task<IMediaStream> GetUserMedia(MediaStreamConstraints constraints)
        {
            ////            var video = AVCaptureDevice.GetDefaultDevice(AVMediaTypes.Video);
            ////            var audio = AVCaptureDevice.GetDefaultDevice(AVMediaTypes.Audio);
            ////            var mediaStream = new MediaStream(/*tracks*/);
            ////            return null;
            ///
            return MediaStream.CreateAsync(constraints);
        }



        Task<MediaTrackSupportedConstraints> IMediaDevices.GetSupportedConstraints()
        {
            throw new NotImplementedException();
        }

    }
}
