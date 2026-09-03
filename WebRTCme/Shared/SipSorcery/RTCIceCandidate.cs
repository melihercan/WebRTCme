using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using WebRTCme.Shared.SipSorcery.Custom;

namespace WebRTCme.Shared.SipSorcery
{
    internal class RTCIceCandidate : NativeBase<SIPSorcery.Net.RTCIceCandidate>, IRTCIceCandidate
    {
        public RTCIceCandidate(SIPSorcery.Net.RTCIceCandidate nativeIceCandidate) : base(nativeIceCandidate)
        {
        }

        /// <summary>
        /// SIPSorcery yields the candidate without the "candidate:" prefix, but the WebRTC API
        /// (and Google's parser on the receiving end) expects the full attribute value. Sending
        /// the bare form aborts libwebrtc inside addIceCandidate, taking the remote peer's
        /// process down with it.
        /// </summary>
        public string Candidate => NativeObject.candidate is null
            ? null
            : NativeObject.candidate.StartsWith(CandidatePrefix, StringComparison.OrdinalIgnoreCase)
                ? NativeObject.candidate
                : CandidatePrefix + NativeObject.candidate;

        private const string CandidatePrefix = "candidate:";

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

        /// <summary>
        /// SIPSorcery leaves sdpMid unset on some candidates. Peers that key off sdpMid rather
        /// than the m-line index would then discard them, so fall back to the index - SIPSorcery
        /// numbers its own media sections "0", "1", ... so the two agree.
        /// </summary>
        public string SdpMid => NativeObject.sdpMid
            ?? NativeObject.sdpMLineIndex.ToString(CultureInfo.InvariantCulture);

        public ushort? SdpMLineIndex => NativeObject.sdpMLineIndex;

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

        public string ToJson() => JsonSerializer.Serialize(this);











        public void Dispose()
        {
        }

    }
}
