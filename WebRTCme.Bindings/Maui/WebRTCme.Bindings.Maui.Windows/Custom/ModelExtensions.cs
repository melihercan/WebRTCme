using ABI.System;
using Org.BouncyCastle.Cmp;
using SIPSorcery.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebRTCme.Bindings.Maui.Windows.Custom
{
    internal static class ModelExtensions
    {
        public static SIPSorcery.Net.RTCConfiguration ToNative(this RTCConfiguration configuration)
        {
            RTCIceServer[] iceServers = configuration.IceServers ?? new RTCIceServer[] { };

            SIPSorcery.Net.RTCConfiguration nativeConfiguration = new()
            {
                iceServers = iceServers.Select(s => s.ToNative()).ToList(),
                iceTransportPolicy = configuration.IceTransportPolicy.HasValue ? 
                    configuration.IceTransportPolicy.Value.ToNative() : SIPSorcery.Net.RTCIceTransportPolicy.all,
                bundlePolicy = configuration.BundlePolicy.HasValue ? 
                    configuration.BundlePolicy.Value.ToNative() : SIPSorcery.Net.RTCBundlePolicy.balanced,
                rtcpMuxPolicy = configuration.RtcpMuxPolicy.HasValue ? 
                    configuration.RtcpMuxPolicy.Value.ToNative() : SIPSorcery.Net.RTCRtcpMuxPolicy.require,
                //certificates = 
                //certificates2 = 
            };

            return nativeConfiguration;
        }

        public static SIPSorcery.Net.RTCIceServer ToNative(this RTCIceServer iceServer)
        {
            //// TODO iceServer.Urls is an array
            SIPSorcery.Net.RTCIceServer nativeIceServer = new()
            {
                urls = iceServer.Urls[0],
                username = iceServer.Username,
                credentialType = iceServer.CredentialType.HasValue ? 
                    iceServer.CredentialType.Value.ToNative() : SIPSorcery.Net.RTCIceCredentialType.password,
                credential = iceServer.Credential,
            };
            return nativeIceServer;
        }

    }
}
