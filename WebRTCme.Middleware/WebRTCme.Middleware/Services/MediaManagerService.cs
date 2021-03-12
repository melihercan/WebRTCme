using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using WebRTCme.Middleware;

namespace WebRTCme.Middleware.Services
{
    public class MediaManagerService : IMediaManagerService
    {
        // Will be used as 'ItemsSource'. 
        public ObservableCollection<MediaParameters> MediaParametersList { get; set; } = new();
        private Dictionary<string/*PeerUserName*/, MediaParameters> _peers = new();


        public void AddPeer(string peerUserName, MediaParameters mediaParameters)
        {
            MediaParametersList.Add(mediaParameters);
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
