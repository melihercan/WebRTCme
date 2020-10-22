using System;
using System.Collections.Generic;
using System.Text;
using WebRTCme;

namespace WebRtcGuiXamarin
{
    internal class VideoWebRtc
    {
        public VideoWebRtc()
        {
            var mediaDevices = WebRtcGui.Navigator.MediaDevices;

            var mediaDevicesInfo = mediaDevices.EnumerateDevices();

            var mediaStream = mediaDevices.GetUserMedia(new MediaStreamConstraints
            {
                Audio = true,
                Video = true
            });



        }
    }
}
