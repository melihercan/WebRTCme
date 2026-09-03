using System;

namespace WebRTCme.Shared.SipSorcery
{
    internal class RTCTrackEvent : IRTCTrackEvent
    {
        public RTCTrackEvent(IMediaStreamTrack track, IMediaStream[] streams = null)
        {
            Track = track;
            Streams = streams ?? Array.Empty<IMediaStream>();
        }

        public IMediaStreamTrack Track { get; }

        public IMediaStream[] Streams { get; }

        /// <summary>SIPSorcery has no receiver object; the track carries everything available.</summary>
        public IRTCRtpReceiver Receiver => null;

        /// <summary>SIPSorcery has no transceiver object.</summary>
        public IRTCRtpTransceiver Transceiver => null;

        public void Dispose() { }
    }
}
