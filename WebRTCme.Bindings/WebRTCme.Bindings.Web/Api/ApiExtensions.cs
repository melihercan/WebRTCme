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
    internal class ApiExtensions : ApiBase, IApiExtensions
    {
        public static IApiExtensions Create(IJSRuntime jsRuntime) =>
            new ApiExtensions(jsRuntime);

        private ApiExtensions(IJSRuntime jsRuntime) : base(jsRuntime) { }


        public void SetCameraVideoCapturer(IMediaStreamTrack cameraVideoTrack, CameraType cameraType, MediaStreamConstraints mediaStreamConstraints)
        {
            throw new NotImplementedException();
        }
    }
}
