using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WebRTCme;
using Webrtc = Org.Webrtc;

namespace WebRTCme.Android
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

        public static Webrtc.PeerConnection.RTCConfiguration ToNative(this RTCConfiguration configuration)
        {
            RTCIceServer[] iceServers = configuration.IceServers ?? new RTCIceServer[] { };
            var nativeConfiguration = new Webrtc.PeerConnection.RTCConfiguration(iceServers
               .Select(server => server.ToNative()).ToList());
            if (configuration.BundlePolicy.HasValue)
                nativeConfiguration.BundlePolicy = configuration.BundlePolicy?.ToNative();
            if (configuration.Certificates is not null)
                nativeConfiguration.Certificate = (Webrtc.RtcCertificatePem)
                    ((RTCCertificate)configuration.Certificates?.ElementAt(0)).NativeObject;
            if (configuration.IceCandidatePoolSize.HasValue)
                nativeConfiguration.IceCandidatePoolSize = (int)configuration.IceCandidatePoolSize;
            if (configuration.IceTransportPolicy.HasValue)
                nativeConfiguration.IceTransportsType = configuration.IceTransportPolicy?.ToNative();
            if (configuration.RtcpMuxPolicy.HasValue)
                nativeConfiguration.RtcpMuxPolicy = configuration.RtcpMuxPolicy?.ToNative();
            nativeConfiguration.SdpSemantics = Webrtc.PeerConnection.SdpSemantics.UnifiedPlan;
            return nativeConfiguration;
        }

        public static Webrtc.PeerConnection.IceServer ToNative(this RTCIceServer iceServer)
        {
            var nativeIceServerBuilder = Webrtc.PeerConnection.IceServer.InvokeBuilder(iceServer.Urls.ToList());
            nativeIceServerBuilder.SetTlsCertPolicy(Webrtc.PeerConnection.TlsCertPolicy.TlsCertPolicySecure);
            if (iceServer.Username is not null)
                nativeIceServerBuilder.SetUsername(iceServer.Username);
            if (iceServer.Credential is not null)
                nativeIceServerBuilder.SetPassword(iceServer.Credential);
            var nativeIceServer = nativeIceServerBuilder.CreateIceServer();
            return nativeIceServer;
        }

        public static Webrtc.DataChannel.Init ToNative(this RTCDataChannelInit dataChannelInit) =>
            new Webrtc.DataChannel.Init
            {
                Ordered = dataChannelInit.Ordered ?? true,
                MaxRetransmitTimeMs = dataChannelInit.MaxPacketLifeTime ?? -1,
                MaxRetransmits = dataChannelInit.MaxRetransmits ?? -1,
                Protocol = dataChannelInit.Protocol ?? string.Empty,
                Negotiated = dataChannelInit.Negotiated ?? false,
                Id = dataChannelInit.Id ?? 1//WebRTCme.WebRtc.Id
            };

        public static Webrtc.SessionDescription ToNative(this RTCSessionDescriptionInit description) =>
            new Webrtc.SessionDescription(description.Type.ToNative(), description.Sdp);

        public static Webrtc.IceCandidate ToNative(this RTCIceCandidateInit iceCandidateInit) =>
            new Webrtc.IceCandidate(iceCandidateInit.SdpMid, (int)iceCandidateInit.SdpMLineIndex,
                iceCandidateInit.Candidate)
            {
                //AdapterType = Webrtc.PeerConnection.AdapterType.AdapterTypeAny,
                ////ServerUrl = ???
            };

        // Null means "no preference" on the way out as much as on the way in, and Java takes null
        // for each of these. Unwrapping an unset one threw, so a caller who set only a bitrate
        // could not produce at all.
        public static Webrtc.RtpParameters.Encoding ToNative(this RTCRtpEncodingParameters parameters) =>
            new Webrtc.RtpParameters.Encoding(parameters.Rid, parameters.Active,
                parameters.ScaleResolutionDownBy is null
                    ? null : (Java.Lang.Double)parameters.ScaleResolutionDownBy.Value)
            {
                MaxBitrateBps = parameters.MaxBitrate is null
                    ? null : (Java.Lang.Integer)(int)parameters.MaxBitrate.Value,
                MaxFramerate = parameters.MaxFramerate is null
                    ? null : (Java.Lang.Integer)(int)parameters.MaxFramerate.Value,
            };

        //{
        //    var sr = (Java.Lang.Double)parameters.ScaleResolutionDownBy;
        //    var mb = (Java.Lang.Integer)(int)parameters.MaxBitrate;
        //    var mf = (Java.Lang.Integer)(int)parameters.MaxFramerate;
        //    var ret = new Webrtc.RtpParameters.Encoding(parameters.Rid, parameters.Active,
        //        sr)
        //    {
        //        MaxBitrateBps = mb,
        //        MaxFramerate = mf
        //    };
        //    return ret;
        //}

        public static Webrtc.RtpTransceiver.RtpTransceiverInit ToNative(this RTCRtpTransceiverInit init)
        {
            RTCRtpEncodingParameters[] initSendEncodings = init.SendEncodings ?? new RTCRtpEncodingParameters[] { };
            var direction = init.Direction is null ? null : ((RTCRtpTransceiverDirection)init.Direction).ToNative();
            var streamIds = init.Streams is null ? null : init.Streams.Select(stream => stream.Id).ToList();
            var sendEncodings = initSendEncodings.Select(encodings => encodings.ToNative()).ToList();
            return new Webrtc.RtpTransceiver.RtpTransceiverInit(direction, streamIds, sendEncodings);
        }

        // numChannels is null for every video codec, and clockRate can be unset too - both are
        // boxed Java Integers, so unwrapping one without checking threw NullReferenceException.
        // That happened on the first video codec in the list, which is why reading a sender's
        // parameters failed on any peer connection actually carrying video.
        public static RTCRtpCodecParameters FromNative(this Webrtc.RtpParameters.Codec nativeCodecParameters) =>
            new RTCRtpCodecParameters
            {
                PayloadType = (byte)nativeCodecParameters.PayloadType,
                ////MimeType = "TODO: FIX ME",
                ClockRate = nativeCodecParameters.ClockRate is null
                    ? null : (ulong?)(int)nativeCodecParameters.ClockRate,
                Channels = nativeCodecParameters.NumChannels is null
                    ? null : (ushort?)(int)nativeCodecParameters.NumChannels,
                ////SdpFmtpLine = "TODO: FIX ME"
            };

        public static RTCRtpHeaderExtensionParameters FromNative(this Webrtc.RtpParameters.HeaderExtension
            nativeHeaderExtension) =>
            new RTCRtpHeaderExtensionParameters
            {
                Uri = nativeHeaderExtension.Uri,
                Id = (ushort)nativeHeaderExtension.Id,
                Encrypted = nativeHeaderExtension.Encrypted
            };

        // RtpParameters.Codecs and .Encodings are bound as the NON-generic
        // System.Collections.IList, so "as List<T>" on one could never have succeeded - it yielded
        // null rather than failing, and every call site below then threw ArgumentNullException
        // from inside LINQ, naming the parameter "source" and pointing nowhere near the cast.
        // That is what made RTCRtpSender.GetParameters unusable on Android, and with it simulcast
        // layer control and any attempt to read back a negotiated ladder.
        //
        // HeaderExtensions is bound generically, hence the second overload.
        static IEnumerable<T> NativeItems<T>(System.Collections.IList nativeList) =>
            nativeList is null ? Enumerable.Empty<T>() : nativeList.Cast<T>();

        static IEnumerable<T> NativeItems<T>(IList<T> nativeList) =>
            nativeList ?? Enumerable.Empty<T>();

        public static RTCRtpReceiveParameters FromNativeToReceive(this Webrtc.RtpParameters nativeRtpParameters) =>
            new RTCRtpReceiveParameters
            {
                Codecs = NativeItems<Webrtc.RtpParameters.Codec>(nativeRtpParameters.Codecs)
                    .Select(nativeCodec => nativeCodec.FromNative()).ToArray(),
                HeaderExtensions = NativeItems(nativeRtpParameters.HeaderExtensions)
                    .Select(nativeHeaderExtension => nativeHeaderExtension.FromNative()).ToArray(),
                Rtcp = null//// TODO: CHECK THIS
            };

        public static RTCRtpSendParameters FromNativeToSend(this Webrtc.RtpParameters nativeRtpParameters) =>
            new RTCRtpSendParameters
            {
                Codecs = NativeItems<Webrtc.RtpParameters.Codec>(nativeRtpParameters.Codecs)
                    .Select(nativeCodec => nativeCodec.FromNative()).ToArray(),
                HeaderExtensions = NativeItems(nativeRtpParameters.HeaderExtensions)
                    .Select(headerExtension => headerExtension.FromNative()).ToArray(),
                Rtcp = null,//// TODO: CHECK THIS
                Encodings = NativeItems<Webrtc.RtpParameters.Encoding>(nativeRtpParameters.Encodings)
                    .Select(nativeEncoding => nativeEncoding.FromNative()).ToArray(),
                TransactionId = nativeRtpParameters.TransactionId
            };

        // Each of these is optional in Java and arrives as a boxed Integer or Double, so an unset
        // one is null. Unwrapping without checking threw on any encoding that left a field to the
        // encoder's discretion, which is most of them.
        public static RTCRtpEncodingParameters FromNative(this Webrtc.RtpParameters.Encoding nativeEncoding) =>
            new RTCRtpEncodingParameters
            {
                Active = nativeEncoding.Active,
                MaxBitrate = nativeEncoding.MaxBitrateBps is null
                    ? null : (ulong?)(int)nativeEncoding.MaxBitrateBps,
                MaxFramerate = nativeEncoding.MaxFramerate is null
                    ? null : (double?)(int)nativeEncoding.MaxFramerate,
                Rid = nativeEncoding.Rid,
                ScaleResolutionDownBy = nativeEncoding.ScaleResolutionDownBy is null
                    ? null : (double?)(double)nativeEncoding.ScaleResolutionDownBy
            };

        public static RTCSessionDescriptionInit FromNative(this Webrtc.SessionDescription nativeDescription) =>
            new RTCSessionDescriptionInit
            {
                //// TODO: No way to get Type, it is class!!!! Type = nativeDescription.Type.FromNative(),
                Sdp = nativeDescription.Description
            };

        public static RTCIceCandidateInit FromNative(this Webrtc.IceCandidate nativeIceCandidate) =>
            new RTCIceCandidateInit
            {
                Candidate = nativeIceCandidate.Sdp,
                SdpMid = nativeIceCandidate.SdpMid,
                SdpMLineIndex = (ushort)nativeIceCandidate.SdpMLineIndex,
                //UsernameFragment = ???
            };


    }
}
