using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using WebRTCme;
using WebRTCme.Platforms.iOS.Custom;

namespace WebRTCme.iOS
{
    internal class RTCIceCandidate : NativeBase<Webrtc.RTCIceCandidate>, IRTCIceCandidate
    {
        public RTCIceCandidate(Webrtc.RTCIceCandidate nativeIceCandidate) : base(nativeIceCandidate) { }

        public string Candidate => NativeObject.Sdp;

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

        public string RelatedAddress
        {
            get
            {
                var array = Candidate
                    .Replace("candidate:", string.Empty, StringComparison.OrdinalIgnoreCase)
                    .Split(" ", StringSplitOptions.RemoveEmptyEntries);
                var index = array.ToList().FindIndex(s => s.Equals("raddr", StringComparison.OrdinalIgnoreCase));
                if (index == -1)
                    return null;
                else
                    return array[index + 1];
            }
        }

        public ushort? RelatedPort
        {
            get
            {
                var array = Candidate
                    .Replace("candidate:", string.Empty, StringComparison.OrdinalIgnoreCase)
                    .Split(" ", StringSplitOptions.RemoveEmptyEntries);
                var index = array.ToList().FindIndex(s => s.Equals("rport", StringComparison.OrdinalIgnoreCase));
                if (index == -1)
                    return null;
                else
                    return Convert.ToUInt16(array[index + 1]);
            }
        }

        public string SdpMid => NativeObject.SdpMid;

        public ushort? SdpMLineIndex => (ushort)NativeObject.SdpMLineIndex;

        public RTCIceTcpCandidateType? TcpType
        {
            get
            {
                var array = Candidate
                    .Replace("candidate:", string.Empty, StringComparison.OrdinalIgnoreCase)
                    .Split(" ", StringSplitOptions.RemoveEmptyEntries);
                var index = array.ToList().FindIndex(s => s.Equals("tcptype", StringComparison.OrdinalIgnoreCase));
                if (index == -1)
                    return null;
                else
                    return (RTCIceTcpCandidateType)Enum.Parse(
                        typeof(RTCIceTcpCandidateType),
                        array[index + 1],
                        true);
            }
        }

        public RTCIceCandidateType Type => (RTCIceCandidateType)Enum.Parse(
            typeof(RTCIceCandidateType),
            Candidate
                .Replace("candidate:", string.Empty, StringComparison.OrdinalIgnoreCase)
                .Split(" ", StringSplitOptions.RemoveEmptyEntries)[7],
            true);

        public string UsernameFragment => null;


        public string ToJson()
        {
            throw new NotImplementedException();
        }


    }
}
