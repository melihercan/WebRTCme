using Webrtc = Org.Webrtc;
using System;
using WebRTCme;
using Org.Webrtc;

namespace WebRTCme.Android
{
    internal class RTCIceCandidate : ApiBase, IRTCIceCandidate
    {
        public static IRTCIceCandidate Create(Webrtc.IceCandidate nativeIceCandidate) =>
            new RTCIceCandidate(nativeIceCandidate);

        private RTCIceCandidate(IceCandidate nativeIceCandidate) : base(nativeIceCandidate)
        {
        }

        public string Candidate => ((Webrtc.IceCandidate)NativeObject).Sdp;

        public RTCIceComponent Component => Candidate
            .Replace("candidate:", string.Empty, StringComparison.OrdinalIgnoreCase)
            .Split(" ", StringSplitOptions.RemoveEmptyEntries)[1] == "1" 
                ? RTCIceComponent.Rtp : RTCIceComponent.Rtcp;

        public string Foundation => Candidate
            .Replace("candidate:", string.Empty, StringComparison.OrdinalIgnoreCase)
            .Split(" ", StringSplitOptions.RemoveEmptyEntries)[0];

        public string Ip => Address;

        public ushort Port => Convert.ToUInt16(Candidate
            .Replace("candidate:", string.Empty, StringComparison.OrdinalIgnoreCase)
            .Split(" ", StringSplitOptions.RemoveEmptyEntries)[5]);

        public uint Priority => Convert.ToUInt32(Candidate
            .Replace("candidate:", string.Empty, StringComparison.OrdinalIgnoreCase)
            .Split(" ", StringSplitOptions.RemoveEmptyEntries)[3]);

        public string Address => Candidate
            .Replace("candidate:", string.Empty, StringComparison.OrdinalIgnoreCase)
            .Split(" ", StringSplitOptions.RemoveEmptyEntries)[4];

        public RTCIceProtocol Protocol => (RTCIceProtocol)Enum.Parse(
            typeof(RTCIceProtocol),
            Candidate
                .Replace("candidate:", string.Empty, StringComparison.OrdinalIgnoreCase)
                .Split(" ", StringSplitOptions.RemoveEmptyEntries)[2],
            true);

        public string RelatedAddress => throw new NotImplementedException();

        public ushort? RelatedPort => throw new NotImplementedException();

        public string SdpMid => ((Webrtc.IceCandidate)NativeObject).SdpMid;

        public ushort? SdpMLineIndex => (ushort)((Webrtc.IceCandidate)NativeObject).SdpMLineIndex;

        public RTCIceTcpCandidateType? TcpType => throw new NotImplementedException();

        public RTCIceCandidateType Type => throw new NotImplementedException();
        public string UsernameFragment => throw new NotImplementedException(); 


        public string ToJson()
        {
            throw new NotImplementedException();
        }
    }
}