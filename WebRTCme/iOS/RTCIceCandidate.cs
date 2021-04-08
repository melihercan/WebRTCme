using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using WebRTCme;

namespace WebRTCme.iOS
{
    internal class RTCIceCandidate : ApiBase, IRTCIceCandidate
    {
        public static IRTCIceCandidate Create(Webrtc.RTCIceCandidate nativeIceCandidate) =>
            new RTCIceCandidate(nativeIceCandidate);

        private RTCIceCandidate(Webrtc.RTCIceCandidate nativeIceCandidate) : base(nativeIceCandidate) { }

        public string Candidate => ((Webrtc.RTCIceCandidate)NativeObject).Sdp;

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

        public string SdpMid => ((Webrtc.RTCIceCandidate)NativeObject).SdpMid;

        public ushort? SdpMLineIndex => (ushort)((Webrtc.RTCIceCandidate)NativeObject).SdpMLineIndex;

        public RTCIceTcpCandidateType? TcpType => throw new NotImplementedException();

        public RTCIceCandidateType Type => throw new NotImplementedException();
        
        public string UsernameFragment => throw new NotImplementedException();


        public string ToJson()
        {
            throw new NotImplementedException();
        }


    }
}
