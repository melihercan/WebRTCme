using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace WebRTCme
{
    public interface IMediaStream :IDisposable // INativeObject
    {
        bool Active { get; }

        string Id { get; }

        event EventHandler<IMediaStreamTrackEvent> OnAddTrack;
        event EventHandler<IMediaStreamTrackEvent> OnRemoveTrack;

        void AddTrack(IMediaStreamTrack track);

        IMediaStream Clone();

        IMediaStreamTrack[] GetAudioTracks();

        IMediaStreamTrack GetTrackById(string id);

        IMediaStreamTrack[] GetTracks();

        IMediaStreamTrack[] GetVideoTracks();

        void RemoveTrack(IMediaStreamTrack track);

        
       
    }

}
