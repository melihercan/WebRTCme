using System;
using System.Collections.Generic;
using System.Text;
using WebRTCme;

namespace WebRtc.iOS
{
    internal static class EnumExtensions
    {
        public static Webrtc.RTCBundlePolicy ToNative(this RTCBundlePolicy bundlePolicy) =>
            bundlePolicy switch
            {
                RTCBundlePolicy.Balanced => Webrtc.RTCBundlePolicy.Balanced,
                RTCBundlePolicy.MaxCompat => Webrtc.RTCBundlePolicy.MaxCompat,
                RTCBundlePolicy.MaxBundle => Webrtc.RTCBundlePolicy.MaxBundle
            };

        public static Webrtc.RTCTlsCertPolicy ToNative(this RTCIceCredentialType iceCredentialType) =>
            iceCredentialType switch
            {
                RTCIceCredentialType.Password => Webrtc.RTCTlsCertPolicy.Secure,
                RTCIceCredentialType.Oauth => Webrtc.RTCTlsCertPolicy.Secure
            };

        public static Webrtc.RTCIceTransportPolicy ToNative(this RTCIceTransportPolicy iceTransportPolicy) =>
            iceTransportPolicy switch
            {
                RTCIceTransportPolicy.Relay => Webrtc.RTCIceTransportPolicy.Relay,
                RTCIceTransportPolicy.All => Webrtc.RTCIceTransportPolicy.All
            };

        public static Webrtc.RTCRtcpMuxPolicy ToNative(this RTCRtcpMuxPolicy rtcpMuxPolicy) =>
            rtcpMuxPolicy switch
            {
                RTCRtcpMuxPolicy.Negotiate => Webrtc.RTCRtcpMuxPolicy.Negotiate,
                RTCRtcpMuxPolicy.Require => Webrtc.RTCRtcpMuxPolicy.Require
            };
    }
}
