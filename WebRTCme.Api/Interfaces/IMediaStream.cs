using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace WebRTCme
{
    public interface IMediaStream : INativeObjects
    {
        public Task<bool> Active { get; }

        ////public Task<bool> Ended { get; }

        public Task<string> Id { get; }

        Task<IMediaStream> Clone();

        Task<List<IMediaStreamTrack>> GetTracks();

        Task<IMediaStreamTrack> GetTrackById(string id);

        Task<List<IMediaStreamTrack>> GetVideoTracks();

        Task<List<IMediaStreamTrack>> GetAudioTracks();

        public Task AddTrack(IMediaStreamTrack track);
        
        public Task RemoveTrack(IMediaStreamTrack track);

        
        
        // Custom APIs.
        Task SetElementReferenceSrcObjectAsync(object media);
    }

}
