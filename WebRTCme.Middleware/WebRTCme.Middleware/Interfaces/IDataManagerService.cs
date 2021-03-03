using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace WebRTCme.Middleware
{
    public interface IDataManagerService
    {
        public ObservableCollection<DataParameters> DataParametersList { get; set; }

        void AddPeer(string peerUserName, IRTCDataChannel dataChannel);

        void RemovePeer(string peerUserName);

        void SendBytes(byte[] data);

        void SendString(string message);

    }
}
