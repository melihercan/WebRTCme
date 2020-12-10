using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebRTCme;
using Xamarin.Essentials;
using Webrtc = Org.Webrtc;

namespace WebRtc.Android
{
    internal class MediaDevices : ApiBase, IMediaDevices
    {
        public static IMediaDevices Create() => new MediaDevices();

        private MediaDevices() { }


        public event EventHandler<IMediaStreamTrackEvent> OnDeviceChange;

        public Task<MediaDeviceInfo[]> EnumerateDevices()
        {
            var camera1Enumerator = new Webrtc.Camera1Enumerator();
            var names1 = camera1Enumerator.GetDeviceNames();

            var context = Platform.CurrentActivity.ApplicationContext;
            var camera2Enumerator = new Webrtc.Camera2Enumerator(context);
            var names2 = camera2Enumerator.GetDeviceNames();

            return null;
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
            throw new NotImplementedException();
        }

    }
}
