using System;
using System.Collections.Generic;
using System.Text;

namespace WebRtc.iOS
{
    internal static class ModelExtensions
    {
        public static Webrtc.RTCConfiguration ToNative(this WebRTCme.RTCConfiguration configuration) => 
            new Webrtc.RTCConfiguration
            {

            };
    }
}
