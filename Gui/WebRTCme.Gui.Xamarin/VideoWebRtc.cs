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
                Audio = new MediaStreamContraintsUnion
                { 
                    Value = true
                },
                Video = new MediaStreamContraintsUnion
                {
                    Value = true
                }
            });
        }
    }
}
