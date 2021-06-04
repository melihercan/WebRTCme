using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;

namespace WebRTCme.Middleware
{
    public interface IDataManagerService
    {
        public ObservableCollection<DataParameters> DataParametersList { get; set; }

        void AddPeer(string peerUserName, IRTCDataChannel dataChannel);

        void RemovePeer(string peerUserName);

        void ClearPeers();

        void SendString(string text);

        void SendMessage(Message message);

        void SendLink(Link link);

        Task SendFileAsync(File file);
    }
}
