using Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WebRTCme;

namespace WebRtc.iOS
{
    internal static class ModelExtensions
    {
        public static Webrtc.RTCConfiguration ToNative(this RTCConfiguration configuration) =>
            new Webrtc.RTCConfiguration()
            {
                BundlePolicy = configuration.BundlePolicy?.ToNative() ?? Webrtc.RTCBundlePolicy.Balanced,
                Certificate = (Webrtc.RTCCertificate)(configuration.Certificates?.ElementAt(0).NativeObject) ??
                    //new Webrtc.RTCCertificate("TODO:private key", "TODO: certiciate"),
                    Webrtc.RTCCertificate.GenerateCertificateWithParams(new NSDictionary<NSString, NSObject>(
                        new[] { new NSString("expires"), new NSString("name") },
                        new NSObject[] { new NSNumber(100000), new NSString("RSASSA-PKCS1-v1_5") })),

                IceCandidatePoolSize = configuration.IceCandidatePoolSize ?? 0,
                IceServers = configuration.IceServers.Select(server => server.ToNative()).ToArray(),
                IceTransportPolicy = configuration.IceTransportPolicy?.ToNative() ?? Webrtc.RTCIceTransportPolicy.All,
                RtcpMuxPolicy = configuration.RtcpMuxPolicy?.ToNative() ?? Webrtc.RTCRtcpMuxPolicy.Require
            };

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
                MaxPacketLifeTime = dataChannelInit.MaxPacketLifeTime ?? 0,
                MaxRetransmits = dataChannelInit.MaxRetransmits ?? 0,
                Protocol = dataChannelInit.Protocol,
                IsNegotiated = dataChannelInit.Negotiated ?? false,
                StreamId = dataChannelInit.Id ?? 0
            };

        public static RTCConfiguration FromNative(this Webrtc.RTCConfiguration nativeConfiguration) =>
            new RTCConfiguration()
            {
                BundlePolicy = nativeConfiguration.BundlePolicy.FromNative(),
                Certificates = new IRTCCertificate[] 
                    { RTCCertificate.Create(nativeConfiguration.Certificate) },
                IceCandidatePoolSize = (byte)nativeConfiguration.IceCandidatePoolSize,
                IceServers = nativeConfiguration.IceServers.Select(nativeServer => nativeServer.FromNative()).ToArray(),
                IceTransportPolicy = nativeConfiguration.IceTransportPolicy.FromNative(),
                RtcpMuxPolicy = nativeConfiguration.RtcpMuxPolicy.FromNative()
            };

        public static RTCIceServer FromNative(this Webrtc.RTCIceServer nativeIceServer) =>
            new RTCIceServer
            {
                Credential = nativeIceServer.Credential,
                CredentialType = nativeIceServer.TlsCertPolicy.FromNative(),
                Urls = nativeIceServer.UrlStrings,
                Username = nativeIceServer.Username
            };

    }
}
