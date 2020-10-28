using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace WebRTCme
{
    public interface IMediaStream : INativeObjects
    {
        IMediaStream Clone();

        public bool Active { get; }

        public bool Ended { get; }

        public string Id { get; }

        List<IMediaStreamTrack> GetTracks();

        IMediaStreamTrack GetTrackById(string id);

        List<IMediaStreamTrack> GetVideoTracks();

        List<IMediaStreamTrack> GetAudioTracks();

        public void AddTrack(IMediaStreamTrack track);

        public void RemoveTrack(IMediaStreamTrack track);

        public event EventHandler<IMediaStreamTrackEvent> OnAddTrack;
        public event EventHandler<IMediaStreamTrackEvent> OnRemoveTrack;
        public event EventHandler OnActive;
        public event EventHandler OnInactive;

        // Custom APIs.
        void SetElementReferenceSrcObject(object media);
    }

    public interface IMediaStreamAsync : INativeObjectsAsync
    {
        Task<IMediaStreamAsync> CloneAsync();

        public bool Active { get; }

        public bool Ended { get; }

        public string Id { get; }

        Task<List<IMediaStreamTrackAsync>> GetTracksAsync();

        Task<IMediaStreamTrackAsync> GetTrackByIdAsync(string id);

        Task<List<IMediaStreamTrack>> GetVideoTracksAsync();

        Task<List<IMediaStreamTrack>> GetAudioTracksAsync();

        public Task AddTrackAsync(IMediaStreamTrackAsync track);
        
        public Task RemoveTrackAsync(IMediaStreamTrackAsync track);

        // Custom APIs.
        Task SetElementReferenceSrcObjectAsync(object media);
    }

}
