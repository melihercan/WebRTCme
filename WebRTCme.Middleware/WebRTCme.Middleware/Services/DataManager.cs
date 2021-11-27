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
using WebRTCme.Middleware.Helpers;
using WebRTCme.Middleware.Models;

namespace WebRTCme.Middleware.Services
{
    public class DataManager : IDataManager
    {
        // 'ItemsSource' to 'ChatView'.
        public ObservableCollection<DataParameters> DataParametersList { get; set; } = new();
        /*internal*/ private Dictionary<string/*PeerUserName*/, IRTCDataChannel> _peers = new();

        IRTCDataChannel _producerDataChannel;
        Dictionary<string, IRTCDataChannel> _consumerPeers = new();

        private Dictionary<(string peerUserName, Guid guid), Stream>
            _incomingFileStreamDispatcher = new();

        internal readonly ILogger<DataManager> Logger;

        internal const ulong Cookie = 0x55aa5aa533cc3cc3;

        private readonly IWebRtcIncomingFileStreamFactory _webRtcIncomingFileStreamFactory;

        public DataManager(IWebRtcIncomingFileStreamFactory webRtcIncomingFileStreamFactory,
            ILogger<DataManager> logger)
        {
            _webRtcIncomingFileStreamFactory = webRtcIncomingFileStreamFactory;
            Logger = logger;
        }

        public void AddPeer(string peerUserName, IRTCDataChannel dataChannel, IRTCDataChannel producerDataChannel,
            IRTCDataChannel consumerDataChannel)
        {
            if (producerDataChannel is not null)
                _producerDataChannel = producerDataChannel;
            else
                AddOrRemovePeer(peerUserName, dataChannel, consumerDataChannel, isRemove: false);
        }

        public void RemovePeer(string peerUserName)
        {
            IRTCDataChannel dataChannel = null;
            IRTCDataChannel consumerDataChannel = null;
            if (_peers.Count != 0)
                dataChannel = _peers[peerUserName];
            else
                consumerDataChannel = _consumerPeers[peerUserName];
            AddOrRemovePeer(peerUserName, dataChannel, consumerDataChannel, isRemove: true);
        }

        public void ClearPeers()
        {
            IRTCDataChannel dataChannel = null;
            IRTCDataChannel consumerDataChannel = null;

            foreach (var peer in _peers)
            {
                var peerUserName = peer.Key;
                if (_peers.Count != 0)
                    dataChannel = _peers[peerUserName];
                else
                    consumerDataChannel = _consumerPeers[peerUserName];
                AddOrRemovePeer(peerUserName, dataChannel, consumerDataChannel, isRemove: true);
            }
            _peers.Clear();
            _consumerPeers.Clear();
            DataParametersList.Clear();
        }

        public void SendMessage(Message message)
        {
            DataParametersList.Add(new DataParameters
            {
                From = DataFromType.Outgoing,
                Time = DateTime.Now.ToString("HH:mm"),
                Object = message
            });

            MessageDto messageDto = new()
            {
                Cookie = DataManager.Cookie,
                DtoObjectType = Enums.DataObjectType.Message,
                Text = message.Text
            };
            var json = JsonSerializer.Serialize(messageDto);
            var bytes = Encoding.UTF8.GetBytes(json);
            var base64 = Convert.ToBase64String(bytes);
            SendObject(base64);
        }

        public void SendLink(Link link)
        {

        }

        public async Task SendFileAsync(File file, Stream stream)
        {
            var dataParameters = new DataParameters
            {
                From = DataFromType.Outgoing,
                Time = DateTime.Now.ToString("HH:mm"),
                Object = file
            };
            DataParametersList.Add(dataParameters);

            WebRtcOutgoingFileStream webRtcOutgoingFileStream = new(file, SendObject, Logger);
            await stream.CopyToAsync(webRtcOutgoingFileStream, 16384);
            stream.Close();
        }


        internal void SendObject(object object_)
        {
            // TODO: ADD MUTEX OR LOCK????

            if (_peers.Count != 0)
            {
                var dataChannels = _peers.Select(p => p.Value);
                foreach (var dataChannel in dataChannels)
                    dataChannel.Send(object_);
            }
            else
            {
                _producerDataChannel.Send(object_);
            }
        }


        void AddOrRemovePeer(string peerUserName, IRTCDataChannel dataChannel, IRTCDataChannel consumerDataChannel, 
            bool isRemove)
        {
            if (isRemove)
            {
                if (dataChannel is not null)
                {
                    dataChannel.OnOpen -= DataChannel_OnOpen;
                    dataChannel.OnClose -= DataChannel_OnClose;
                    dataChannel.OnError -= DataChannel_OnError;
                    dataChannel.OnMessage -= DataChannel_OnMessage;
                    _peers.Remove(peerUserName);
                }
                else
                {
                    consumerDataChannel.OnOpen -= ConsumerDataChannel_OnOpen;
                    consumerDataChannel.OnClose -= ConsumerDataChannel_OnClose;
                    consumerDataChannel.OnError -= ConsumerDataChannel_OnError;
                    consumerDataChannel.OnMessage -= ConsumerDataChannel_OnMessage;
                    _consumerPeers.Remove(peerUserName);
                }
                DataParametersList.Remove(DataParametersList.Single(dp => dp.PeerUserName == peerUserName));
            }
            else
            {
                if (dataChannel is not null)
                {
                    dataChannel.OnOpen += DataChannel_OnOpen;
                    dataChannel.OnClose += DataChannel_OnClose;
                    dataChannel.OnError += DataChannel_OnError;
                    dataChannel.OnMessage += DataChannel_OnMessage;
                    _peers.Add(peerUserName, dataChannel);
                }
                else
                {
                    consumerDataChannel.OnOpen += ConsumerDataChannel_OnOpen;
                    consumerDataChannel.OnClose += ConsumerDataChannel_OnClose;
                    consumerDataChannel.OnError += ConsumerDataChannel_OnError;
                    consumerDataChannel.OnMessage += ConsumerDataChannel_OnMessage;
                    _consumerPeers.Add(peerUserName, consumerDataChannel);
                }

                DataParametersList.Add(new DataParameters
                {
                    From = DataFromType.System,
                    PeerUserName = peerUserName,
                    Time = DateTime.Now.ToString("HH:mm"),
                    Object = new Message { Text = $"User {peerUserName} has joined the room" }
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
                        case Enums.DataObjectType.Message:
                            var messageDto = JsonSerializer.Deserialize<MessageDto>(json);
                            if (messageDto.Cookie != DataManager.Cookie)
                                throw new Exception("Bad cookie");
                            dataParameters.Object = new Message { Text = messageDto.Text};
                            DataParametersList.Add(dataParameters);
                            break;

                        case Enums.DataObjectType.Link:
                            //DataParametersList.Add(dataParameters);
                            break;

                        case Enums.DataObjectType.File:
                            var fileDto = JsonSerializer.Deserialize<FileDto>(json);
                            if (fileDto.Cookie != DataManager.Cookie)
                                throw new Exception("Bad cookie");

                            Logger.LogInformation($"============== READING {fileDto.Name} offset:{fileDto.Offset} count:{fileDto.Data.Length}");

                            if (!_incomingFileStreamDispatcher.TryGetValue((peerUserName, fileDto.Guid), out var stream))
                            {
                                // New file starting.
                                var file = new File
                                {
                                    Guid = fileDto.Guid,
                                    Name = fileDto.Name,
                                    ContentType = fileDto.ContentType,
                                    Size = fileDto.Size
                                };
                                dataParameters.Object = file;
                                DataParametersList.Add(dataParameters);

                                stream = await _webRtcIncomingFileStreamFactory.CreateAsync(
                                    peerUserName,
                                    dataParameters,
                                    OnWebRtcIncomingFileStreamCompleted);

                                _incomingFileStreamDispatcher.Add((peerUserName, fileDto.Guid), stream);
                            }

                            await stream.WriteAsync(fileDto.Data, 0, fileDto.Data.Length);
                            break;

                        default:
                            throw new Exception("Unknown object");
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine($"************* DataChannel_OnMessage EXCEPTION: {ex.Message}");
                }
            }

            void DataChannel_OnError(object sender, IErrorEvent e)
            {
                Console.WriteLine($"************* DataChannel_OnError");
            }


            void ConsumerDataChannel_OnOpen(object sender, EventArgs e)
            {
                Console.WriteLine($"************* ConsumerDataChannel_OnOpen");
            }

            void ConsumerDataChannel_OnClose(object sender, EventArgs e)
            {
                Console.WriteLine($"************* ConsumerDataChannel_OnClose");
            }

            async void ConsumerDataChannel_OnMessage(object sender, IMessageEvent e)
            {
                try
                {
                    Console.WriteLine($"************* ConsumerDataChannel_OnMessage");

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
                        case Enums.DataObjectType.Message:
                            var messageDto = JsonSerializer.Deserialize<MessageDto>(json);
                            if (messageDto.Cookie != DataManager.Cookie)
                                throw new Exception("Bad cookie");
                            dataParameters.Object = new Message { Text = messageDto.Text };
                            DataParametersList.Add(dataParameters);
                            break;

                        case Enums.DataObjectType.Link:
                            //DataParametersList.Add(dataParameters);
                            break;

                        case Enums.DataObjectType.File:
                            var fileDto = JsonSerializer.Deserialize<FileDto>(json);
                            if (fileDto.Cookie != DataManager.Cookie)
                                throw new Exception("Bad cookie");

                            Logger.LogInformation($"============== READING {fileDto.Name} offset:{fileDto.Offset} count:{fileDto.Data.Length}");

                            if (!_incomingFileStreamDispatcher.TryGetValue((peerUserName, fileDto.Guid), out var stream))
                            {
                                // New file starting.
                                var file = new File
                                {
                                    Guid = fileDto.Guid,
                                    Name = fileDto.Name,
                                    ContentType = fileDto.ContentType,
                                    Size = fileDto.Size
                                };
                                dataParameters.Object = file;
                                DataParametersList.Add(dataParameters);

                                stream = await _webRtcIncomingFileStreamFactory.CreateAsync(
                                    peerUserName,
                                    dataParameters,
                                    OnWebRtcIncomingFileStreamCompleted);

                                _incomingFileStreamDispatcher.Add((peerUserName, fileDto.Guid), stream);
                            }

                            await stream.WriteAsync(fileDto.Data, 0, fileDto.Data.Length);
                            break;

                        default:
                            throw new Exception("Unknown object");
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine($"************* DataChannel_OnMessage EXCEPTION: {ex.Message}");
                }
            }

            void ConsumerDataChannel_OnError(object sender, IErrorEvent e)
            {
                Console.WriteLine($"************* DataChannel_OnError");
            }

        }

        private void OnWebRtcIncomingFileStreamCompleted(string peerUserName, Guid fileGuid)
        {

        }

#if false
        private class WebRtcOutgoingDataStream : Stream
        {
            private readonly DataManagerService _dataManagerService;
            private readonly File _file;
            private readonly DataParameters _dataParameters;
            private ulong _wrOffset;
            private ulong _rdOffset;
            private Guid _guid;

            public WebRtcOutgoingDataStream(DataManagerService dataManagerService, File file)
            {
                _dataManagerService = dataManagerService;
                _file = file;
                _guid = Guid.NewGuid();

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
                    Guid = _guid,
                    Name = _file.Name,
                    Size = _file.Size,
                    Offset = _wrOffset,
                    ContentType = _file.ContentType,
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
#endif
    }
}
