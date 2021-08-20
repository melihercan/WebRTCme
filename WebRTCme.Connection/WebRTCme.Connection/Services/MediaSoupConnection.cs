using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Utilme;
using WebRTCme.Connection.MediaSoup;
using WebRTCme.Connection.MediaSoup.Proxy.Client;
using WebRTCme.Connection.MediaSoup.Proxy.Enums;
using WebRTCme.Connection.MediaSoup.Proxy.Models;
using Xamarin.Essentials;

namespace WebRTCme.Connection.Services
{
    class MediaSoupConnection : IConnection, IMediaSoupServerNotify
    {
        readonly IConfiguration _configuration;
        readonly IMediaSoupServerApi _mediaSoupServerApi;
        readonly ILogger<MediaSoupConnection> _logger;
        readonly IWebRtc _webRtc;
        readonly IJSRuntime _jsRuntime;
        readonly MediaSoup.Proxy.Models.Device _device;

        Transport _sendTransport;
        Transport _recvTransport;
        string _displayName;

        bool _produce;
        bool _consume;
        bool _useDataChannel;

        Dictionary<string, Consumer> _consumers = new();
        Dictionary<string, DataConsumer> _dataConsumers = new();
        Dictionary<string, PeerParameters> _peers = new();

        public MediaSoupConnection(IConfiguration configuration, 
            IMediaSoupServerApi mediaSoupServerApi,
            ILogger<MediaSoupConnection> logger, IWebRtc webRtc, IJSRuntime jsRuntime = null)
        {
            _configuration = configuration;
            _mediaSoupServerApi = mediaSoupServerApi;
            _logger = logger;
            _webRtc = webRtc;
            _jsRuntime = jsRuntime;

            _device = GetDevice();
        }


        public IObservable<PeerResponse> ConnectionRequest(UserContext userContext)
        {
            return Observable.Create<PeerResponse>(async observer =>
            {
                var guid = Guid.NewGuid();
                var forceTcp = _configuration.GetValue<bool>("MediaSoupServer:ForceTcp");
                _produce = _configuration.GetValue<bool>("MediaSoupServer:Produce");
                _consume = _configuration.GetValue<bool>("MediaSoupServer:Consume");
                _useDataChannel = _configuration.GetValue<bool>("MediaSoupServer:UseDataChannel");

                _displayName = userContext.Name;

                try
                {
                    _mediaSoupServerApi.NotifyEventAsync += OnNotifyAsync;
                    _mediaSoupServerApi.RequestEventAsync += OnRequestAsync;

                    await _mediaSoupServerApi.ConnectAsync(guid, userContext.Name, userContext.Room);

                    var mediaSoupDevice = new MediaSoup.Proxy.Client.Device();

                    var routerRtpCapabilities = (RtpCapabilities)ParseResponse(MethodName.GetRouterRtpCapabilities,
                        await _mediaSoupServerApi.CallAsync(MethodName.GetRouterRtpCapabilities));
                    await mediaSoupDevice.LoadAsync(routerRtpCapabilities);


                    // Create mediasoup Transport for sending (unless we don't want to produce).
                    if (_produce)
                    {
                        var transportInfo = (TransportInfo)ParseResponse(MethodName.CreateWebRtcTransport,
                            await _mediaSoupServerApi.CallAsync(MethodName.CreateWebRtcTransport,
                                new WebRtcTransportCreateParameters
                                {
                                    ForceTcp = forceTcp,
                                    Producing = true,
                                    Consuming = false,
                                    SctpCapabilities = _useDataChannel ? mediaSoupDevice.SctpCapabilities : null
                                }));

                        _sendTransport = mediaSoupDevice.CreateSendTransport(new TransportOptions 
                        {
                            Id = transportInfo.Id,
                            IceParameters = transportInfo.IceParameters,
                            IceCandidates = transportInfo.IceCandidates,
                            DtlsParameters = transportInfo.DtlsParameters,
                            SctpParameters = transportInfo.SctpParameters,
                            IceServers = new RTCIceServer[] { },
                            //// AdditionalSettings = TODO: this goes to Handler.Run and as parametere to RTCPeerConnection???
                            //// ProprietaryConstraints = TODO: this goes to Handler.Run and as parametere to RTCPeerConnection???
                        });

                        _sendTransport.OnConnectAsync += SendTransport_OnConnectAsync;
                        _sendTransport.OnConnectionStateChangeAsync += SendTransport_OnConnectionStateChangeAsync;
                        _sendTransport.OnProduceAsync += SendTransport_OnProduceAsync;
                        _sendTransport.OnProduceDataAsync += SendTransport_OnProduceDataAsync;

                    }

                    // Create mediasoup Transport for receiving (unless we don't want to consume).
                    if (_consume)
                    {
                        var transportInfo = (TransportInfo)ParseResponse(MethodName.CreateWebRtcTransport,
                            await _mediaSoupServerApi.CallAsync(MethodName.CreateWebRtcTransport,
                                new WebRtcTransportCreateParameters
                                {
                                    ForceTcp = forceTcp,
                                    Producing = false,
                                    Consuming = true,
                                    SctpCapabilities = _useDataChannel ? mediaSoupDevice.SctpCapabilities : null
                                }));

                        _recvTransport = mediaSoupDevice.CreateRecvTransport(new TransportOptions
                        {
                            Id = transportInfo.Id,
                            IceParameters = transportInfo.IceParameters,
                            IceCandidates = transportInfo.IceCandidates,
                            DtlsParameters = transportInfo.DtlsParameters,
                            SctpParameters = transportInfo.SctpParameters,
                            IceServers = new RTCIceServer[] { },
                            //// AdditionalSettings = TODO: this goes to Handler.Run and as parametere to RTCPeerConnection???
                            //// ProprietaryConstraints = TODO: this goes to Handler.Run and as parametere to RTCPeerConnection???
                        });

                        _recvTransport.OnConnectAsync += RecvTransport_OnConnectAsync;
                        _recvTransport.OnConnectionStateChangeAsync += RecvTransport_OnConnectionStateChangeAsync;
                    }

                    // Join now into the room.
                    // NOTE: Don't send our RTP capabilities if we don't want to consume.
                    var peers = (Peer[])ParseResponse(MethodName.Join,
                        await _mediaSoupServerApi.CallAsync(MethodName.Join,
                            new JoinParameters
                            {
                                DisplayName = _displayName,
                                Device = _device,
                                RtpCapabilities = _consume ? mediaSoupDevice.RtpCapabilities : null,
                                SctpCapabilities = _useDataChannel ? mediaSoupDevice.SctpCapabilities : null
                            }));

                    foreach (var peer in peers)
                        OnNewPeer(peer);



                    //if (_produce)
                    //{
                        //var micProducer = await _sendTransport.ProduceAsync(new ProducerOptions
                        //{
                        //    Track = userContext.LocalStream.GetAudioTracks().First(),
                        //    Encodings = new RtpEncodingParameters[] { },
                        //    CodecOptions = new ProducerCodecOptions 
                        //    { 
                        //        OpusStereo = true,
                        //        OpusDtx = true
                        //    }
                        //});

                    //}


                    //connectionContext = new ConnectionContext
                    //{
                    //    ConnectionRequestParameters = request,
                    //    Observer = observer
                    //};

                }
                catch (Exception ex)
                {
                    observer.OnError(ex);
                }

                return async () =>
                {
                    try
                    {
                        _mediaSoupServerApi.NotifyEventAsync -= OnNotifyAsync;
                        _mediaSoupServerApi.RequestEventAsync -= OnRequestAsync;
                        await _mediaSoupServerApi.DisconnectAsync(guid);
                        //await mediaServerProxy.StopAsync();
                    }
                    catch { };
                };

                async Task SendTransport_OnConnectAsync(object sender, DtlsParameters dtlsParameters)
                {
                    _logger.LogInformation($"-------> SendTransport_OnConnectAsync");
                    _ = ParseResponse(MethodName.ConnectWebRtcTransport,
                        await _mediaSoupServerApi.CallAsync(MethodName.ConnectWebRtcTransport,
                            new WebRtcTransportConnectParameters
                            {
                                TransportId = _sendTransport.Id,
                                DtlsParameters = dtlsParameters
                            }));
                }


                async Task SendTransport_OnConnectionStateChangeAsync(object sender, ConnectionState connectionState)
                {
                    if (connectionState == ConnectionState.Connected)
                    {
                        var micProducer = await _sendTransport.ProduceAsync(new ProducerOptions
                        {
                            Track = userContext.LocalStream.GetAudioTracks().First(),
                            Encodings = new RtpEncodingParameters[] { },
                            CodecOptions = new ProducerCodecOptions
                            {
                                OpusStereo = true,
                                OpusDtx = true
                            }
                        });


                    }
                }

                Task<string> SendTransport_OnProduceAsync(object sender, ProduceEventParameters params_)
                {
                    _logger.LogInformation($"-------> SendTransport_OnProduceAsync");
                    throw new NotImplementedException();
                }
                
                Task<string> SendTransport_OnProduceDataAsync(object sender, ProduceDataEventParameters params_)
                {
                    _logger.LogInformation($"-------> SendTransport_OnProduceDataAsync");
                    throw new NotImplementedException();
                }

                async Task RecvTransport_OnConnectAsync(object sender, DtlsParameters dtlsParameters)
                {
                    _logger.LogInformation($"-------> RecvTransport_OnConnectAsync");
                    _ = ParseResponse(MethodName.ConnectWebRtcTransport,
                        await _mediaSoupServerApi.CallAsync(MethodName.ConnectWebRtcTransport,
                            new WebRtcTransportConnectParameters
                            {
                                TransportId = _recvTransport.Id,
                                DtlsParameters = dtlsParameters
                            }));
                }

                async Task RecvTransport_OnConnectionStateChangeAsync(object sender, ConnectionState connectionState)
                {
                    //if (connectionState == ConnectionState.Connected)
                    //{

                    //}
                }


            });


        }


        public async Task OnNotifyAsync(string method, object data)
        {
            _logger.LogInformation($"=======> OnNotifyAsync: {method}");
            
            switch (method)
            {
                case MethodName.NewPeer:
                    {
                        var json = ((JsonElement)data).GetRawText();
                        var peer = JsonSerializer.Deserialize<Peer>(
                            json, JsonHelper.CamelCaseAndIgnoreNullJsonSerializerOptions);
                        OnNewPeer(peer);
                    }
                    break;

                default:
                    _logger.LogError($"-------> UNKNOWN Notification: {method}");
                    break;

            }
        }

        public async Task OnRequestAsync(string method, object data,
            IMediaSoupServerNotify.Accept accept, IMediaSoupServerNotify.Reject reject)
        {
            _logger.LogInformation($"=======> OnRequestAsync: {method}");

            switch (method)
            {
                case MethodName.NewConsumer:
                    {

                    }
                    break;

                case MethodName.NewDataConsumer:
                    DataConsumer dataConsumer = null;
                    {
                        if (!_consume)
                        {
                            reject(403, "I do not want to data consume");
                            return;
                        }
                        if (!_useDataChannel)
                        {
                            reject(403, "I do not want DataChannels");
                            return;
                        }

                        var json = ((JsonElement)data).GetRawText();
   //_logger.LogInformation($"NewDataConsumer.JSON: {json}");
                        var requestData = JsonSerializer.Deserialize<DataConsumerRequestParameters>(
                            json, JsonHelper.CamelCaseAndIgnoreNullJsonSerializerOptions);

                        var appData = requestData.AppData;
                        appData.Add(KeyName.PeerId, requestData.PeerId);  // trick

                        // Invoke accept here, ConsumerDataAsync call assumes DataConsumer is already created.
                        ////accept();


                        dataConsumer = await _recvTransport.ConsumerDataAsync(new DataConsumerOptions 
                        {
                            Id = requestData.Id,
                            DataProducerId = requestData.DataProducerId,
                            SctpStreamParameters = requestData.SctpStreamParameters,
                            Label = requestData.Label,
                            Protocol = requestData.Protocol,
                            AppData = appData
                        });

                        _dataConsumers.Add(dataConsumer.Id, dataConsumer);
                        if (requestData.PeerId is not null)
                        {
                            var peer = _peers[requestData.PeerId];
                            peer.DataConsumerIds.Add(dataConsumer.Id);
                        }

                        dataConsumer.OnOpen += DataConsumer_OnOpen;
                        dataConsumer.OnClose += DataConsumer_OnClose;
                        dataConsumer.OnTransportClosed += DataConsumer_OnTransportClosed;
                        dataConsumer.OnError += DataConsumer_OnError;
                        dataConsumer.OnMessage += DataConsumer_OnMessage;

                        accept();
                    }
                    break;

                    //// TODO: HOW TO DEREGISTER EVENTS???
                    void DataConsumer_OnOpen(object sender, EventArgs e)
                    {
                        _logger.LogInformation($"-------> DataConsumer_OnOpen");

                    }
                    void DataConsumer_OnClose(object sender, EventArgs e)
                    {
                        _logger.LogInformation($"-------> DataConsumer_OnClose");
                        var peer = _peers[(string)dataConsumer.AppData[KeyName.PeerId]];
                        peer.DataConsumerIds.Remove(dataConsumer.Id);
                        _dataConsumers.Remove(dataConsumer.Id);
                    }

                    void DataConsumer_OnTransportClosed(object sender, EventArgs e)
                    {
                        _logger.LogInformation($"-------> DataConsumer_OnTransportClosed");
                        var peer = _peers[(string)dataConsumer.AppData[KeyName.PeerId]];
                        peer.DataConsumerIds.Remove(dataConsumer.Id);
                        _dataConsumers.Remove(dataConsumer.Id);

                    }

                    void DataConsumer_OnError(object sender, IErrorEvent e)
                    {
                        _logger.LogInformation($"-------> DataConsumer_OnError");
                    }

                    void DataConsumer_OnMessage(object sender, IMessageEvent e)
                    {
                        _logger.LogInformation($"-------> DataConsumer_OnMessage");
                        throw new NotImplementedException();
                    }

                default:
                    _logger.LogError($"-------> UNKNOWN Request: {method}");
                    break;
            }


        }






        public Task<IRTCStatsReport> GetStats(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task ReplaceOutgoingTrackAsync(IMediaStreamTrack track, IMediaStreamTrack newTrack)
        {
            throw new NotImplementedException();
        }

        object ParseResponse(string method, Result<object> result)
        {
            _logger.LogInformation($"######## CallAsync Response: {method}");

            if (!result.IsOk)
                throw new Exception(result.ErrorMessage);

            var data = result.Value;
            var json = ((JsonElement)data).GetRawText();

   ////_logger.LogInformation($"JSON: {json}");

            switch (method)
            {
                case MethodName.GetRouterRtpCapabilities:
                    var routerRtpCapabilities = JsonSerializer.Deserialize<RtpCapabilities>(
                        json, JsonHelper.CamelCaseAndIgnoreNullJsonSerializerOptions);
                    
                    // Need to convert object (Parameters.Value) to either string or int.
                    foreach (var codec in routerRtpCapabilities.Codecs)
                    {
                        foreach (var item in codec.Parameters)
                        {
                            var jElement = (JsonElement)item.Value;
                            if (jElement.ValueKind == JsonValueKind.String)
                                codec.Parameters[item.Key] = jElement.GetString();
                            else if (jElement.ValueKind == JsonValueKind.Number)
                                codec.Parameters[item.Key] = jElement.GetInt32();
                        }
                    }

                    return routerRtpCapabilities;

                case MethodName.CreateWebRtcTransport:
                    var transportInfo = JsonSerializer.Deserialize<TransportInfo>(
                        json, JsonHelper.CamelCaseAndIgnoreNullJsonSerializerOptions);
                    return transportInfo;

                case MethodName.Join:
                    var joinResponse = JsonSerializer.Deserialize<JoinResponse>(
                        json, JsonHelper.CamelCaseAndIgnoreNullJsonSerializerOptions);
                    var peers = joinResponse.Peers;
                    return peers;

                case MethodName.ConnectWebRtcTransport:

                    return null;
            }

            return null;

        }


        void OnNewPeer(Peer peer)
        {
            _peers.Add(peer.Id, new PeerParameters 
            { 
                Peer = peer,
                ConsumerIds = new(),
                DataConsumerIds =new(),
            });
        }

        MediaSoup.Proxy.Models.Device GetDevice()
        {
            if (DeviceInfo.Platform == DevicePlatform.Android)
                return new MediaSoup.Proxy.Models.Device
                {
                    Flag = "Android",
                    Name = DeviceInfo.Name,
                    Version = DeviceInfo.Version.ToString()
                };
            else if (DeviceInfo.Platform == DevicePlatform.iOS)
                return new MediaSoup.Proxy.Models.Device
                {
                    Flag = "iOS",
                    Name = DeviceInfo.Name,
                    Version = DeviceInfo.Version.ToString()
                };
            else
                return new MediaSoup.Proxy.Models.Device
                {
                    Flag = "Blazor",
                    Name = "Browser",
                    Version = "1.0"
                };
        }
    }
}
