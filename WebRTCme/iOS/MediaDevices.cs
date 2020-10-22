using System;
using System.Collections.Generic;
using System.Text;
using WebRTCme;
using WebRTCme.Bindings.Xamarin.iOS;

namespace WebRtc.iOS
{
    internal class MediaDevices : ApiBase, IMediaDevices
    {
        public IEnumerable<IMediaDeviceInfo> EnumerateDevices()
        {
            var capturer = new RTCCameraVideoCapturer();
            var captureDevices = RTCCameraVideoCapturer.CaptureDevices;


            return null;
        }

        public IMediaStream GetUserMedia(MediaStreamConstraints constraints)
        {
            throw new NotImplementedException();
        }
    }
}
