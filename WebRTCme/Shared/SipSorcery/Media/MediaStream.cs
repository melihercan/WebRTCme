using System;
using System.Collections.Generic;
using System.Linq;

namespace WebRTCme.Shared.SipSorcery.Media
{
    /// <summary>
    /// Browser-shaped media stream over SipSorcery capture endpoints. SIPSorcery has no stream
    /// concept, so this is purely a container that groups the tracks produced by one
    /// GetUserMedia call.
    /// </summary>
    internal class MediaStream : IMediaStream
    {
        private readonly List<IMediaStreamTrack> _tracks = new();

        public MediaStream() => Id = Guid.NewGuid().ToString();

        public MediaStream(IEnumerable<IMediaStreamTrack> tracks) : this() => _tracks.AddRange(tracks);

        public bool Active => _tracks.Any(track => track.ReadyState == MediaStreamTrackState.Live);

        public string Id { get; }

        public event EventHandler<IMediaStreamTrackEvent> OnAddTrack;
        public event EventHandler<IMediaStreamTrackEvent> OnRemoveTrack;

        public void AddTrack(IMediaStreamTrack track)
        {
            if (_tracks.Any(existing => existing.Id == track.Id))
                return;
            _tracks.Add(track);
            OnAddTrack?.Invoke(this, new MediaStreamTrackEvent(track));
        }

        public IMediaStream Clone() => new MediaStream(_tracks);

        public IMediaStreamTrack[] GetAudioTracks() =>
            _tracks.Where(track => track.Kind == MediaStreamTrackKind.Audio).ToArray();

        public IMediaStreamTrack GetTrackById(string id) =>
            _tracks.FirstOrDefault(track => track.Id == id);

        public IMediaStreamTrack[] GetTracks() => _tracks.ToArray();

        public IMediaStreamTrack[] GetVideoTracks() =>
            _tracks.Where(track => track.Kind == MediaStreamTrackKind.Video).ToArray();

        public void RemoveTrack(IMediaStreamTrack track)
        {
            if (!_tracks.Remove(track))
                return;
            OnRemoveTrack?.Invoke(this, new MediaStreamTrackEvent(track));
        }

        public void Dispose()
        {
            foreach (var track in _tracks.ToArray())
                track.Dispose();
            _tracks.Clear();
        }
    }
}
