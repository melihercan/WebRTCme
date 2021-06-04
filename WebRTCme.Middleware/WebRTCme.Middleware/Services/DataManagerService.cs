using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using WebRTCme;
using WebRTCme.Middleware;

namespace WebRTCme.Middleware.Services
{
    public class DataManagerService : IDataManagerService
    {
        // 'ItemsSource' to 'ChatView'.
        public ObservableCollection<DataParameters> DataParametersList { get; set; } = new();
        internal Dictionary<string/*PeerUserName*/, IRTCDataChannel> Peers { get; set; } = new();

        public void AddPeer(string peerUserName, IRTCDataChannel dataChannel)
        {
            AddOrRemovePeer(peerUserName, dataChannel, isRemove: false);
        }

        public void RemovePeer(string peerUserName)
        {
            var dataChannel = Peers[peerUserName];
            AddOrRemovePeer(peerUserName, dataChannel, isRemove: true);
        }

        public void ClearPeers()
        {
            foreach (var peer in Peers)
            {
                var peerUserName = peer.Key;
                var dataChannel = Peers[peerUserName];
                AddOrRemovePeer(peerUserName, dataChannel, isRemove: true);
            }
            Peers.Clear();
            DataParametersList.Clear();
        }

        public void SendString(string text)
        {
            DataParametersList.Add(new DataParameters
            {
                From = DataFromType.Outgoing,
                Time = DateTime.Now.ToString("HH:mm"),
                Text = text
            });

            var dataChannels = Peers.Select(p => p.Value);
            foreach (var dataChannel in dataChannels)
                dataChannel.Send(text);
        }

        public void SendMessage(Message message)
        {

        }

        public void SendLink(Link link)
        {

        }

        public async Task SendFileAsync(File file)
        {
            WebRtcDataStream webRtcDataStream = new(this, file);
            await file.Stream.CopyToAsync(webRtcDataStream, 16384);
        }



        internal void SendBytes(byte[] bytes)
        {
            var dataChannels = Peers.Select(p => p.Value);
            foreach (var dataChannel in dataChannels)
                dataChannel.Send(bytes);
        }

        private void AddOrRemovePeer(string peerUserName, IRTCDataChannel dataChannel, bool isRemove)
        {
            if (isRemove)
            {
                dataChannel.OnOpen -= DataChannel_OnOpen;
                dataChannel.OnClose -= DataChannel_OnClose;
                dataChannel.OnError -= DataChannel_OnError;
                dataChannel.OnMessage -= DataChannel_OnMessage;
                Peers.Remove(peerUserName);
                DataParametersList.Remove(DataParametersList.Single(dp => dp.PeerUserName == peerUserName));
            }
            else
            {
                dataChannel.OnOpen += DataChannel_OnOpen;
                dataChannel.OnClose += DataChannel_OnClose;
                dataChannel.OnError += DataChannel_OnError;
                dataChannel.OnMessage += DataChannel_OnMessage;

                Peers.Add(peerUserName, dataChannel);

                DataParametersList.Add(new DataParameters
                {
                    From = DataFromType.System,
                    PeerUserName = peerUserName,
                    Time = DateTime.Now.ToString("HH:mm"),
                    Text = $"User {peerUserName} has joined the room"
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
                    //// TODO: DECODE OBJECT HERE
                    dataParameters.Object = (byte[])e.Data;
                else if (e.Data.GetType() == typeof(string))
                    dataParameters.Text = (string)e.Data;
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

        private class WebRtcDataStream : Stream
        {
            private readonly DataManagerService _dataManagerService;
            private readonly File _file;

            public WebRtcDataStream(DataManagerService dataManagerService, File file)
            {
                _dataManagerService = dataManagerService;
                _file = file;
            }

            public override bool CanRead => true;

            public override bool CanSeek => false;

            public override bool CanWrite => true;

            public override long Length => 0;

            public override long Position 
            { 
                get => throw new NotImplementedException(); 
                set => throw new NotImplementedException(); 
            }

            public override void Flush()
            {
                throw new NotImplementedException();
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                throw new NotImplementedException();
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                throw new NotImplementedException();
            }

            public override void SetLength(long value)
            {
                throw new NotImplementedException();
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                throw new NotImplementedException();
            }
        }


    }
}
