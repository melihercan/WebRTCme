using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace WebRTCme
{
    public interface IMediaStream : INativeObject
    {
        public bool Active { get; }

        ////public Task<bool> Ended { get; }

        public string Id { get; }

        IMediaStream Clone();

        List<IMediaStreamTrack> GetTracks();

        IMediaStreamTrack GetTrackById(string id);

        List<IMediaStreamTrack> GetVideoTracks();

        List<IMediaStreamTrack> GetAudioTracks();

        public void AddTrack(IMediaStreamTrack track);
        
        public void RemoveTrack(IMediaStreamTrack track);

        
        
        // Custom APIs.
        void SetElementReferenceSrcObject(object media);
    }

}
