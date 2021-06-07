using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using WebRTCme;
using WebRTCme.Middleware;
using WebRTCme.Middleware.Models;

namespace WebRTCme.Middleware.Services
{
    public class DataManagerService : IDataManagerService
    {
        // 'ItemsSource' to 'ChatView'.
        public ObservableCollection<DataParameters> DataParametersList { get; set; } = new();
        internal Dictionary<string/*PeerUserName*/, IRTCDataChannel> Peers { get; set; } = new();

        internal readonly ILogger<DataManagerService> Logger;

        internal const ulong Cookie = 0x55aa5aa533cc3cc3;

        public DataManagerService(ILogger<DataManagerService> logger)
        {
            Logger = logger;
        }

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

            SendObject(text);
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


        internal void SendObject(object object_)
        {
            // TODO: ADD MUTEX OR LOCK????

            var dataChannels = Peers.Select(p => p.Value);
            foreach (var dataChannel in dataChannels)
                dataChannel.Send(object_);
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

            async void DataChannel_OnMessage(object sender, IMessageEvent e)
            {
                try
                {
                    Console.WriteLine($"************* DataChannel_OnMessage");

                    string json = string.Empty;
                    BaseDto baseDto = null;

                    var dataParameters = new DataParameters
                    {
                        From = DataFromType.Incoming,
                        PeerUserName = peerUserName,
                        PeerUserNameTextColor = "#123456",
                        Time = DateTime.Now.ToString("HH:mm"),
                    };

                    if (e.Data.GetType() == typeof(byte[]))
                    {
                        var bytes = (byte[])e.Data;
                        json = Encoding.UTF8.GetString(bytes);
                        baseDto = JsonSerializer.Deserialize<BaseDto>(json);
                    }
                    else if (e.Data.GetType() == typeof(string))
                    {
                        var bytes = Convert.FromBase64String((string)e.Data);
                        json = Encoding.UTF8.GetString(bytes);
                        baseDto = JsonSerializer.Deserialize<BaseDto>(json);
                    }
                    else
                        throw new Exception("Bad data type");

                    switch (baseDto.DtoObjectType)
                    {
                        case Enums.DtoObjectType.Message:
                            break;
                        case Enums.DtoObjectType.Link:
                            break;
                        case Enums.DtoObjectType.File:
                            var file = JsonSerializer.Deserialize<FileDto>(json);
                            if (file.Cookie != DataManagerService.Cookie)
                                throw new Exception("Bad cookie");

                            //// TODO: CREATE DATA PARAMETERS LIST FOR INCOMING FILES. ON EACH CALL BOTH STREAM(FOR FILE SAVE)  AND PROGRESS BAR SHOULD BE UPDATED
                            /// 

                            //if (file.Offset == 0)
                            //{
                            //    // New incoming file.

                            //}
                            //else if (file.Offset + (ulong)file.Data.Length == file.Size)
                            //{
                            //    // End of file.
                            //}

                            Logger.LogInformation($"============== READING {file.Name} offset:{file.Offset} count:{file.Data.Length}");




                            break;
                        default:
                            throw new Exception("Unknown object");
                    }

                    DataParametersList.Add(dataParameters);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"************* DataChannel_OnMessage EXCEPTION: {ex.Message}");
                }
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
            private readonly DataParameters _dataParameters;
            private ulong _wrOffset;
            private ulong _rdOffset;

            public WebRtcDataStream(DataManagerService dataManagerService, File file)
            {
                _dataManagerService = dataManagerService;
                _file = file;

                _dataParameters = new DataParameters
                {
                    From = DataFromType.Outgoing,
                    Time = DateTime.Now.ToString("HH:mm"),
                    Object = file
                };
                dataManagerService.DataParametersList.Add(_dataParameters);
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
                _dataManagerService.Logger.LogInformation($"============== WRITING {_file.Name} offset:{_wrOffset} count:{count}");

                var data = buffer.Skip(offset).Take(count).ToArray();

                FileDto fileDto = new() 
                { 
                    Cookie = DataManagerService.Cookie,
                    DtoObjectType = Enums.DtoObjectType.File,
                    Name = _file.Name,
                    Size = _file.Size,
                    ContentType = _file.ContentType,
                    Offset = _wrOffset,
                    Data = data
                };
                var json = JsonSerializer.Serialize(fileDto);
                //_dataManagerService.SendObject(json);
                var bytes = Encoding.UTF8.GetBytes(json);
                var base64 = Convert.ToBase64String(bytes);
                _dataManagerService.SendObject(base64);

                _wrOffset += (ulong)count;
            }
        }
    }
}
