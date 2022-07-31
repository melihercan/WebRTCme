using Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WebRTCme;

namespace WebRTCme.iOS
{
    internal static class ModelExtensions
    {
        public static Webrtc.RTCConfiguration ToNative(this RTCConfiguration configuration)
        {
            RTCIceServer[] iceServers = configuration.IceServers ?? new RTCIceServer[] { };
            var nativeConfiguration = new Webrtc.RTCConfiguration();
            if (configuration.BundlePolicy.HasValue)
                nativeConfiguration.BundlePolicy = ((RTCBundlePolicy)configuration.BundlePolicy).ToNative();
            if (configuration.Certificates is not null)
                nativeConfiguration.Certificate = (Webrtc.RTCCertificate)
                    ((RTCCertificate)configuration.Certificates?.ElementAt(0)).NativeObject;
            if (configuration.IceCandidatePoolSize.HasValue)
                nativeConfiguration.IceCandidatePoolSize = (int)configuration.IceCandidatePoolSize;
            nativeConfiguration.IceServers = iceServers.Select(server => server.ToNative()).ToArray();
            if (configuration.IceTransportPolicy.HasValue)
                nativeConfiguration.IceTransportPolicy = ((RTCIceTransportPolicy)configuration.IceTransportPolicy).ToNative();
            if (configuration.RtcpMuxPolicy.HasValue)
                nativeConfiguration.RtcpMuxPolicy = ((RTCRtcpMuxPolicy)configuration.RtcpMuxPolicy).ToNative();
            if (configuration.SdpSemantics.HasValue)
                nativeConfiguration.SdpSemantics = ((SdpSemantics)configuration.SdpSemantics).ToNative();
            return nativeConfiguration;
        }

        public static Webrtc.RTCIceServer ToNative(this RTCIceServer iceServer) =>
            new Webrtc.RTCIceServer
            (
                urlStrings: iceServer.Urls,
                username: iceServer.Username,
                credential: iceServer.Credential,
                tlsCertPolicy: iceServer.CredentialType?.ToNative() ?? Webrtc.RTCTlsCertPolicy.Secure
            );


        public static Webrtc.RTCDataChannelConfiguration ToNative(this RTCDataChannelInit dataChannelInit) =>
            new Webrtc.RTCDataChannelConfiguration
            {
                IsOrdered = dataChannelInit.Ordered ?? true,
                MaxRetransmits = dataChannelInit.MaxRetransmits ?? -1,
                Protocol = dataChannelInit.Protocol ?? string.Empty,
                IsNegotiated = dataChannelInit.Negotiated ?? false,
                ChannelId = dataChannelInit.Id ?? 1//WebRTCme.WebRtc.Id
            };

        public static Webrtc.RTCSessionDescription ToNative(this RTCSessionDescriptionInit description) =>
            new Webrtc.RTCSessionDescription(description.Type.ToNative(), description.Sdp);

        public static RTCConfiguration FromNative(this Webrtc.RTCConfiguration nativeConfiguration) =>
            new RTCConfiguration
            {
                BundlePolicy = nativeConfiguration.BundlePolicy.FromNative(),
                Certificates = new IRTCCertificate[]
                    { new RTCCertificate(nativeConfiguration.Certificate) },
                IceCandidatePoolSize = (byte)nativeConfiguration.IceCandidatePoolSize,
                IceServers = nativeConfiguration.IceServers.Select(nativeServer => nativeServer.FromNative()).ToArray(),
                IceTransportPolicy = nativeConfiguration.IceTransportPolicy.FromNative(),
                RtcpMuxPolicy = nativeConfiguration.RtcpMuxPolicy.FromNative()
            };

        public static Webrtc.RTCIceCandidate ToNative(this RTCIceCandidateInit iceCandidateInit) =>
            new Webrtc.RTCIceCandidate(iceCandidateInit.Candidate, (int)iceCandidateInit.SdpMLineIndex, 
                iceCandidateInit.SdpMid)
            {
                ///ServerUrl = ???
            };

        public static Webrtc.RTCRtpEncodingParameters ToNative(this RTCRtpEncodingParameters parameters) =>
            new Webrtc.RTCRtpEncodingParameters
            {
                Rid = parameters.Rid,
                IsActive = parameters.Active,
                MaxBitrateBps = parameters.MaxBitrate,
                MaxFramerate = parameters.MaxFramerate,
                ScaleResolutionDownBy = parameters.ScaleResolutionDownBy,
            };

        public static Webrtc.RTCRtpTransceiverInit ToNative(this RTCRtpTransceiverInit init)
        {
            RTCRtpEncodingParameters[] initSendEncodings = init.SendEncodings ?? new RTCRtpEncodingParameters[] { };
            var direction = init.Direction is null ? Webrtc.RTCRtpTransceiverDirection.Inactive : 
                ((RTCRtpTransceiverDirection)init.Direction).ToNative();
            var streamIds = init.Streams is null ? null : init.Streams.Select(stream => stream.Id).ToArray();
            var sendEncodings = initSendEncodings.Select(encodings => encodings.ToNative()).ToArray();
            return new Webrtc.RTCRtpTransceiverInit
            {
                Direction = direction,
                StreamIds = streamIds,
                SendEncodings = sendEncodings
            };
        }

        public static RTCIceServer FromNative(this Webrtc.RTCIceServer nativeIceServer) =>
            new RTCIceServer
            {
                Credential = nativeIceServer.Credential,
                CredentialType = nativeIceServer.TlsCertPolicy.FromNative(),
                Urls = nativeIceServer.UrlStrings,
                Username = nativeIceServer.Username
            };

        public static RTCRtpReceiveParameters FromNativeToReceive(this Webrtc.RTCRtpParameters nativeRtpParameters) =>
            new RTCRtpReceiveParameters
            {
                Codecs = nativeRtpParameters.Codecs.Select(nativeCodec => nativeCodec.FromNative()).ToArray(),
                HeaderExtensions = nativeRtpParameters.HeaderExtensions
                    .Select(nativeHeaderExtension => nativeHeaderExtension.FromNative()).ToArray(),
                Rtcp = null//// TODO: CHECK THIS
            };

        public static RTCRtpSendParameters FromNativeToSend(this Webrtc.RTCRtpParameters nativeRtpParameters) =>
            new RTCRtpSendParameters
            {
                Codecs = nativeRtpParameters.Codecs.Select(nativeCodec => nativeCodec.FromNative()).ToArray(),
                HeaderExtensions = nativeRtpParameters.HeaderExtensions
                    .Select(nativeHeaderExtension => nativeHeaderExtension.FromNative()).ToArray(),
                Rtcp = null,//// TODO: CHECK THIS
                Encodings = nativeRtpParameters.Encodings.Select(nativeEncoding => nativeEncoding.FromNative())
                    .ToArray(),
                TransactionId = nativeRtpParameters.TransactionId
            };

        public static RTCRtpCodecParameters FromNative(this Webrtc.RTCRtpCodecParameters nativeRtpCodecParameters) =>
            new RTCRtpCodecParameters
            {
                PayloadType = (byte)nativeRtpCodecParameters.PayloadType,
                MimeType = string.Empty, //// TODO: FIX THIS
                ClockRate = nativeRtpCodecParameters.ClockRate.UInt64Value,
                Channels = nativeRtpCodecParameters.NumChannels.UInt16Value,
                SdpFmtpLine = string.Empty //// TODO: FIX THIS
            };

        public static RTCRtpHeaderExtensionParameters FromNative(this Webrtc.RTCRtpHeaderExtension
            nativeRtpHeaderExtension) =>
            new RTCRtpHeaderExtensionParameters
            {
                Uri = nativeRtpHeaderExtension.Uri,
                Id = (ushort)nativeRtpHeaderExtension.Id,
                Encrypted = nativeRtpHeaderExtension.Encrypted
            };

        public static RTCRtpEncodingParameters FromNative(this Webrtc.RTCRtpEncodingParameters nativeEncoding) =>
            new RTCRtpEncodingParameters
            {
                Active = nativeEncoding.IsActive,
                ////CodecPayloadType = 0, //// TODO: CHECK THIS
                ////Dtx = RTCDtxStatus.Enabled, //// TODO: CHECK THIS
                MaxBitrate = nativeEncoding.MaxBitrateBps.UInt64Value,
                MaxFramerate = nativeEncoding.MaxFramerate.DoubleValue,
                ////Ptime = 0, //// TODO: CHECK THIS
                Rid = nativeEncoding.Rid,
                ScaleResolutionDownBy = nativeEncoding.ScaleResolutionDownBy.DoubleValue
            };

        public static RTCSessionDescriptionInit FromNative(this Webrtc.RTCSessionDescription nativeDescription) =>
            new RTCSessionDescriptionInit
            {
                Type = nativeDescription.Type.FromNative(),
                Sdp = nativeDescription.Sdp
            };

        public static RTCIceCandidateInit FromNative(this Webrtc.RTCIceCandidate nativeIceCandidate) =>
            new RTCIceCandidateInit
            {
                Candidate = nativeIceCandidate.Sdp,
                SdpMid = nativeIceCandidate.SdpMid,
                SdpMLineIndex = (ushort)nativeIceCandidate.SdpMLineIndex,
                //UsernameFragment = ???
            };

    }

}
