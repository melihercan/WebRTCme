using AVFoundation;
using System;
using System.Collections.Generic;
using System.Text;
using WebRTCme;
using System.Threading.Tasks;
using System.Linq;

namespace WebRtcMe.iOS
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

            var audioCaptureDevices = AVCaptureDevice.DevicesWithMediaType(AVMediaType.Audio)
                .Select(device => new MediaDeviceInfo
                {
                    DeviceId = device.UniqueID,
                    GroupId = device.ModelID,
                    Kind = MediaDeviceInfoKind.AudioInput,
                    Label = device.LocalizedName
                });

            return Task.FromResult(cameraCaptureDevices.Concat(audioCaptureDevices).ToArray());
        }

        public MediaTrackSupportedConstraints GetSupportedConstraints()
        {
            throw new NotImplementedException();
        }

        public Task<IMediaStream> GetDisplayMedia(MediaStreamConstraints constraints)
        {
            throw new NotImplementedException();
        }

        public Task<IMediaStream> GetUserMedia(MediaStreamConstraints constraints) =>
            Task.FromResult(MediaStream.Create(constraints));
    }
}
