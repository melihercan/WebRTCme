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
            new Webrtc.RTCConfiguration
            {
                BundlePolicy = configuration.BundlePolicy?.ToNative() ?? Webrtc.RTCBundlePolicy.Balanced,
                Certificate = (Webrtc.RTCCertificate)(configuration.Certificates[0]?.NativeObject) ??
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
    }
}
