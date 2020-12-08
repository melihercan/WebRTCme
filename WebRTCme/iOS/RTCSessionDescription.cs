using System;
using System.Collections.Generic;
using System.Text;
using WebRTCme;

namespace WebRtc.iOS
{
    internal class RTCSessionDescription : ApiBase, IRTCSessionDescription
    {
        public static IRTCSessionDescription Create(Webrtc.RTCSessionDescription nativeSessionDescription) =>
            new RTCSessionDescription(nativeSessionDescription);

        private RTCSessionDescription(Webrtc.RTCSessionDescription nativeSessionDescription) : 
            base(nativeSessionDescription)
        {
        }

        public RTCSdpType Type => throw new NotImplementedException();

        public string Sdp => throw new NotImplementedException();

        public string ToJson()
        {
            throw new NotImplementedException();
        }
    }
}
