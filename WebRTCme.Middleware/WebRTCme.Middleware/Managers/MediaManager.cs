using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using WebRTCme.Middleware;

namespace WebRtcMeMiddleware.Managers
{
    public class MediaManager : IMediaManager
    {
        // Will be used as 'ItemsSource'. 
        public ObservableCollection<MediaParameters> MediaParametersList { get; set; } = new();
        private Dictionary<string/*PeerUserName*/, MediaParameters> _peers = new();


        public void AddPeer(string peerUserName, MediaParameters mediaParameters)
        {
            throw new NotImplementedException();
        }

        public void RemovePeer(string peerUserName)
        {
            throw new NotImplementedException();
        }

        public void Update(string peerUserName, MediaParameters mediaParameters)
        {
            throw new NotImplementedException();
        }
    }
}
