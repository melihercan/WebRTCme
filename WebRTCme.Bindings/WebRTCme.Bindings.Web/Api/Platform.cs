using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Text;
using WebRtcBindingsWeb.Extensions;
using WebRtcBindingsWeb.Interops;
using WebRTCme;

namespace WebRtcBindingsWeb.Api
{
    internal class Platform : ApiBase, IPlatform
    {
        public static IPlatform Create(IJSRuntime jsRuntime) =>
            new Platform(jsRuntime);

        public IVideoView GetCameraView()
        {
            throw new NotImplementedException();
        }

        public IVideoRenderer GetCameraRenderer()
        {
            throw new NotImplementedException();
        }

        public IVideoCapturer GetCameraCapturer()
        {
            throw new NotImplementedException();
        }

        public IVideoCapturer StartCameraVideoCapturer(IMediaStreamTrack cameraVideoTrack, CameraType cameraType, MediaStreamConstraints mediaStreamConstraints = null)
        {
            throw new NotImplementedException();
        }

        private Platform(IJSRuntime jsRuntime) : base(jsRuntime) { }

    }
}
