using System;
using System.Globalization;
using System.Threading.Tasks;
using SIPSorceryMedia.Abstractions;

namespace WebRTCme.Shared.SipSorcery.Media
{
    /// <summary>
    /// Wraps a SIPSorcery media endpoint as a browser-shaped media stream track.
    ///
    /// SIPSorcery has no track concept of its own: media is produced by an IAudioSource /
    /// IVideoSource and consumed by an IAudioSink / IVideoSink. A local track therefore wraps a
    /// source and a remote track wraps a sink, which is what <see cref="IsRemote"/> distinguishes.
    /// Working against the abstractions rather than concrete endpoints lets Windows use its
    /// native capture while Mac Catalyst uses FFmpeg.
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
            DeviceId = deviceId ?? string.Empty;
            Label = label;
            IsRemote = isRemote;
            Id = Guid.NewGuid().ToString();
        }

        /// <summary>Local audio capture.</summary>
        internal MediaStreamTrack(IAudioSource audioSource, string deviceId, string label)
            : this(MediaStreamTrackKind.Audio, deviceId, label, isRemote: false) =>
            AudioSource = audioSource;

        /// <summary>Local video capture.</summary>
        internal MediaStreamTrack(IVideoSource videoSource, string deviceId, string label,
            uint width, uint height, uint fps)
            : this(MediaStreamTrackKind.Video, deviceId, label, isRemote: false)
        {
            VideoSource = videoSource;
            _width = width;
            _height = height;
            _fps = fps;
        }

        /// <summary>Remote audio playback.</summary>
        internal MediaStreamTrack(IAudioSink audioSink, string label)
            : this(MediaStreamTrackKind.Audio, deviceId: null, label, isRemote: true) =>
            AudioSink = audioSink;

        /// <summary>Remote video, decoded for rendering.</summary>
        internal MediaStreamTrack(IVideoSink videoSink, string label)
            : this(MediaStreamTrackKind.Video, deviceId: null, label, isRemote: true) =>
            VideoSink = videoSink;

        internal bool IsRemote { get; }

        internal IAudioSource AudioSource { get; }

        internal IVideoSource VideoSource { get; }

        internal IAudioSink AudioSink { get; }

        internal IVideoSink VideoSink { get; }

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
                // Pausing the endpoint is the closest equivalent SIPSorcery offers. The property
                // is synchronous, so the endpoint task is deliberately not awaited.
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
                "Applying constraints to a running track is not supported by the SipSorcery " +
                "binding. Pass the constraints to GetUserMedia instead.");

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

        public override string ToString() =>
            $"{(IsRemote ? "remote" : "local")} {Kind.ToString().ToLower(CultureInfo.InvariantCulture)} " +
            $"track '{Label}'";

        private async Task PauseAsync()
        {
            if (AudioSource is not null)
                await AudioSource.PauseAudio();
            if (VideoSource is not null)
                await VideoSource.PauseVideo();
            if (AudioSink is not null)
                await AudioSink.PauseAudioSink();
            if (VideoSink is not null)
                await VideoSink.PauseVideoSink();
            OnMute?.Invoke(this, EventArgs.Empty);
        }

        private async Task ResumeAsync()
        {
            if (AudioSource is not null)
                await AudioSource.ResumeAudio();
            if (VideoSource is not null)
                await VideoSource.ResumeVideo();
            if (AudioSink is not null)
                await AudioSink.ResumeAudioSink();
            if (VideoSink is not null)
                await VideoSink.ResumeVideoSink();
            OnUnmute?.Invoke(this, EventArgs.Empty);
        }

        private async Task CloseAsync()
        {
            if (AudioSource is not null)
                await AudioSource.CloseAudio();
            if (VideoSource is not null)
                await VideoSource.CloseVideo();
            if (AudioSink is not null)
                await AudioSink.CloseAudioSink();
            if (VideoSink is not null)
                await VideoSink.CloseVideoSink();
        }
    }
}
