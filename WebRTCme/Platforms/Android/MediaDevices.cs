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

        /// <summary>
        /// Captures the screen, after asking the user for permission.
        /// </summary>
        /// <remarks>
        /// The order below is fixed by Android and getting it wrong throws: the foreground service
        /// has to be running <b>before</b> the projection is used, because since Android 10 a
        /// <c>MediaProjection</c> can only be obtained while a service of type
        /// <c>mediaProjection</c> is already up.
        ///
        /// Audio is not captured. Android can mix system audio into a projection from API 29, but
        /// it is a separate source with its own consent implications, and a screen share that
        /// silently carried everything the device was playing is not what a caller asking for
        /// <c>getDisplayMedia</c> expects.
        ///
        /// The size comes from the display rather than from the constraints: capturing a screen at
        /// something other than its own aspect ratio bakes letterboxing into the frames, which is
        /// worse than sending the real shape and letting the far side fit it.
        /// </remarks>
        /// <exception cref="InvalidOperationException">The user refused, or dismissed the dialog.</exception>
        public async Task<IMediaStream> GetDisplayMedia(MediaStreamConstraints constraints)
        {
            var context = (global::Android.Content.Context)Platform.CurrentActivity
                ?? global::Android.App.Application.Context;

            // Asked for before the service is started, because a foreground service has to post a
            // notification and Android 13 made that a runtime permission. Refusal is not fatal -
            // the service still runs - so this does not check the answer, it only makes sure the
            // user was asked before the notification would otherwise have been dropped.
            if (global::Android.OS.Build.VERSION.SdkInt >= global::Android.OS.BuildVersionCodes.Tiramisu)
            {
                try { await Permissions.RequestAsync<Permissions.PostNotifications>(); }
                catch { /* Older MAUI targets do not know this permission; the service copes. */ }
            }

            var permission = await ScreenCapturePermissionActivity.RequestAsync(context)
                ?? throw new InvalidOperationException(
                    "Screen capture was not allowed. Android asks each time, and an answer " +
                    "authorises a single capture session.");

            // Started, and then *waited for*. startForegroundService returns immediately and the
            // service reaches the foreground later on the main looper; taking the projection in
            // between fails with a SecurityException that names the missing service and reads like
            // a manifest problem. Ten seconds is far longer than this takes and still bounded -
            // Android itself kills a service that has not started in about five.
            ScreenCaptureService.Start(context);

            var ready = await Task.WhenAny(
                ScreenCaptureService.ReadyAsync,
                Task.Delay(TimeSpan.FromSeconds(10)));

            if (ready != ScreenCaptureService.ReadyAsync || !ScreenCaptureService.ReadyAsync.Result)
            {
                ScreenCaptureService.Stop(context);
                throw new InvalidOperationException(
                    "The screen capture service did not reach the foreground, so the projection " +
                    "cannot be used. Its own log line says why.");
            }

            var metrics = context.Resources?.DisplayMetrics;
            var width = metrics?.WidthPixels ?? 1080;
            var height = metrics?.HeightPixels ?? 1920;
            var frameRate = VideoConstraints.From(constraints).FrameRate ?? 30;

            var track = MediaStreamTrack.Create(MediaStreamTrackKind.Video, $"screen:{WebRtc.Id}");

            try
            {
                AndroidSupport.StartScreenCapture(track, permission, width, height, frameRate);
            }
            catch
            {
                // The service is only justified by a capture that is actually running.
                ScreenCaptureService.Stop(context);
                throw;
            }

            return MediaStream.Create(new[] { track });
        }

        public Task<IMediaStream> GetUserMedia(MediaStreamConstraints constraints) =>
            Task.FromResult(MediaStream.Create(constraints));

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
