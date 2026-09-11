using Webrtc = Org.Webrtc;
using System;
using WebRTCme;
using System.Threading.Tasks;
using WebRTCme.Platforms.Android.Custom;

namespace WebRTCme.Android
{
    internal class RTCRtpSender : NativeBase<Webrtc.RtpSender>, IRTCRtpSender
    {
        public void Dispose() { }
        private readonly Webrtc.PeerConnection _nativePeerConnection;

        public RTCRtpSender(Webrtc.RtpSender nativeRtpSender,
            Webrtc.PeerConnection nativePeerConnection = null) : base(nativeRtpSender)
        {
            _nativePeerConnection = nativePeerConnection;
        }

        public RTCRtpSender(Func<Webrtc.RtpSender> nativeRtpSenderProvider,
            Webrtc.PeerConnection nativePeerConnection = null) : base(nativeRtpSenderProvider)
        {
            _nativePeerConnection = nativePeerConnection;
        }

        public IRTCDTMFSender Dtmf => new RTCDTMFSender(NativeObject.Dtmf());

        public IMediaStreamTrack Track => new MediaStreamTrack(() => NativeObject.Track());

        public IRTCDtlsTransport Transport => throw new NotImplementedException();


        public RTCRtpSendParameters GetParameters() => NativeObject.Parameters.FromNativeToSend();

        public Task<IRTCStatsReport> GetStats()
        {
            if (_nativePeerConnection is null)
                throw new InvalidOperationException(
                    "This sender was not created from a peer connection, so it cannot report stats.");

            var tcs = new TaskCompletionSource<IRTCStatsReport>();
            _nativePeerConnection.GetStats(NativeObject, new StatsExtensions.StatsCollectorProxy(tcs));
            return tcs.Task;
        }

        /// <summary>
        /// Applies changed encodings to the sender.
        /// </summary>
        /// <remarks>
        /// The native parameters are fetched, mutated and handed straight back, rather than built
        /// from the managed object. Java's <c>RtpParameters</c> has no public constructor - the
        /// only legitimate instance comes from <c>getParameters()</c> and carries state the native
        /// side checks on the way in - so a fresh one is not an option, and only the fields a
        /// caller can meaningfully change are copied across.
        ///
        /// Matched by rid where there is one, since a ladder's order is not guaranteed to survive
        /// the round trip, and by position otherwise: a single encoding has no rid.
        /// </remarks>
        public Task SetParameters(RTCRtpSendParameters parameters)
        {
            if (parameters?.Encodings is null)
                throw new ArgumentException("There are no encodings to apply.", nameof(parameters));

            var native = NativeObject.Parameters
                ?? throw new InvalidOperationException(
                    "This sender reported no parameters, so there is nothing to change.");

            var nativeEncodings = (native.Encodings as System.Collections.IList)
                ?.Cast<Webrtc.RtpParameters.Encoding>().ToArray()
                ?? Array.Empty<Webrtc.RtpParameters.Encoding>();

            for (var index = 0; index < parameters.Encodings.Length; index++)
            {
                var encoding = parameters.Encodings[index];

                var target = encoding.Rid is not null
                    ? nativeEncodings.FirstOrDefault(candidate => candidate.Rid == encoding.Rid)
                    : index < nativeEncodings.Length ? nativeEncodings[index] : null;

                if (target is null)
                    continue;

                target.Active = encoding.Active;
                target.MaxBitrateBps = encoding.MaxBitrate is null
                    ? null : (Java.Lang.Integer)(int)encoding.MaxBitrate.Value;
                target.MaxFramerate = encoding.MaxFramerate is null
                    ? null : (Java.Lang.Integer)(int)encoding.MaxFramerate.Value;
                target.ScaleResolutionDownBy = encoding.ScaleResolutionDownBy is null
                    ? null : (Java.Lang.Double)encoding.ScaleResolutionDownBy.Value;
            }

            // Refusal is reported by returning false rather than by throwing, and a change that is
            // silently ignored is the exact failure this path exists to prevent.
            if (!NativeObject.SetParameters(native))
                throw new InvalidOperationException("The sender rejected the encoding parameters.");

            return Task.CompletedTask;
        }

        public void SetStreams(IMediaStream[] mediaStreams)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Swaps what this sender is sending, keeping the negotiation it already has.
        /// </summary>
        /// <remarks>
        /// Ownership is not taken. The track belongs to the stream the caller is holding - the
        /// local camera stream, usually, which the preview is still rendering - and letting the
        /// sender dispose it would stop a track that is still in use elsewhere.
        ///
        /// A null track is meaningful: it stops the sender without renegotiating.
        /// </remarks>
        public Task ReplaceTrack(IMediaStreamTrack newTrack = null)
        {
            var native = newTrack is null
                ? null
                : ((MediaStreamTrack)newTrack).NativeObject as Webrtc.MediaStreamTrack;

            if (!NativeObject.SetTrack(native, false))
                throw new InvalidOperationException(
                    "The sender refused the track. A sender can only take a track of the kind it " +
                    "was negotiated for.");

            return Task.CompletedTask;
        }

        public RTCRtpCapabilities GetCapabilities(string kind)
        {
            throw new NotImplementedException();
        }


    }
}