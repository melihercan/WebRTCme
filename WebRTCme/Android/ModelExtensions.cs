using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WebRTCme;
using Webrtc = Org.Webrtc;

namespace WebRtc.Android
{
    internal static class ModelExtensions
    {
        public static Webrtc.MediaConstraints ToNative(this MediaTrackConstraints constraints)
        {
            var mandatoryDictionary = new Dictionary<string, string>();
            var optionalDictionary = new Dictionary<string, string>();

            mandatoryDictionary.Add("googEchoCancellation", constraints.EchoCancellation.ToBool() ? "true" : "false");
            mandatoryDictionary.Add("googAutoGainControl", constraints.AutoGainControl.ToBool() ? "true" : "false");
            mandatoryDictionary.Add("googHighpassFilter", "false");
            mandatoryDictionary.Add("googNoiseSuppression", constraints.NoiseSuppression.ToBool() ? "true" : "false");

            var mandatory = mandatoryDictionary.Select(
                pair => new Webrtc.MediaConstraints.KeyValuePair(pair.Key, pair.Value)).ToList();
            var optional = optionalDictionary.Select
                (pair => new Webrtc.MediaConstraints.KeyValuePair(pair.Key, pair.Value)).ToList();

            var nativeConstrains = new Webrtc.MediaConstraints
            {
                Mandatory = mandatory,
                Optional = optional
            };

            return nativeConstrains;
        }

        public static Webrtc.PeerConnection.RTCConfiguration ToNative(this RTCConfiguration configuration) =>
            new Webrtc.PeerConnection.RTCConfiguration(configuration.IceServers
                .Select(server => server.ToNative()).ToArray())
            {
                BundlePolicy = configuration.BundlePolicy?.ToNative() ?? Webrtc.PeerConnection.BundlePolicy.Balanced,
                Certificate = (Webrtc.RtcCertificatePem)(configuration.Certificates?.ElementAt(0).NativeObject) ??
                    Webrtc.RtcCertificatePem.GenerateCertificate(),
                IceCandidatePoolSize = configuration.IceCandidatePoolSize ?? 0,
                IceTransportsType = configuration.IceTransportPolicy?.ToNative() ?? 
                    Webrtc.PeerConnection.IceTransportsType.All,
                RtcpMuxPolicy = configuration.RtcpMuxPolicy?.ToNative() ?? Webrtc.PeerConnection.RtcpMuxPolicy.Require 

            };
            
        public static Webrtc.PeerConnection.IceServer ToNative(this RTCIceServer iceServer) =>
            Webrtc.PeerConnection.IceServer.InvokeBuilder(iceServer.Urls)
                .SetTlsCertPolicy(iceServer.CredentialType?.ToNative() ??
                    Webrtc.PeerConnection.TlsCertPolicy.TlsCertPolicySecure)
                .SetUsername(iceServer.Username)
                .SetPassword(iceServer.Credential)
                .CreateIceServer();


        public static Webrtc.DataChannel.Init ToNative(this RTCDataChannelInit dataChannelInit) =>
            new Webrtc.DataChannel.Init
            {
                Ordered = dataChannelInit.Ordered ?? true,
                ///// TODO: CHECK THIS!!!! MaxRetransmitTimeMs = dataChannelInit.MaxPacketLifeTime ?? 0,
                MaxRetransmits = dataChannelInit.MaxRetransmits ?? 0,
                Protocol = dataChannelInit.Protocol,
                Negotiated = dataChannelInit.Negotiated ?? false,
                Id = dataChannelInit.Id ?? 0
            };

    }
}
