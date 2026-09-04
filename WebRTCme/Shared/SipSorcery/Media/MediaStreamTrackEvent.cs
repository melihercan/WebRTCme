using System;

namespace WebRTCme.Shared.SipSorcery.Media
{
    internal class MediaStreamTrackEvent : IMediaStreamTrackEvent
    {
        public MediaStreamTrackEvent(IMediaStreamTrack track) => Track = track;

        public IMediaStreamTrack Track { get; }

        public void Dispose() { }
    }
}
