using Android.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WebRTCme.Android;
using WebRTCme;
using Xamarin.Essentials;
using Webrtc = Org.Webrtc;

namespace WebRTCme
{
    public static class AndroidSupport
    {
        public static Webrtc.VideoSource GetNativeVideoSource(IMediaStreamTrack videoTrack)
        {
            return ((MediaStreamTrack)videoTrack).GetNativeMediaSource() as Webrtc.VideoSource;
        }

        public static Webrtc.IEglBase GetNativeEglBase()
        {
            return WebRtc.NativeEglBase;
        }
    }
}

