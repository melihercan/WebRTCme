using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace WebRTCme.Middleware
{
    public interface IMediaManager
    {
        ObservableCollection<MediaParameters> MediaParametersList { get; set; }

        void AddPeer(string peerUserName, MediaParameters mediaParameters);
        void RemovePeer(string peerUserName);

        void Update(string peerUserName, MediaParameters mediaParameters);

    }
}
