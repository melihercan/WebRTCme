using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace WebRTCme
{
    public interface IMediaStreamShared
    {
        public bool Active { get; }

        public bool Ended { get; }

        public string Id { get; }

    }

    public interface IMediaStream : IMediaStreamShared, INativeObjects
    {
        IMediaStream Clone();

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

    public interface IMediaStreamAsync : IMediaStreamShared, INativeObjectsAsync
    {
        Task<IMediaStreamAsync> CloneAsync();

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
