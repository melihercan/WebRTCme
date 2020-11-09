using System;
using System.Collections.Generic;
using System.Text;
using WebRTCme;

namespace WebRtcGuiXamarin
{
    public static class WebRtcGui
    {
        internal static IWebRtc WebRtc { get; set; }

        internal static IWindow Window { get; private set; }
        internal static INavigator Navigator { get; private set; }


        public static void Initialize()
        {
            WebRtc = CrossWebRtc.Current;
            WebRtc.Initialize(); 

            //Window = WebRtc.Window;
            //Navigator = Window.Navigator;
            //var _mediaDevices = Navigator.MediaDevices();
            //var _mediaStream = _mediaDevices.GetUserMedia(new MediaStreamConstraints
            //{
            //    Audio = true,
            //    Video = true
            //});

            //_mediaStream.SetElementReferenceSrcObject(null);


        }

        public static void Cleanup()
        {
            WebRtc.Cleanup();
        }
    }
}
