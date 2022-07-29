using AVFoundation;
using System;
using System.Collections.Generic;
using System.Text;
using WebRTCme;

namespace WebRTCme.iOS
{
    internal static class EnumExtensions
    {
        public static Webrtc.RTCBundlePolicy ToNative(this RTCBundlePolicy bundlePolicy) =>
            bundlePolicy switch
            {
                RTCBundlePolicy.Balanced => Webrtc.RTCBundlePolicy.Balanced,
                RTCBundlePolicy.MaxCompat => Webrtc.RTCBundlePolicy.MaxCompat,
                RTCBundlePolicy.MaxBundle => Webrtc.RTCBundlePolicy.MaxBundle,
                _ => throw new NotImplementedException()
            };

        public static Webrtc.RTCTlsCertPolicy ToNative(this RTCIceCredentialType iceCredentialType) =>
            iceCredentialType switch
            {
                RTCIceCredentialType.Password => Webrtc.RTCTlsCertPolicy.Secure,
                RTCIceCredentialType.Oauth => Webrtc.RTCTlsCertPolicy.Secure,
                _ => throw new NotImplementedException()
            };

        public static Webrtc.RTCIceTransportPolicy ToNative(this RTCIceTransportPolicy iceTransportPolicy) =>
            iceTransportPolicy switch
            {
                RTCIceTransportPolicy.Relay => Webrtc.RTCIceTransportPolicy.Relay,
                RTCIceTransportPolicy.All => Webrtc.RTCIceTransportPolicy.All,
                _ => throw new NotImplementedException()
            };

        public static Webrtc.RTCRtcpMuxPolicy ToNative(this RTCRtcpMuxPolicy rtcpMuxPolicy) =>
            rtcpMuxPolicy switch
            {
                RTCRtcpMuxPolicy.Negotiate => Webrtc.RTCRtcpMuxPolicy.Negotiate,
                RTCRtcpMuxPolicy.Require => Webrtc.RTCRtcpMuxPolicy.Require,
                _ => throw new NotImplementedException()
            };

        public static Webrtc.RTCRtpTransceiverDirection ToNative(this RTCRtpTransceiverDirection direction) =>
            direction switch
            {
                RTCRtpTransceiverDirection.SendRecv => Webrtc.RTCRtpTransceiverDirection.SendRecv,
                RTCRtpTransceiverDirection.SendOnly => Webrtc.RTCRtpTransceiverDirection.SendOnly,
                RTCRtpTransceiverDirection.RecvOnly => Webrtc.RTCRtpTransceiverDirection.RecvOnly,
                RTCRtpTransceiverDirection.Inactive => Webrtc.RTCRtpTransceiverDirection.Inactive,
                _ => throw new NotImplementedException()
            };

        public static Webrtc.RTCSdpType ToNative(this RTCSdpType type) =>
            type switch
            {
                RTCSdpType.Answer => Webrtc.RTCSdpType.Answer,
                RTCSdpType.Offer => Webrtc.RTCSdpType.Offer,
                RTCSdpType.Pranswer => Webrtc.RTCSdpType.PrAnswer,
                RTCSdpType.Rollback => Webrtc.RTCSdpType.Rollback,
                _ => throw new NotImplementedException()
            };

        public static Webrtc.RTCRtpMediaType ToNative(this MediaStreamTrackKind kind) =>
            kind switch
            {
                MediaStreamTrackKind.Audio => Webrtc.RTCRtpMediaType.Audio,
                MediaStreamTrackKind.Video => Webrtc.RTCRtpMediaType.Video,
                _ => throw new NotImplementedException()
            };

        public static Webrtc.RTCSdpSemantics ToNative(this SdpSemantics sdpSemantics) =>
            sdpSemantics switch
            {
                SdpSemantics.PlanB => Webrtc.RTCSdpSemantics.PlanB,
                SdpSemantics.UnifiedPlan => Webrtc.RTCSdpSemantics.UnifiedPlan,
                _ => throw new NotImplementedException()
            };


        public static RTCBundlePolicy FromNative(this Webrtc.RTCBundlePolicy nativeBundlePolicy) =>
            nativeBundlePolicy switch
            {
                Webrtc.RTCBundlePolicy.Balanced => RTCBundlePolicy.Balanced,
                Webrtc.RTCBundlePolicy.MaxCompat => RTCBundlePolicy.MaxCompat,
                Webrtc.RTCBundlePolicy.MaxBundle => RTCBundlePolicy.MaxBundle,
                _ => throw new NotImplementedException()
            };

        public static RTCIceTransportPolicy FromNative(this Webrtc.RTCIceTransportPolicy nativeIceTransportPolicy) =>
            nativeIceTransportPolicy switch
            {
                Webrtc.RTCIceTransportPolicy.Relay => RTCIceTransportPolicy.Relay,
                Webrtc.RTCIceTransportPolicy.All => RTCIceTransportPolicy.All,
                _ => throw new NotImplementedException()
            };

        public static RTCPeerConnectionState FromNative(this Webrtc.RTCPeerConnectionState nativePeerConnectionState) =>
            nativePeerConnectionState switch
            {
                Webrtc.RTCPeerConnectionState.New => RTCPeerConnectionState.New,
                Webrtc.RTCPeerConnectionState.Connecting => RTCPeerConnectionState.Connecting,
                Webrtc.RTCPeerConnectionState.Connected => RTCPeerConnectionState.Connected,
                Webrtc.RTCPeerConnectionState.Disconnected => RTCPeerConnectionState.Disconnected,
                Webrtc.RTCPeerConnectionState.Failed => RTCPeerConnectionState.Failed,
                Webrtc.RTCPeerConnectionState.Closed => RTCPeerConnectionState.Closed,
                _ => throw new NotImplementedException()
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
                Webrtc.RTCIceConnectionState.Count => throw new NotSupportedException(),
                _ => throw new NotImplementedException()
            };

        public static RTCIceGatheringState FromNative(this Webrtc.RTCIceGatheringState nativeIceGatheringState) =>
            nativeIceGatheringState switch
            {
                Webrtc.RTCIceGatheringState.New => RTCIceGatheringState.New, 
                Webrtc.RTCIceGatheringState.Gathering => RTCIceGatheringState.Gathering,
                Webrtc.RTCIceGatheringState.Complete => RTCIceGatheringState.Complete,
                _ => throw new NotImplementedException()
            };

        public static RTCSignalingState FromNative(this Webrtc.RTCSignalingState nativeSignalingState) =>
            nativeSignalingState switch
            {
                Webrtc.RTCSignalingState.Stable => RTCSignalingState.Stable,
                Webrtc.RTCSignalingState.HaveLocalOffer => RTCSignalingState.HaveLocalOffer,
                Webrtc.RTCSignalingState.HaveLocalPrAnswer => RTCSignalingState.HaveLocalPranswer,
                Webrtc.RTCSignalingState.HaveRemoteOffer => RTCSignalingState.HaveRemoteOffer,
                Webrtc.RTCSignalingState.HaveRemotePrAnswer => RTCSignalingState.HaveRemotePranswer,
                Webrtc.RTCSignalingState.Closed => RTCSignalingState.Closed,
                _ => throw new NotImplementedException()
            };

        public static RTCIceCredentialType FromNative(this Webrtc.RTCTlsCertPolicy nativeTlsCertPolicy) =>
            nativeTlsCertPolicy switch
            {
                Webrtc.RTCTlsCertPolicy.InsecureNoCheck => throw new NotImplementedException(),
                Webrtc.RTCTlsCertPolicy.Secure => RTCIceCredentialType.Password,
                _ => throw new NotImplementedException()
            };

        public static RTCRtcpMuxPolicy FromNative(this Webrtc.RTCRtcpMuxPolicy nativeRtcpMuxPolicy) =>
            nativeRtcpMuxPolicy switch
            {
                Webrtc.RTCRtcpMuxPolicy.Negotiate => RTCRtcpMuxPolicy.Negotiate,
                Webrtc.RTCRtcpMuxPolicy.Require => RTCRtcpMuxPolicy.Require,
                _ => throw new NotImplementedException()
            };

        public static RTCDataChannelState FromNative(this Webrtc.RTCDataChannelState nativeDataChannelState) =>
            nativeDataChannelState switch
            {
                Webrtc.RTCDataChannelState.Connecting => RTCDataChannelState.Connecting,
                Webrtc.RTCDataChannelState.Open => RTCDataChannelState.Open,
                Webrtc.RTCDataChannelState.Closing => RTCDataChannelState.Closing,
                Webrtc.RTCDataChannelState.Closed => RTCDataChannelState.Closed,
                _ => throw new NotImplementedException()
            };

        public static RTCRtpTransceiverDirection FromNative(this Webrtc.RTCRtpTransceiverDirection nativeDirection) =>
            nativeDirection switch
            {
                Webrtc.RTCRtpTransceiverDirection.SendRecv => RTCRtpTransceiverDirection.SendRecv,
                Webrtc.RTCRtpTransceiverDirection.SendOnly => RTCRtpTransceiverDirection.SendOnly,
                Webrtc.RTCRtpTransceiverDirection.RecvOnly => RTCRtpTransceiverDirection.RecvOnly,
                Webrtc.RTCRtpTransceiverDirection.Inactive => RTCRtpTransceiverDirection.Inactive,
                Webrtc.RTCRtpTransceiverDirection.Stopped => throw new NotImplementedException(),
                _ => throw new NotImplementedException()
            };

        public static RTCSdpType FromNative(this Webrtc.RTCSdpType nativeType) =>
            nativeType switch
            {
                Webrtc.RTCSdpType.Answer => RTCSdpType.Answer,
                Webrtc.RTCSdpType.Offer => RTCSdpType.Offer,
                Webrtc.RTCSdpType.PrAnswer => RTCSdpType.Pranswer,
                Webrtc.RTCSdpType.Rollback => RTCSdpType.Rollback,
                _ => throw new NotImplementedException()
            };

        public static MediaStreamTrackState FromNative(this Webrtc.RTCMediaStreamTrackState nativeState) =>
            nativeState switch
            {
                Webrtc.RTCMediaStreamTrackState.Live => MediaStreamTrackState.Live,
                Webrtc.RTCMediaStreamTrackState.Ended => MediaStreamTrackState.Ended,
                _ => throw new NotImplementedException()
            };
    }
}
