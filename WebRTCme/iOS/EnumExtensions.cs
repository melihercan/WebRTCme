﻿using System;
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

        public static RTCPeerConnectionState FromNative(this Webrtc.RTCPeerConnectionState nativePeerConnectionState) =>
            nativePeerConnectionState switch
            {
                Webrtc.RTCPeerConnectionState.New => RTCPeerConnectionState.New,
                Webrtc.RTCPeerConnectionState.Connecting => RTCPeerConnectionState.Connecting,
                Webrtc.RTCPeerConnectionState.Connected => RTCPeerConnectionState.Connected,
                Webrtc.RTCPeerConnectionState.Disconnected => RTCPeerConnectionState.Disconnected,
                Webrtc.RTCPeerConnectionState.Failed => RTCPeerConnectionState.Failed,
                Webrtc.RTCPeerConnectionState.Closed => RTCPeerConnectionState.Closed
            };

        public static RTCIceConnectionState FromNative(this Webrtc.RTCIceConnectionState nativeIceConnectionState) =>
            nativeIceConnectionState switch
            {
                Webrtc.RTCIceConnectionState.New => RTCIceConnectionState.New,
                Webrtc.RTCIceConnectionState.Checking => RTCIceConnectionState.Checking,
                Webrtc.RTCIceConnectionState.Connected => RTCIceConnectionState.Connected,
                Webrtc.RTCIceConnectionState.Completed => RTCIceConnectionState.Completed,
                Webrtc.RTCIceConnectionState.Failed => RTCIceConnectionState.Failed,
                Webrtc.RTCIceConnectionState.Disconnected => RTCIceConnectionState.Disconnected,
                Webrtc.RTCIceConnectionState.Closed => RTCIceConnectionState.Closed,
                Webrtc.RTCIceConnectionState.Count => throw new NotSupportedException()
            };

        public static RTCIceGatheringState FromNative(this Webrtc.RTCIceGatheringState nativeIceGatheringState) =>
            nativeIceGatheringState switch
            {
                Webrtc.RTCIceGatheringState.New => RTCIceGatheringState.New, 
                Webrtc.RTCIceGatheringState.Gathering => RTCIceGatheringState.Gathering,
                Webrtc.RTCIceGatheringState.Complete => RTCIceGatheringState.Complete
            };

        public static RTCSignalingState FromNative(this Webrtc.RTCSignalingState nativeSignalingState) =>
            nativeSignalingState switch
            {
                Webrtc.RTCSignalingState.Stable => RTCSignalingState.Stable,
                Webrtc.RTCSignalingState.HaveLocalOffer => RTCSignalingState.HaveLocalOffer,
                Webrtc.RTCSignalingState.HaveLocalPrAnswer => RTCSignalingState.HaveLocalPranswer,
                Webrtc.RTCSignalingState.HaveRemoteOffer => RTCSignalingState.HaveRemoteOffer,
                Webrtc.RTCSignalingState.HaveRemotePrAnswer => RTCSignalingState.HaveRemotePranswer,
                Webrtc.RTCSignalingState.Closed => throw new NotSupportedException()
            };

        public static RTCIceCredentialType FromNative(this Webrtc.RTCTlsCertPolicy nativeTlsCertPolicy) =>
            nativeTlsCertPolicy switch
            {
                Webrtc.RTCTlsCertPolicy.InsecureNoCheck => throw new NotImplementedException(),
                Webrtc.RTCTlsCertPolicy.Secure => RTCIceCredentialType.Password
            };

    }
}
