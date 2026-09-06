namespace WebRTCme.Windows;

/// <summary>
/// A managed grouping of tracks. WebRTC's native API has a stream object, but the ABI does not
/// expose one -- streams cross the wire as the id passed to <c>rtc_peer_connection_add_track</c>
/// and read back from the SDP -- so this is bookkeeping on the .NET side.
/// </summary>
internal sealed class MediaStream : IMediaStream
{
    private readonly List<IMediaStreamTrack> _tracks;

    internal MediaStream(IEnumerable<IMediaStreamTrack> tracks = null) =>
        _tracks = tracks is null ? [] : [.. tracks];

    internal MediaStream(string id, IEnumerable<IMediaStreamTrack> tracks = null) : this(tracks) =>
        Id = id;

    public string Id { get; } = Guid.NewGuid().ToString();

    public bool Active
    {
        get
        {
            lock (_tracks)
                return _tracks.Any(track => track.ReadyState == MediaStreamTrackState.Live);
        }
    }

    public event EventHandler<IMediaStreamTrackEvent> OnAddTrack;
    public event EventHandler<IMediaStreamTrackEvent> OnRemoveTrack;

    public void AddTrack(IMediaStreamTrack track)
    {
        ArgumentNullException.ThrowIfNull(track);

        lock (_tracks)
        {
            if (_tracks.Contains(track))
                return;
            _tracks.Add(track);
        }

        OnAddTrack?.Invoke(this, new MediaStreamTrackEvent(track));
    }

    public void RemoveTrack(IMediaStreamTrack track)
    {
        ArgumentNullException.ThrowIfNull(track);

        lock (_tracks)
        {
            if (!_tracks.Remove(track))
                return;
        }

        OnRemoveTrack?.Invoke(this, new MediaStreamTrackEvent(track));
    }

    public IMediaStream Clone()
    {
        lock (_tracks)
            return new MediaStream(_tracks);
    }

    public IMediaStreamTrack[] GetTracks()
    {
        lock (_tracks)
            return [.. _tracks];
    }

    public IMediaStreamTrack[] GetAudioTracks() => TracksOfKind(MediaStreamTrackKind.Audio);

    public IMediaStreamTrack[] GetVideoTracks() => TracksOfKind(MediaStreamTrackKind.Video);

    public IMediaStreamTrack GetTrackById(string id)
    {
        lock (_tracks)
            return _tracks.FirstOrDefault(track => track.Id == id);
    }

    private IMediaStreamTrack[] TracksOfKind(MediaStreamTrackKind kind)
    {
        lock (_tracks)
            return [.. _tracks.Where(track => track.Kind == kind)];
    }

    /// <summary>
    /// Disposes only the grouping. Tracks outlive the streams they appear in -- the same track
    /// is routinely in a local stream and attached to several peer connections -- so ownership
    /// stays with whoever created them.
    /// </summary>
    public void Dispose()
    {
        lock (_tracks)
            _tracks.Clear();
    }
}
