using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace WebRTCme
{

    public interface IMediaStreamTrack : INativeObjects
    {
        public Task<string> ContentHint { get; set; }
        public Task<bool> Enabled { get; set; }
        public Task<string> Id { get; }
        public Task<bool> Isolated { get; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public Task<MediaStreamTrackKind> Kind { get; }
        public Task<string> Label { get; }
        public Task<bool> Muted { get; }
        public Task<bool> Readonly { get; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public Task<MediaStreamTrackState> ReadyState { get; }
        public Task<bool> Remote { get; }

        public event EventHandler OnMute;
        public event EventHandler OnUnmute;
        public event EventHandler OnEnded;


        Task<IMediaStreamTrack> Clone();

        Task<MediaTrackCapabilities> GetCapabilities();

        Task<MediaTrackSettings> GetSettings();

        Task<MediaTrackConstraints> GetContraints();

        Task ApplyConstraints(MediaTrackConstraints contraints);

        Task Stop();

        
        
        
        // Custom APIs.
    }
}
