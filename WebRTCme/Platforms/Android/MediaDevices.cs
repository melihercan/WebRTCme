using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebRTCme;
////#if! (NET6_0 || NET7_0 || NET8_0)
////using Xamarin.Essentials;
////#endif
using Webrtc = Org.Webrtc;
using Android.Media;
using Android.Hardware.Camera2;
using WebRTCme.Platforms.Android.Custom;

namespace WebRTCme.Android
{
    internal class MediaDevices : NativeBase<object>, IMediaDevices
    {
        public MediaDevices() { }

        public event EventHandler<IMediaStreamTrackEvent> OnDeviceChange;

        public Task<MediaDeviceInfo[]> EnumerateDevices()
        {
            var activity = Platform.CurrentActivity;
            var context = activity.ApplicationContext;

            var camera2Enumerator = new Webrtc.Camera2Enumerator(context);
            var camera2Names = camera2Enumerator.GetDeviceNames();
            var cameraCaptureDevices = camera2Names.Select(name => new MediaDeviceInfo
            {
                DeviceId = name,
                GroupId = name,
                Kind = MediaDeviceInfoKind.VideoInput,
                Label = name
            }).ToArray();
            
            var cm = (CameraManager)activity.GetSystemService(global::Android.Content.Context.CameraService);
            var cameraIds = cm.GetCameraIdList();
            foreach (var cameraId in cameraIds)
            {
                var cameraCaptureDevice = cameraCaptureDevices.FirstOrDefault(
                    name => name.DeviceId.Equals(cameraId, StringComparison.OrdinalIgnoreCase));
                if (cameraCaptureDevice is not null)
                {
                    var characteristics = cm.GetCameraCharacteristics(cameraId);
                    var facing = characteristics.Get(CameraCharacteristics.LensFacing);
                    ////var capabilities = characteristics.Get(CameraCharacteristics.RequestAvailableCapabilities);
                    if (((int)facing) == (int)LensFacing.Front)
                        cameraCaptureDevice.Label = LensFacing.Front.ToString();
                    else if (((int)facing) == (int)LensFacing.Back)
                        cameraCaptureDevice.Label = LensFacing.Back.ToString();
                    else if (((int)facing) == (int)LensFacing.External)
                        cameraCaptureDevice.Label = LensFacing.External.ToString();
                }
            }

            var audioManager = (AudioManager)context.GetSystemService(
                global::Android.Content.Context.AudioService);
            var inputs = audioManager.GetDevices(GetDevicesTargets.Inputs);
            var audioCaptureDevices = inputs.Select(input => new MediaDeviceInfo
            {
                DeviceId = input.Id.ToString(),
                GroupId = input.Id.ToString(),
                Kind = MediaDeviceInfoKind.AudioInput,
                Label = input.Type.ToString() + (string.IsNullOrEmpty(input.Address) ?
                    string.Empty : "." + input.Address)
            });
            var outputs = audioManager.GetDevices(GetDevicesTargets.Outputs);
            var audioRenderDevices = outputs.Select(output => new MediaDeviceInfo
            {
                DeviceId = output.Id.ToString(),
                GroupId = output.Id.ToString(),
                Kind = MediaDeviceInfoKind.AudioOutput,
                Label = output.Type.ToString() + (string.IsNullOrEmpty(output.Address) ?
                    string.Empty : "." + output.Address)
            });

            return Task.FromResult(
                cameraCaptureDevices.Concat(audioCaptureDevices).Concat(audioRenderDevices).ToArray());
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

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
