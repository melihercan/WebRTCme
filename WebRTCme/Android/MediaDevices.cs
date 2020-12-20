using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebRTCme;
using Xamarin.Essentials;
using Webrtc = Org.Webrtc;
//using Android.Hardware.Camera2;
using Android.Media;

namespace WebRtc.Android
{
    internal class MediaDevices : ApiBase, IMediaDevices
    {
        public static IMediaDevices Create() => new MediaDevices();

        private MediaDevices() { }

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
            });
            //var cm = (CameraManager)activity.GetSystemService(global::Android.Content.Context.CameraService);
            //var list = cm.GetCameraIdList();

            var audioManager = (AudioManager)/*activity*/context.GetSystemService(global::Android.Content.Context.AudioService);
            var inputs = audioManager.GetDevices(GetDevicesTargets.Inputs);
            var audioCaptureDevices = inputs.Select(input => new MediaDeviceInfo
            {
                DeviceId = input.Id.ToString(),
                GroupId = input.Id.ToString(),
                Kind = MediaDeviceInfoKind.AudioInput,
                Label = input.Address
            });
            var outputs = audioManager.GetDevices(GetDevicesTargets.Outputs);
            var audioRenderDevices = outputs.Select(output => new MediaDeviceInfo
            {
                DeviceId = output.Id.ToString(),
                GroupId = output.Id.ToString(),
                Kind = MediaDeviceInfoKind.AudioOutput,
                Label = output.Address
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
    }
}
