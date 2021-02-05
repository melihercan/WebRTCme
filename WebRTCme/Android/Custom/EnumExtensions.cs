﻿using System;
using System.Collections.Generic;
using System.Text;
using WebRTCme;
using Webrtc = Org.Webrtc;

namespace WebRtc.Android
{
    internal static class EnumExtensions
    {
        public static Webrtc.PeerConnection.BundlePolicy ToNative(this RTCBundlePolicy bundlePolicy) =>
            bundlePolicy switch
            {
                RTCBundlePolicy.Balanced => Webrtc.PeerConnection.BundlePolicy.Balanced,
                RTCBundlePolicy.MaxCompat => Webrtc.PeerConnection.BundlePolicy.Maxcompat,
                RTCBundlePolicy.MaxBundle => Webrtc.PeerConnection.BundlePolicy.Maxbundle,
                _ => throw new NotImplementedException()
            };


        public static Webrtc.PeerConnection.TlsCertPolicy ToNative(this RTCIceCredentialType iceCredentialType) =>
            iceCredentialType switch
            {
                RTCIceCredentialType.Password => Webrtc.PeerConnection.TlsCertPolicy.TlsCertPolicySecure,
                RTCIceCredentialType.Oauth => Webrtc.PeerConnection.TlsCertPolicy.TlsCertPolicySecure,
                _ => throw new NotImplementedException()
            };

        public static Webrtc.PeerConnection.IceTransportsType ToNative(this RTCIceTransportPolicy iceTransportPolicy) =>
            iceTransportPolicy switch
            {
                RTCIceTransportPolicy.Relay => Webrtc.PeerConnection.IceTransportsType.Relay,
                RTCIceTransportPolicy.All => Webrtc.PeerConnection.IceTransportsType.All,
                _ => throw new NotImplementedException()
            };

        public static Webrtc.PeerConnection.RtcpMuxPolicy ToNative(this RTCRtcpMuxPolicy rtcpMuxPolicy) =>
            rtcpMuxPolicy switch
            {
                RTCRtcpMuxPolicy.Negotiate => Webrtc.PeerConnection.RtcpMuxPolicy.Negotiate,
                RTCRtcpMuxPolicy.Require => Webrtc.PeerConnection.RtcpMuxPolicy.Require,
                _ => throw new NotImplementedException()
            };

        public static Webrtc.RtpTransceiver.RtpTransceiverDirection ToNative(
            this RTCRtpTransceiverDirection transceiverDirection) =>
                transceiverDirection switch
                {
                    RTCRtpTransceiverDirection.Sendrecv => Webrtc.RtpTransceiver.RtpTransceiverDirection.SendRecv,
                    RTCRtpTransceiverDirection.Sendonly => Webrtc.RtpTransceiver.RtpTransceiverDirection.SendOnly,
                    RTCRtpTransceiverDirection.Recvonly => Webrtc.RtpTransceiver.RtpTransceiverDirection.RecvOnly,
                    RTCRtpTransceiverDirection.Inactive => Webrtc.RtpTransceiver.RtpTransceiverDirection.Inactive,
                    _ => throw new NotImplementedException()
                };

        public static Webrtc.SessionDescription.Type ToNative(this RTCSdpType type) =>
            type switch
            {
                RTCSdpType.Answer => Webrtc.SessionDescription.Type.Answer,
                RTCSdpType.Offer => Webrtc.SessionDescription.Type.Offer,
                RTCSdpType.Pranswer => Webrtc.SessionDescription.Type.Pranswer,
                RTCSdpType.Rollback => Webrtc.SessionDescription.Type.Rollback,
                _ => throw new NotImplementedException()
            };

        public static RTCPeerConnectionState FromNative(
            this Webrtc.PeerConnection.PeerConnectionState nativePeerConnectionState)
        {
            if (nativePeerConnectionState == Webrtc.PeerConnection.PeerConnectionState.New)
                return RTCPeerConnectionState.New;
            if (nativePeerConnectionState == Webrtc.PeerConnection.PeerConnectionState.Connecting)
                return RTCPeerConnectionState.Connecting;
            if (nativePeerConnectionState == Webrtc.PeerConnection.PeerConnectionState.Connected)
                return RTCPeerConnectionState.Connected;
            if (nativePeerConnectionState == Webrtc.PeerConnection.PeerConnectionState.Disconnected)
                return RTCPeerConnectionState.Disconnected;
            if (nativePeerConnectionState == Webrtc.PeerConnection.PeerConnectionState.Failed)
                return RTCPeerConnectionState.Failed;
            if (nativePeerConnectionState == Webrtc.PeerConnection.PeerConnectionState.Closed)
                return RTCPeerConnectionState.Closed;
            throw new ArgumentOutOfRangeException(nameof(nativePeerConnectionState), nativePeerConnectionState, null);
        }

        public static RTCIceConnectionState FromNative(
            this Webrtc.PeerConnection.IceConnectionState nativeIceConnectionState)
        {
            if (nativeIceConnectionState == Webrtc.PeerConnection.IceConnectionState.New)
                return RTCIceConnectionState.New;
            if (nativeIceConnectionState == Webrtc.PeerConnection.IceConnectionState.Checking)
                return RTCIceConnectionState.Checking;
            if (nativeIceConnectionState == Webrtc.PeerConnection.IceConnectionState.Connected) 
                return RTCIceConnectionState.Connected;
            if (nativeIceConnectionState == Webrtc.PeerConnection.IceConnectionState.Completed) 
                return RTCIceConnectionState.Completed;
            if (nativeIceConnectionState == Webrtc.PeerConnection.IceConnectionState.Failed) 
                return RTCIceConnectionState.Failed;
            if (nativeIceConnectionState == Webrtc.PeerConnection.IceConnectionState.Disconnected) 
                return RTCIceConnectionState.Disconnected;
            if (nativeIceConnectionState == Webrtc.PeerConnection.IceConnectionState.Closed) 
                return RTCIceConnectionState.Closed;
            throw new ArgumentOutOfRangeException(nameof(nativeIceConnectionState), nativeIceConnectionState, null);
        }

        public static RTCIceGatheringState FromNative(
            this Webrtc.PeerConnection.IceGatheringState nativeIceGatheringState)
        {
            if (nativeIceGatheringState == Webrtc.PeerConnection.IceGatheringState.New)
                return RTCIceGatheringState.New;
            if (nativeIceGatheringState == Webrtc.PeerConnection.IceGatheringState.Gathering)
                return RTCIceGatheringState.Gathering;
            if (nativeIceGatheringState == Webrtc.PeerConnection.IceGatheringState.Complete)
                return RTCIceGatheringState.Complete;
            throw new ArgumentOutOfRangeException(nameof(nativeIceGatheringState), nativeIceGatheringState, null);
        }

        public static RTCSignalingState FromNative(this Webrtc.PeerConnection.SignalingState nativeSignalingState)
        {
            if (nativeSignalingState == Webrtc.PeerConnection.SignalingState.Stable)
                return RTCSignalingState.Stable;
            if (nativeSignalingState == Webrtc.PeerConnection.SignalingState.HaveLocalOffer)
                return RTCSignalingState.HaveLocalOffer;
            if (nativeSignalingState == Webrtc.PeerConnection.SignalingState.HaveLocalPranswer)
                return RTCSignalingState.HaveLocalPranswer;
            if (nativeSignalingState == Webrtc.PeerConnection.SignalingState.HaveRemoteOffer)
                return RTCSignalingState.HaveRemoteOffer;
            if (nativeSignalingState == Webrtc.PeerConnection.SignalingState.HaveRemotePranswer)
                return RTCSignalingState.HaveRemotePranswer;
            if (nativeSignalingState == Webrtc.PeerConnection.SignalingState.Closed)
                throw new NotSupportedException();
            throw new ArgumentOutOfRangeException(nameof(nativeSignalingState), nativeSignalingState, null);
        }

        public static RTCRtpTransceiverDirection FromNative(
            this Webrtc.RtpTransceiver.RtpTransceiverDirection nativeTransceiverDirection)
        {
            if (nativeTransceiverDirection == Webrtc.RtpTransceiver.RtpTransceiverDirection.SendRecv)
                return RTCRtpTransceiverDirection.Sendrecv;
            if (nativeTransceiverDirection == Webrtc.RtpTransceiver.RtpTransceiverDirection.SendOnly)
                return RTCRtpTransceiverDirection.Sendonly;
            if (nativeTransceiverDirection == Webrtc.RtpTransceiver.RtpTransceiverDirection.RecvOnly)
                return RTCRtpTransceiverDirection.Recvonly;
            if (nativeTransceiverDirection == Webrtc.RtpTransceiver.RtpTransceiverDirection.Inactive)
                return RTCRtpTransceiverDirection.Inactive;
            throw new ArgumentOutOfRangeException(nameof(nativeTransceiverDirection), nativeTransceiverDirection, null);
        }

        public static RTCSdpType FromNative(this Webrtc.SessionDescription.Type nativeType)
        {
            if (nativeType == Webrtc.SessionDescription.Type.Answer)
                return RTCSdpType.Answer;
            if (nativeType == Webrtc.SessionDescription.Type.Offer)
                return RTCSdpType.Offer;
            if (nativeType == Webrtc.SessionDescription.Type.Pranswer)
                return RTCSdpType.Pranswer;
            if (nativeType == Webrtc.SessionDescription.Type.Rollback)
                return RTCSdpType.Rollback;
            throw new ArgumentOutOfRangeException(nameof(nativeType), nativeType, null);
        }
    }
}
