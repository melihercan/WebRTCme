﻿using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace WebRTCme
{
    public interface IMediaStream : INativeObject
    {
        bool Active { get; }

        string Id { get; }

        event EventHandler<IMediaStreamTrackEvent> OnAddTrack;
        event EventHandler<IMediaStreamTrackEvent> OnRemoveTrack;

        void AddTrack(IMediaStreamTrack track);

        IMediaStream Clone();

        List<IMediaStreamTrack> GetAudioTracks();

        IMediaStreamTrack GetTrackById(string id);

        List<IMediaStreamTrack> GetTracks();

        List<IMediaStreamTrack> GetVideoTracks();

        void RemoveTrack(IMediaStreamTrack track);

        
        
        // Custom APIs.
        void SetElementReferenceSrcObject(object media);
    }

}
