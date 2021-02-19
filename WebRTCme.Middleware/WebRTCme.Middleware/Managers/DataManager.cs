using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using WebRTCme;
using WebRTCme.Middleware;

namespace WebRtcMeMiddleware.Managers
{
    public class DataManager : IDataManager
    {
        // 'ItemsSource' to 'ChatView'.
        public ObservableCollection<DataParameters> DataParametersList { get; set; } = new();
        private Dictionary<string/*PeerUserName*/, IRTCDataChannel> _peers = new();

        public void AddPeer(string peerUserName, IRTCDataChannel dataChannel)
        {
            throw new NotImplementedException();
        }

        public void RemovePeer(string peerUserName)
        {
            throw new NotImplementedException();
        }

        public void SendBytes(byte[] data)
        {
            throw new NotImplementedException();
        }

        public void SendString(string message)
        {
            throw new NotImplementedException();
        }
    }
}
