using Webrtc = Org.Webrtc;
using System;
using WebRTCme;
using Org.Webrtc;

namespace WebRtc.Android
{
    internal class RTCIceCandidate : ApiBase, IRTCIceCandidate
    {
        public static IRTCIceCandidate Create(Webrtc.IceCandidate nativeIceCandidate) =>
            new RTCIceCandidate(nativeIceCandidate);

        private RTCIceCandidate(IceCandidate nativeIceCandidate) : base(nativeIceCandidate)
        {
        }

        public string Candidate => throw new NotImplementedException();

        public string Component => throw new NotImplementedException();

        public string Foundation => throw new NotImplementedException();

        public string Ip => throw new NotImplementedException();

        public ushort? Port => throw new NotImplementedException();

        public ulong? Priority => throw new NotImplementedException();

        public string Address => throw new NotImplementedException();

        public RTCIceProtocol Protocol { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public string RelatedAddress => throw new NotImplementedException();

        public ushort? RelatedPort => throw new NotImplementedException();

        public string SdpMid => ((Webrtc.IceCandidate)NativeObject).SdpMid;

        public ushort? SdpMLineIndex => (ushort)((Webrtc.IceCandidate)NativeObject).SdpMLineIndex;

        public RTCIceTcpCandidateType TcpType => throw new NotImplementedException();

        public string Type { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string UsernameFragment { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }


        public string ToJson()
        {
            throw new NotImplementedException();
        }
    }
}