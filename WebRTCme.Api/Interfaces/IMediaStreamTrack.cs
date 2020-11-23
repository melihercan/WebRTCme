using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace WebRTCme
{

    public interface IMediaStreamTrack : INativeObjects
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


        IMediaStreamTrack Clone();

        MediaTrackCapabilities GetCapabilities();

        MediaTrackSettings GetSettings();

        MediaTrackConstraints GetContraints();

        Task ApplyConstraints(MediaTrackConstraints contraints);

        void Stop();




        // Custom APIs.
        TView GetView<TView>();

    }
}
