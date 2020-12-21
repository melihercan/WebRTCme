using WebRTCme;
using Webrtc = Org.Webrtc;

namespace WebRtc.Android
{
    internal class RTCSessionDescription : ApiBase, IRTCSessionDescription
    {
        public static IRTCSessionDescription Create(Webrtc.SessionDescription nativeSessionDescription) =>
            new RTCSessionDescription(nativeSessionDescription);

        private RTCSessionDescription(Webrtc.SessionDescription nativeSessionDescription) :
            base(nativeSessionDescription)
        {
        }

        public RTCSdpType Type => throw new System.NotImplementedException();

        public string Sdp => throw new System.NotImplementedException();

        public string ToJson()
        {
            throw new System.NotImplementedException();
        }
    }
}