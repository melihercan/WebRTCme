using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using WebRTCme;

namespace WebRtc.iOS
{
    internal class RTCIceCandidate : ApiBase, IRTCIceCandidate
    {
        public static IRTCIceCandidate Create(Webrtc.RTCIceCandidate nativeIceCandidate) =>
            new RTCIceCandidate(nativeIceCandidate);

        private RTCIceCandidate(Webrtc.RTCIceCandidate nativeIceCandidate) : base(nativeIceCandidate) { }

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

        public string SdpMid => throw new NotImplementedException();

        public ushort? SdpMLineIndex => throw new NotImplementedException();

        public RTCIceTcpCandidateType TcpType => throw new NotImplementedException();

        public string Type { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string UsernameFragment { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }


        public string ToJson()
        {
            throw new NotImplementedException();
        }


    }
}
