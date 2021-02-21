using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
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
            AddOrRemovePeer(peerUserName, dataChannel, isRemove: false);
        }


        public void RemovePeer(string peerUserName)
        {
            var dataChannel = _peers[peerUserName];
            AddOrRemovePeer(peerUserName, dataChannel, isRemove: true);
        }

        public void SendBytes(byte[] data)
        {
            DataParametersList.Add(new DataParameters
            {
                From = DataFromType.Outgoing,
                Time = DateTime.Now.ToString("HH:mm"),
                Bytes = data
            });

            var dataChannels = _peers.Select(p => p.Value);
            foreach (var dataChannel in dataChannels)
                dataChannel.Send(data);

        }

        public void SendString(string message)
        {
            DataParametersList.Add(new DataParameters
            {
                From = DataFromType.Outgoing,
                Time = DateTime.Now.ToString("HH:mm"),
                Message = message
            });

            var dataChannels = _peers.Select(p => p.Value);
            foreach (var dataChannel in dataChannels)
                dataChannel.Send(message);
        }

        private void AddOrRemovePeer(string peerUserName, IRTCDataChannel dataChannel, bool isRemove)
        {
            if (isRemove)
            {
                dataChannel.OnOpen -= DataChannel_OnOpen;
                dataChannel.OnClose -= DataChannel_OnClose;
                dataChannel.OnError -= DataChannel_OnError;
                dataChannel.OnMessage -= DataChannel_OnMessage;
                _peers.Remove(peerUserName);

                DataParametersList.Add(new DataParameters
                {
                    From = DataFromType.System,
                    PeerUserName = peerUserName,
                    Time = DateTime.Now.ToString("HH:mm"),
                    Message = $"User {peerUserName} has left the room"
                });
            }
            else
            {
                dataChannel.OnOpen += DataChannel_OnOpen;
                dataChannel.OnClose += DataChannel_OnClose;
                dataChannel.OnError += DataChannel_OnError;
                dataChannel.OnMessage += DataChannel_OnMessage;

                _peers.Add(peerUserName, dataChannel);

                DataParametersList.Add(new DataParameters
                {
                    From = DataFromType.System,
                    PeerUserName = peerUserName,
                    Time = DateTime.Now.ToString("HH:mm"),
                    Message = $"User {peerUserName} has joined the room"
                });
            }

            void DataChannel_OnOpen(object sender, EventArgs e)
            {
                Console.WriteLine($"************* DataChannel_OnOpen");
            }

            void DataChannel_OnClose(object sender, EventArgs e)
            {
                Console.WriteLine($"************* DataChannel_OnClose");
            }

            void DataChannel_OnMessage(object sender, IMessageEvent e)
            {
                Console.WriteLine($"************* DataChannel_OnMessage");

                var dataParameters = new DataParameters
                {
                    From = DataFromType.Incoming,
                    PeerUserName = peerUserName,
                    PeerUserNameTextColor = "#123456",
                    Time = DateTime.Now.ToString("HH:mm"),
                };

                if (e.Data.GetType() == typeof(byte[]))
                    dataParameters.Bytes = (byte[])e.Data;
                else if (e.Data.GetType() == typeof(string))
                    dataParameters.Message = (string)e.Data;
                else
                    throw new Exception("Bad data type");

                DataParametersList.Add(dataParameters);
            }

            void DataChannel_OnError(object sender, IErrorEvent e)
            {
                Console.WriteLine($"************* DataChannel_OnError");
                throw new NotImplementedException();
            }
        }
    }
}
