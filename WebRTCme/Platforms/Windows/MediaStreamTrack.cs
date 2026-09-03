using System;
using System.Threading.Tasks;
using SIPSorceryMedia.Windows;

namespace WebRTCme.Windows
{
    /// <summary>
    /// Wraps a SIPSorcery Windows capture endpoint as a browser-shaped media stream track.
    /// SIPSorcery has no track concept of its own - a capture endpoint is both the source and
    /// (for the remote direction) the sink - so a track here is a thin identity/state holder
    /// over one endpoint.
    /// </summary>
    internal class MediaStreamTrack : IMediaStreamTrack
    {
        private readonly uint _width;
        private readonly uint _height;
        private readonly uint _fps;
        private bool _enabled = true;
        private MediaStreamTrackState _readyState = MediaStreamTrackState.Live;

        private MediaStreamTrack(MediaStreamTrackKind kind, string deviceId, string label,
            bool isRemote)
        {
            Kind = kind;
            DeviceId = deviceId;
            Label = label;
            IsRemote = isRemote;
            Id = Guid.NewGuid().ToString();
        }

        internal MediaStreamTrack(WindowsAudioEndPoint audioEndPoint, string deviceId, string label,
            bool isRemote = false)
            : this(MediaStreamTrackKind.Audio, deviceId, label, isRemote)
        {
            AudioEndPoint = audioEndPoint;
        }

        internal MediaStreamTrack(WindowsVideoEndPoint videoEndPoint, string deviceId, string label,
            uint width, uint height, uint fps, bool isRemote = false)
            : this(MediaStreamTrackKind.Video, deviceId, label, isRemote)
        {
            VideoEndPoint = videoEndPoint;
            _width = width;
            _height = height;
            _fps = fps;
        }

        /// <summary>
        /// True when the endpoint is acting as a sink for media arriving from a peer, false when
        /// it is a local capture source. Determines which half of the endpoint is controlled.
        /// </summary>
        internal bool IsRemote { get; }

        /// <summary>Set for audio tracks; null for video tracks.</summary>
        internal WindowsAudioEndPoint AudioEndPoint { get; }

        /// <summary>Set for video tracks; null for audio tracks.</summary>
        internal WindowsVideoEndPoint VideoEndPoint { get; }

        internal string DeviceId { get; }

        public string ContentHint { get; set; } = string.Empty;

        public bool Enabled
        {
            get => _enabled;
            set
            {
                if (_enabled == value)
                    return;
                _enabled = value;

                // A disabled track is meant to transmit silence/black rather than stop capturing.
                // Pausing the capture endpoint is the closest equivalent SIPSorcery offers.
                // The property is synchronous, so the endpoint task is deliberately not awaited.
                if (_readyState == MediaStreamTrackState.Ended)
                    return;

                _ = value ? ResumeAsync() : PauseAsync();
            }
        }

        public string Id { get; }

        public bool Isolated => false;

        public MediaStreamTrackKind Kind { get; }

        public string Label { get; }

        public bool Muted => !_enabled;

        public bool Readonly => false;

        public MediaStreamTrackState ReadyState => _readyState;

        public event EventHandler OnEnded;
        public event EventHandler OnMute;
        public event EventHandler OnUnmute;

        public Task ApplyConstraints(MediaTrackConstraints contraints) =>
            throw new NotSupportedException(
                "Applying constraints to a running track is not supported by the SipSorcery binding. " +
                "Pass the constraints to GetUserMedia instead.");

        public IMediaStreamTrack Clone() =>
            throw new NotSupportedException(
                "Cloning a track is not supported by the SipSorcery binding - a capture endpoint " +
                "cannot be shared between two tracks.");

        public MediaTrackCapabilities GetCapabilities() =>
            throw new NotSupportedException(
                "Track capabilities are not reported by the SipSorcery binding.");

        public MediaTrackConstraints GetConstraints() =>
            throw new NotSupportedException(
                "Track constraints are not reported by the SipSorcery binding.");

        /// <remarks>
        /// Width/height/frame rate are the values requested through GetUserMedia; zero means the
        /// capture device chose its own format, which SIPSorcery does not report back.
        /// </remarks>
        public MediaTrackSettings GetSettings() => Kind == MediaStreamTrackKind.Video
            ? new MediaTrackSettings
            {
                DeviceId = DeviceId,
                Width = _width,
                Height = _height,
                FrameRate = _fps,
                AspectRatio = _height == 0 ? 0 : (double)_width / _height
            }
            : new MediaTrackSettings
            {
                DeviceId = DeviceId
            };

        public void Stop()
        {
            if (_readyState == MediaStreamTrackState.Ended)
                return;
            _readyState = MediaStreamTrackState.Ended;

            _ = CloseAsync();

            OnEnded?.Invoke(this, EventArgs.Empty);
        }

        public void Dispose() => Stop();

        private async Task PauseAsync()
        {
            if (AudioEndPoint is not null)
                await (IsRemote ? AudioEndPoint.PauseAudioSink() : AudioEndPoint.PauseAudio());
            if (VideoEndPoint is not null)
                await (IsRemote ? VideoEndPoint.PauseVideoSink() : VideoEndPoint.PauseVideo());
            OnMute?.Invoke(this, EventArgs.Empty);
        }

        private async Task ResumeAsync()
        {
            if (AudioEndPoint is not null)
                await (IsRemote ? AudioEndPoint.ResumeAudioSink() : AudioEndPoint.ResumeAudio());
            if (VideoEndPoint is not null)
                await (IsRemote ? VideoEndPoint.ResumeVideoSink() : VideoEndPoint.ResumeVideo());
            OnUnmute?.Invoke(this, EventArgs.Empty);
        }

        private async Task CloseAsync()
        {
            if (AudioEndPoint is not null)
                await (IsRemote ? AudioEndPoint.CloseAudioSink() : AudioEndPoint.CloseAudio());
            if (VideoEndPoint is not null)
                await (IsRemote ? VideoEndPoint.CloseVideoSink() : VideoEndPoint.CloseVideo());
        }
    }
}
