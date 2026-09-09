using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace WebRTCme.Middleware
{
    public interface IDataManager
    {
        public ObservableCollection<DataParameters> DataParametersList { get; set; }

        void AddPeer(string peerUserName, IRTCDataChannel dataChannel, IRTCDataChannel producerDataChannel = null,
            IRTCDataChannel consumerDataChannel = null);

        void RemovePeer(string peerUserName);

        void ClearPeers();

        void SendMessage(Message message);

        void SendLink(Link link);

        Task SendFileAsync(File file, Stream stream);
    }
}
