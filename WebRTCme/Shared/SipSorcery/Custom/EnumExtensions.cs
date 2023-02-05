using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebRTCme.Shared.SipSorcery.Custom
{
    public static class EnumExtensions
    {
        public static SIPSorcery.Net.RTCIceCredentialType ToNative(this RTCIceCredentialType iceCredentialType) =>
            iceCredentialType switch
            {
                RTCIceCredentialType.Password => SIPSorcery.Net.RTCIceCredentialType.password,
                _ => throw new NotImplementedException()
            };

        public static SIPSorcery.Net.RTCIceTransportPolicy ToNative(this RTCIceTransportPolicy iceTransportPolicy) =>
            iceTransportPolicy switch
            {
                RTCIceTransportPolicy.Relay => SIPSorcery.Net.RTCIceTransportPolicy.relay,
                RTCIceTransportPolicy.All => SIPSorcery.Net.RTCIceTransportPolicy.all,
                _ => throw new NotImplementedException(),
            };

        public static SIPSorcery.Net.RTCBundlePolicy ToNative(this RTCBundlePolicy bundlePolicy) =>
            bundlePolicy switch
            {
                RTCBundlePolicy.Balanced => SIPSorcery.Net.RTCBundlePolicy.balanced,
                RTCBundlePolicy.MaxCompat => SIPSorcery.Net.RTCBundlePolicy.max_compat,
                RTCBundlePolicy.MaxBundle => SIPSorcery.Net.RTCBundlePolicy.max_bundle,
                _ => throw new NotImplementedException()
            };
        public static SIPSorcery.Net.RTCRtcpMuxPolicy ToNative(this RTCRtcpMuxPolicy rtcpMuxPolicy) =>
            rtcpMuxPolicy switch
            {
                RTCRtcpMuxPolicy.Require => SIPSorcery.Net.RTCRtcpMuxPolicy.require,
                _ => throw new NotImplementedException()
            };

        public static SIPSorcery.Net.RTCSdpType ToNative(this RTCSdpType sdpType) =>
            sdpType switch
            {
                RTCSdpType.Answer => SIPSorcery.Net.RTCSdpType.answer,
                RTCSdpType.Offer => SIPSorcery.Net.RTCSdpType.offer,
                RTCSdpType.Pranswer => SIPSorcery.Net.RTCSdpType.pranswer,
                RTCSdpType.Rollback => SIPSorcery.Net.RTCSdpType.rollback,
                _ => throw new NotImplementedException(),
            };

        public static RTCSdpType FromNative(this SIPSorcery.Net.RTCSdpType nativeSdpType) =>
            nativeSdpType switch
            {
                SIPSorcery.Net.RTCSdpType.answer => RTCSdpType.Answer,
                SIPSorcery.Net.RTCSdpType.offer => RTCSdpType.Offer,
                SIPSorcery.Net.RTCSdpType.pranswer => RTCSdpType.Pranswer,
                SIPSorcery.Net.RTCSdpType.rollback => RTCSdpType.Rollback,
                _ => throw new NotImplementedException(),
            };

        public static RTCPeerConnectionState FromNative(this SIPSorcery.Net.RTCPeerConnectionState nativeConnectionState) =>
            nativeConnectionState switch
            {
                SIPSorcery.Net.RTCPeerConnectionState.closed => RTCPeerConnectionState.Closed,
                SIPSorcery.Net.RTCPeerConnectionState.failed => RTCPeerConnectionState.Failed,
                SIPSorcery.Net.RTCPeerConnectionState.disconnected => RTCPeerConnectionState.Disconnected,
                SIPSorcery.Net.RTCPeerConnectionState.@new => RTCPeerConnectionState.New,
                SIPSorcery.Net.RTCPeerConnectionState.connecting => RTCPeerConnectionState.Connecting,
                SIPSorcery.Net.RTCPeerConnectionState.connected => RTCPeerConnectionState.Connected,
                _ => throw new NotImplementedException(),
            };

        public static RTCDataChannelState FromNative(this SIPSorcery.Net.RTCDataChannelState nativeTCDataChannelState) =>
            nativeTCDataChannelState switch
            {
                SIPSorcery.Net.RTCDataChannelState.connecting => RTCDataChannelState.Connecting,
                SIPSorcery.Net.RTCDataChannelState.open => RTCDataChannelState.Open,
                SIPSorcery.Net.RTCDataChannelState.closing => RTCDataChannelState.Closing,
                SIPSorcery.Net.RTCDataChannelState.closed => RTCDataChannelState.Closed,
                _ => throw new NotImplementedException(),
            };

    }
}
