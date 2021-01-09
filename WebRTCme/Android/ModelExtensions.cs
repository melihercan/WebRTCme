using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WebRTCme;
using Webrtc = Org.Webrtc;

namespace WebRtc.Android
{
    internal static class ModelExtensions
    {
        public static Webrtc.MediaConstraints ToNative(this MediaTrackConstraints constraints)
        {
            var mandatoryDictionary = new Dictionary<string, string>();
            var optionalDictionary = new Dictionary<string, string>();

            mandatoryDictionary.Add("googEchoCancellation", constraints.EchoCancellation.ToBool() ? "true" : "false");
            mandatoryDictionary.Add("googAutoGainControl", constraints.AutoGainControl.ToBool() ? "true" : "false");
            mandatoryDictionary.Add("googHighpassFilter", "false");
            mandatoryDictionary.Add("googNoiseSuppression", constraints.NoiseSuppression.ToBool() ? "true" : "false");

            var mandatory = mandatoryDictionary.Select(
                pair => new Webrtc.MediaConstraints.KeyValuePair(pair.Key, pair.Value)).ToList();
            var optional = optionalDictionary.Select
                (pair => new Webrtc.MediaConstraints.KeyValuePair(pair.Key, pair.Value)).ToList();

            var nativeConstrains = new Webrtc.MediaConstraints
            {
                Mandatory = mandatory,
                Optional = optional
            };

            return nativeConstrains;
        }

        public static Webrtc.PeerConnection.RTCConfiguration ToNative(this RTCConfiguration configuration) =>
            new Webrtc.PeerConnection.RTCConfiguration(configuration.IceServers
                .Select(server => server.ToNative()).ToArray())
            {
                BundlePolicy = configuration.BundlePolicy?.ToNative() ?? Webrtc.PeerConnection.BundlePolicy.Balanced,
                Certificate = (Webrtc.RtcCertificatePem)(configuration.Certificates?.ElementAt(0).NativeObject) ??
                    Webrtc.RtcCertificatePem.GenerateCertificate(),
                IceCandidatePoolSize = configuration.IceCandidatePoolSize ?? 0,
                IceTransportsType = configuration.IceTransportPolicy?.ToNative() ?? 
                    Webrtc.PeerConnection.IceTransportsType.All,
                RtcpMuxPolicy = configuration.RtcpMuxPolicy?.ToNative() ?? Webrtc.PeerConnection.RtcpMuxPolicy.Require 

            };
            
        public static Webrtc.PeerConnection.IceServer ToNative(this RTCIceServer iceServer) =>
            Webrtc.PeerConnection.IceServer.InvokeBuilder(iceServer.Urls)
                .SetTlsCertPolicy(iceServer.CredentialType?.ToNative() ??
                    Webrtc.PeerConnection.TlsCertPolicy.TlsCertPolicySecure)
                ////.SetUsername(iceServer.Username)
                .If(iceServer.Username != null, builder => builder.SetUsername(iceServer.Username))
                ////.SetPassword(iceServer.Credential)
                .If(iceServer.Credential != null, builder => builder.SetPassword(iceServer.Credential))
                .CreateIceServer();


        public static Webrtc.DataChannel.Init ToNative(this RTCDataChannelInit dataChannelInit) =>
            new Webrtc.DataChannel.Init
            {
                Ordered = dataChannelInit.Ordered ?? true,
                ///// TODO: CHECK THIS!!!! MaxRetransmitTimeMs = dataChannelInit.MaxPacketLifeTime ?? 0,
                MaxRetransmits = dataChannelInit.MaxRetransmits ?? 0,
                Protocol = dataChannelInit.Protocol,
                Negotiated = dataChannelInit.Negotiated ?? false,
                Id = dataChannelInit.Id ?? 0
            };

        public static RTCRtpCodecParameters FromNative(this Webrtc.RtpParameters.Codec nativeCodecParameters) =>
            new RTCRtpCodecParameters
            {
                PayloadType = (byte)nativeCodecParameters.PayloadType,
                MimeType = "TODO: FIX ME",
                ClockRate = (ulong)(int)nativeCodecParameters.ClockRate,
                Channels = (ushort)(int)nativeCodecParameters.NumChannels,
                SdpFmtpLine = "TODO: FIX ME"
            };

        public static RTCRtpHeaderExtensionParameters FromNative(this Webrtc.RtpParameters.HeaderExtension
            nativeHeaderExtension) =>
            new RTCRtpHeaderExtensionParameters
            {
                Uri = nativeHeaderExtension.Uri,
                Id = (ushort)nativeHeaderExtension.Id,
                Encrypted = nativeHeaderExtension.Encrypted
            };

        public static RTCRtpReceiveParameters FromNativeToReceive(this Webrtc.RtpParameters nativeRtpParameters) =>
            new RTCRtpReceiveParameters
            {
                Codecs = (nativeRtpParameters.Codecs as List<Webrtc.RtpParameters.Codec>)
                    .Select(nativeCodec => nativeCodec.FromNative()).ToArray(),
                HeaderExtensions = (nativeRtpParameters.HeaderExtensions as List<Webrtc.RtpParameters.HeaderExtension>)
                    .Select(nativeHeaderExtension => nativeHeaderExtension.FromNative()).ToArray(),
                Rtcp = null//// TODO: CHECK THIS
            };

        public static RTCRtpSendParameters FromNativeToSend(this Webrtc.RtpParameters nativeRtpParameters) =>
            new RTCRtpSendParameters
            {
                Codecs = (nativeRtpParameters.Codecs as List<Webrtc.RtpParameters.Codec>)
                    .Select(nativeCodec => nativeCodec.FromNative()).ToArray(),
                HeaderExtensions = (nativeRtpParameters.HeaderExtensions as List<Webrtc.RtpParameters.HeaderExtension>)
                    .Select(headerExtension => headerExtension.FromNative()).ToArray(),
                Rtcp = null,//// TODO: CHECK THIS
                Encodings = (nativeRtpParameters.Encodings as List<Webrtc.RtpParameters.Encoding>)
                    .Select(nativeEncoding => nativeEncoding.FromNative()).ToArray(),
                TransactionId = nativeRtpParameters.TransactionId
            };

        public static RTCRtpEncodingParameters FromNative(this Webrtc.RtpParameters.Encoding nativeEncoding) =>
            new RTCRtpEncodingParameters
            {
                Active = nativeEncoding.Active,
                CodecPayloadType = 0, //// TODO: CHECK THIS
                Dtx = RTCDtxStatus.Enabled, //// TODO: CHECK THIS
                MaxBitrate = (ulong)(int)nativeEncoding.MaxBitrateBps,
                MaxFramerate = (double)(int)nativeEncoding.MaxFramerate,
                Ptime = 0, //// TODO: CHECK THIS
                Rid = nativeEncoding.Rid,
                ScaleResolutionDownBy = (double)nativeEncoding.ScaleResolutionDownBy
            };

    }
}
