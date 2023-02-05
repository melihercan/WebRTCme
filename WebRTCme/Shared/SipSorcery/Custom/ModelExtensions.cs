using WebRTCme.Shared.SipSorcery.Custom;

namespace WebRTCme.Shared.SipSorcery.Custom
{
    internal static class ModelExtensions
    {
        public static SIPSorcery.Net.RTCConfiguration ToNative(this RTCConfiguration configuration)
        {
            RTCIceServer[] iceServers = configuration.IceServers ?? new RTCIceServer[] { };

            SIPSorcery.Net.RTCConfiguration nativeConfiguration = new()
            {
                iceServers = iceServers.Select(s => s.ToNative()).ToList(),
                iceTransportPolicy = configuration.IceTransportPolicy.HasValue ?
                    configuration.IceTransportPolicy.Value.ToNative() : SIPSorcery.Net.RTCIceTransportPolicy.all,
                bundlePolicy = configuration.BundlePolicy.HasValue ?
                    configuration.BundlePolicy.Value.ToNative() : SIPSorcery.Net.RTCBundlePolicy.balanced,
                rtcpMuxPolicy = configuration.RtcpMuxPolicy.HasValue ?
                    configuration.RtcpMuxPolicy.Value.ToNative() : SIPSorcery.Net.RTCRtcpMuxPolicy.require,
                //certificates = 
                //certificates2 = 
            };

            return nativeConfiguration;
        }

        public static SIPSorcery.Net.RTCIceServer ToNative(this RTCIceServer iceServer)
        {
            //// TODO iceServer.Urls is an array
            SIPSorcery.Net.RTCIceServer nativeIceServer = new()
            {
                urls = iceServer.Urls[0],
                username = iceServer.Username,
                credentialType = iceServer.CredentialType.HasValue ?
                    iceServer.CredentialType.Value.ToNative() : SIPSorcery.Net.RTCIceCredentialType.password,
                credential = iceServer.Credential,
            };
            return nativeIceServer;
        }

        public static SIPSorcery.Net.RTCDataChannelInit ToNative(this RTCDataChannelInit dataChannelInit) =>
            new SIPSorcery.Net.RTCDataChannelInit
            {
                ordered = dataChannelInit.Ordered ?? true,
                maxRetransmits = dataChannelInit.MaxRetransmits ?? 0,
                protocol = dataChannelInit.Protocol ?? string.Empty,
                negotiated = dataChannelInit.Negotiated ?? false,
                id = dataChannelInit.Id.HasValue ? (ushort)dataChannelInit.Id.Value : (ushort)0//WebRTCme.WebRtc.Id
            };

        public static SIPSorcery.Net.RTCOfferOptions ToNative(this RTCOfferOptions offerOptions) =>
            new SIPSorcery.Net.RTCOfferOptions
            {
                X_ExcludeIceCandidates = false
            };

        public static SIPSorcery.Net.RTCAnswerOptions ToNative(this RTCAnswerOptions answerOptions) =>
            new SIPSorcery.Net.RTCAnswerOptions
            {
                X_ExcludeIceCandidates = false
            };

        public static SIPSorcery.Net.RTCSessionDescriptionInit ToNative(this RTCSessionDescriptionInit sessionDescription) =>
            new SIPSorcery.Net.RTCSessionDescriptionInit
            {
                type = sessionDescription.Type.ToNative(),
                sdp = sessionDescription.Sdp
            };

        public static SIPSorcery.Net.RTCIceCandidateInit ToNative(this RTCIceCandidateInit iceCandidate) =>
            new SIPSorcery.Net.RTCIceCandidateInit
            {
                candidate = iceCandidate.Candidate,
                sdpMid = iceCandidate.SdpMid,
                sdpMLineIndex = iceCandidate.SdpMLineIndex.HasValue ? iceCandidate.SdpMLineIndex!.Value : (ushort)0,
                usernameFragment = iceCandidate.UsernameFragment,
            };

        public static RTCSessionDescriptionInit FromNative(this SIPSorcery.Net.RTCSessionDescriptionInit nativeSessionDescription) =>
            new RTCSessionDescriptionInit
            {
                Type = nativeSessionDescription.type.FromNative(),
                Sdp = nativeSessionDescription.sdp,
            };

    }
}
