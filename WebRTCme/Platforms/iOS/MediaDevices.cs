using AVFoundation;
using System;
using System.Collections.Generic;
using System.Text;
using WebRTCme;
using System.Threading.Tasks;
using System.Linq;
using CoreAudioKit;
using AudioUnit;
using WebRTCme.Platforms.iOS.Custom;

namespace WebRTCme.iOS
{
    internal class MediaDevices : NativeBase<object>, IMediaDevices
    {
        public MediaDevices() { }

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
            // XAMARINIOS is not defined on .NET for iOS, so the placeholder media type below was
            // the live code path and could never match a device. AVMediaTypes.Audio is the modern
            // equivalent of the old AVMediaType.Audio constant.
            var audioCaptureDevices = AVCaptureDevice
                .DevicesWithMediaType(AVMediaTypes.Audio.GetConstant())
                .Select(device => new MediaDeviceInfo
                {
                    DeviceId = device.UniqueID,
                    GroupId = device.ModelID,
                    Kind = MediaDeviceInfoKind.AudioInput,
                    Label = device.LocalizedName
                });


#if TESTING
            //// TESTING TO GET LIST OF AUDIO OUTPUT DEVICES
            /// Apple don't want developers to change the output route/volume programmically. 
            /// https://stackoverflow.com/questions/29999393/avaudiosession-output-selection
            var x = Webrtc.RTCAudioSession.SharedInstance();
            var xouts = x.OutputDataSources;
            var xins = x.InputDataSources;

            var y = AVAudioSession.SharedInstance();
            y.SetCategory(AVAudioSessionCategory.PlayAndRecord/*, AVAudioSessionCategoryOptions.DefaultToSpeaker*/);
            y.SetActive(true);
            var cr = y.CurrentRoute;
            var outs2 = cr.Outputs;
            var ins2 = cr.Inputs;

            var outs = y.OutputDataSources;
            var ins = y.InputDataSources;

            var xxxx = outs;
            y.SetActive(false);
#endif



            return Task.FromResult(cameraCaptureDevices.Concat(audioCaptureDevices).ToArray());
        }

        public MediaTrackSupportedConstraints GetSupportedConstraints()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Captures the screen, as far as iOS lets an application capture it.
        /// </summary>
        /// <remarks>
        /// ReplayKit's in-process recorder, which captures <b>this application's own content
        /// only</b> - see <see cref="ScreenCapture"/> for why, and for what a system-wide share
        /// would additionally need. This threw <c>NotImplementedException</c> until 2026-09-12,
        /// so a caller that could not share at all now can, with a documented limit.
        ///
        /// Capture starts here rather than when a view binds the track, which is the opposite of
        /// the camera path: a camera has a device to open later, and a screen has nothing to wait
        /// for. Starting eagerly also means a refusal - recording unavailable, or already running
        /// - is reported to the caller that asked to share, rather than silently much later.
        /// </remarks>
        /// <exception cref="InvalidOperationException">Screen recording would not start.</exception>
        public async Task<IMediaStream> GetDisplayMedia(MediaStreamConstraints constraints)
        {
            var frameRate = (int?)constraints?.Video?.Object?.FrameRate?.Value ?? DefaultShareFrameRate;

            // Its own track id rather than a device id: there is no capture device behind a
            // screen, and IosSupport keys camera formats by device UniqueID.
            var track = MediaStreamTrack.Create(MediaStreamTrackKind.Video, $"screen:{Guid.NewGuid()}");

            try
            {
                await ScreenCapture.StartAsync(track, frameRate);
            }
            catch
            {
                track.Stop();
                throw;
            }

            return MediaStream.Create(new[] { track });
        }

        /// <summary>
        /// A shared screen is read rather than watched, so a lower rate leaves bandwidth for
        /// resolution - which is what keeps text legible. Matches the Windows default.
        /// </summary>
        internal const int DefaultShareFrameRate = 15;

        public Task<IMediaStream> GetUserMedia(MediaStreamConstraints constraints) =>
            Task.FromResult(MediaStream.Create(constraints));
    }
}
