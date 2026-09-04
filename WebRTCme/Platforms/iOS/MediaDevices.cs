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

        public Task<IMediaStream> GetDisplayMedia(MediaStreamConstraints constraints)
        {
            throw new NotImplementedException();
        }

        public Task<IMediaStream> GetUserMedia(MediaStreamConstraints constraints) =>
            Task.FromResult(MediaStream.Create(constraints));
    }
}
