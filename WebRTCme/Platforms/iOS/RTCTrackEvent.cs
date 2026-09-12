using System;
using System.Collections.Generic;
using System.Text;
using WebRTCme;
using WebRTCme.Platforms.iOS.Custom;

namespace WebRTCme.iOS
{
    internal class RTCTrackEvent : NativeBase<Webrtc.RTCMediaStreamTrack>, IRTCTrackEvent
    {
        private readonly Webrtc.RTCMediaStreamTrack _nativeTrack;
        private readonly IMediaStream[] _streams;

        public RTCTrackEvent(Webrtc.RTCMediaStreamTrack nativeTrack,
            Webrtc.RTCMediaStream nativeStream = null)
        {
            _nativeTrack = nativeTrack;

            // The stream the track arrived on, which the caller of this constructor has and used
            // to throw away. It is the only thing that distinguishes two video tracks from one
            // peer - see SignalingConnection.OnTrack - so discarding it made a second source
            // impossible to recognise here.
            _streams = nativeStream is null
                ? Array.Empty<IMediaStream>()
                : new IMediaStream[] { new MediaStream(nativeStream) };
        }

        public IRTCRtpReceiver Receiver => throw new NotImplementedException();

        /// <summary>
        /// The streams this track arrived on.
        /// </summary>
        /// <remarks>
        /// This threw <see cref="NotImplementedException"/> until 2026-09-12, and the day it got a
        /// caller it took the app down on launch: peer-to-peer screen sharing needs the stream id
        /// to tell a second source from a camera, so every incoming track went through here and
        /// the exception came out through the Objective-C trampoline as an uncaught NSException.
        ///
        /// Worth remembering as the exact failure the gaps document warns about - the stubs are
        /// harmless only while nothing calls them, and "does anything call it" is a question that
        /// changes every time someone writes a feature.
        ///
        /// One stream at most, because the legacy <c>DidAddStream</c> callback this is built from
        /// reports one at a time. Empty rather than throwing when there is none.
        /// </remarks>
        public IMediaStream[] Streams => _streams;

        public IMediaStreamTrack Track => new MediaStreamTrack(_nativeTrack);

        public IRTCRtpTransceiver Transceiver => throw new NotImplementedException();
    }
}
