using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace WebRTCme
{
    public interface IMediaStream : INativeObject
    {
        List<IMediaStreamTrack> GetTracks();


        // Custom APIs.
        void SetElementReferenceSrcObject(object media);
    }

    public interface IMediaStreamAsync : INativeObjectAsync
    {
        Task<List<IMediaStreamTrackAsync>> GetTracksAsync();


        // Custom APIs.
        Task SetElementReferenceSrcObjectAsync(object media);
    }

}
