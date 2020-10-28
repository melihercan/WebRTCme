using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace WebRTCme
{

    public interface IMediaStreamTrackProperties
    {
        public string ContentHint { get; set; }
        public bool Enabled { get; set; }
        public string Id { get; }
        public bool Isolated { get; }
        public string Kind { get; }
        public string Label { get; }
        public bool Muted { get; }
        public bool Readonly { get; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ReadyState ReadyState { get; }
        public bool Remote { get; }
    }

    public interface IMediaStreamTrack : IMediaStreamTrackProperties, INativeObjects
    {
        //public string ContentHint { get; set; }
        //public bool Enabled { get; set; }
        //public string Id { get; }
        //public bool Isolated { get; }
        //public string Kind { get; }
        //public string Label { get; }
        //public bool Muted { get; }
        //public bool Readonly { get; }
        //[JsonConverter(typeof(JsonStringEnumConverter))]
        //public ReadyState ReadyState { get; }
        //public bool Remote { get; }





    }

    public interface IMediaStreamTrackAsync : IMediaStreamTrackProperties, INativeObjectsAsync
    {
    }
}
