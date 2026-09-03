using System;

namespace WebRTCme.Windows
{
    internal class MediaStreamTrackEvent : IMediaStreamTrackEvent
    {
        public MediaStreamTrackEvent(IMediaStreamTrack track) => Track = track;

        public IMediaStreamTrack Track { get; }

        public void Dispose() { }
    }
}
