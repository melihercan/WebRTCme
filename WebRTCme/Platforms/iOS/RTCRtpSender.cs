using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebRTCme;
using WebRTCme.Platforms.iOS.Custom;

namespace WebRTCme.iOS
{
    internal class RTCRtpSender : NativeBase<Webrtc.IRTCRtpSender>, IRTCRtpSender
    {
        public RTCRtpSender(Webrtc.IRTCRtpSender nativeRtpSender) : base(nativeRtpSender)
        { }

        public IRTCDTMFSender Dtmf => new RTCDTMFSender(NativeObject.DtmfSender);

        public IMediaStreamTrack Track => new MediaStreamTrack(NativeObject.Track);

        public IRTCDtlsTransport Transport => throw new NotImplementedException();

        public RTCRtpCapabilities GetCapabilities(string kind)
        {
            throw new NotImplementedException();
        }

        public RTCRtpSendParameters GetParameters() => NativeObject.Parameters
            .FromNativeToSend();

        public Task<IRTCStatsReport> GetStats()
        {
            // The Objective-C category binds statisticsForSender:/statisticsForReceiver: to the
            // generated RTCRtpSender/RTCRtpReceiver classes, but senders and receivers reach us as
            // protocol wrappers, so there is nothing valid to pass. Reaching them needs the binding
            // definition changed to take the protocol interface.
            throw new NotSupportedException(
                "Per-sender stats are not reachable through the current iOS binding; "
                + "use RTCPeerConnection.GetStats() and select the sender entries from the report.");
        }

        /// <summary>
        /// Swaps what this sender is sending, keeping the negotiation it already has.
        /// </summary>
        /// <remarks>
        /// A plain property assignment, where Android needs <c>SetTrack</c> and a decision about
        /// ownership. A null track is meaningful: it stops the sender without renegotiating.
        /// </remarks>
        public Task ReplaceTrack(IMediaStreamTrack newTrack = null)
        {
            NativeObject.Track = newTrack is null
                ? null
                : ((MediaStreamTrack)newTrack).NativeObject as Webrtc.RTCMediaStreamTrack;

            return Task.CompletedTask;
        }

        /// <summary>
        /// Applies changed encodings to the sender.
        /// </summary>
        /// <remarks>
        /// The native parameters are fetched, mutated and assigned back. <c>parameters</c> is
        /// declared <c>copy</c>, so what comes out is already safe to change - but it has to be
        /// the object the sender produced, because it carries the codecs and header extensions the
        /// far side negotiated, and a fresh one would send those back empty.
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

            var nativeEncodings = native.Encodings ?? Array.Empty<Webrtc.RTCRtpEncodingParameters>();

            for (var index = 0; index < parameters.Encodings.Length; index++)
            {
                var encoding = parameters.Encodings[index];

                var target = encoding.Rid is not null
                    ? nativeEncodings.FirstOrDefault(candidate => candidate.Rid == encoding.Rid)
                    : index < nativeEncodings.Length ? nativeEncodings[index] : null;

                if (target is null)
                    continue;

                target.IsActive = encoding.Active;
                target.MaxBitrateBps = encoding.MaxBitrate;
                target.MaxFramerate = encoding.MaxFramerate;
                target.ScaleResolutionDownBy = encoding.ScaleResolutionDownBy;
            }

            NativeObject.Parameters = native;
            return Task.CompletedTask;
        }

        public void SetStreams(IMediaStream[] mediaStreams)
        {
            throw new NotImplementedException();
        }
    }
}
