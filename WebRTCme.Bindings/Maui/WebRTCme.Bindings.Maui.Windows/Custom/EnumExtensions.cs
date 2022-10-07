using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebRTCme.Bindings.Maui.Windows.Custom
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


    }
}
