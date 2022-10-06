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


    }
}
