using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebRtc.Android;
using WebRTCme;
using Xamarin.Essentials;
using Webrtc = Org.Webrtc;

namespace WebRtc.Android
{
    internal class EglBaseContext : ApiBase, IEglBaseContext
    {
        public static IEglBaseContext Create(Webrtc.IEglBaseContext eglBaseContext) => 
            new EglBaseContext(eglBaseContext);

        private EglBaseContext(Webrtc.IEglBaseContext eglBaseContext) : base(eglBaseContext)
        { }

    }
}
