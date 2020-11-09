using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace WebRTCme
{

    public interface IMediaStreamTrackShared
    {
        public string ContentHint { get; set; }
        public bool Enabled { get; set; }
        public string Id { get; }
        public bool Isolated { get; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public MediaStreamTrackKind Kind { get; }
        public string Label { get; }
        public bool Muted { get; }
        public bool Readonly { get; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public MediaStreamTrackState ReadyState { get; }
        public bool Remote { get; }

        public event EventHandler OnMute;
        public event EventHandler OnUnmute;
        public event EventHandler OnEnded;

    }

    public interface IMediaStreamTrack : IMediaStreamTrackShared, INativeObjects
    {
        IMediaStreamTrack Clone();

        MediaTrackCapabilities GetCapabilities();

        MediaTrackSettings GetSettings();

        MediaTrackConstraints GetContraints();
        void ApplyConstraints(MediaTrackConstraints contraints);

        void Stop();

        // Custom APIs.
        ////void Play<TRenderer>(TRenderer renderer);

        TView GetView<TView>();
    }

    public interface IMediaStreamTrackAsync : IMediaStreamTrackShared, INativeObjectsAsync
    {
        Task<IMediaStreamTrackAsync> CloneAsync();

        Task<MediaTrackCapabilities> GetCapabilitiesAsync();

        Task<MediaTrackSettings> GetSettingsAsync();

        Task<MediaTrackConstraints> GetContraintsAsync();

        Task ApplyConstraintsAsync(MediaTrackConstraints contraints);

        Task Stop();

        // Custom APIs.
    }
}
