using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using Microsoft.Maui.Devices;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Utilme;
using WebRTCme.Connection.MediaSoup;
using WebRTCme.Connection.Models;
using WebRTCme.Connection.MediaSoup.Client;
////using Xamarin.Essentials;

namespace WebRTCme.Connection.Services
{
    class MediaSoupConnection : IConnection, IMediaSoupServerNotify
    {
        readonly IConfiguration _configuration;
        readonly IMediaSoupServerApi _mediaSoupServerApi;
        readonly ILogger<MediaSoupConnection> _logger;
        readonly IWebRtc _webRtc;
        readonly IJSRuntime _jsRuntime;
        readonly MediaSoup.Device _device;

        ConnectionContext _connectionContext;

        MediaSoup.Client.Device _mediaSoupDevice;
        Transport _sendTransport;
        Transport _recvTransport;
        string _displayName;

        bool _produce;
        bool _consume;
        bool _useDataChannel;

        ConcurrentDictionary<string, Consumer> _consumers = new();
        ConcurrentDictionary<string, DataConsumer> _dataConsumers = new();
        Producer _micProducer;
        Producer _webcamProducer;
        DataProducer _chatDataProducer;
        DataProducer _botDataProducer;
        ConcurrentDictionary<string, PeerParameters> _peers = new();

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
            // This service is a singleton, so a previous call's transports and producers are
            // still here unless something clears them.
            CloseConnection();

            var guid = Guid.NewGuid();
            var forceTcp = _configuration.GetValue<bool>("MediaSoupServer:ForceTcp");
            _produce = _configuration.GetValue<bool>("MediaSoupServer:Produce");
            _consume = _configuration.GetValue<bool>("MediaSoupServer:Consume");
            _useDataChannel = _configuration.GetValue<bool>("MediaSoupServer:UseDataChannel");
            var forceH264 = _configuration.GetValue<bool>("MediaSoupServer:ForceH264");
            var forceVP9 = _configuration.GetValue<bool>("MediaSoupServer:ForceVP9");
            var useSimulcast = _configuration.GetValue<bool>("MediaSoupServer:UseSimulcast");
            var useSharingSimulcast = _configuration.GetValue<bool>("MediaSoupServer:UseSharingSimulcast");
            var audioOnly = _configuration.GetValue<bool>("MediaSoupServer:AudioOnly");
            var e2eKey = _configuration.GetValue<string>("MediaSoupServer:E2eKey");

            _displayName = userContext.Name;

            _connectionContext = new ConnectionContext
            {
                UserContext = userContext,
                Observer = observer
            };


            try
            {
                _mediaSoupServerApi.NotifyEventAsync += OnNotifyAsync;
                _mediaSoupServerApi.RequestEventAsync += OnRequestAsync;

                await _mediaSoupServerApi.ConnectAsync(guid, userContext.Name, userContext.Room);

                _mediaSoupDevice = new MediaSoup.Client.Device();

                var routerRtpCapabilities = (RtpCapabilities)ParseResponse(MethodName.GetRouterRtpCapabilities,
                    await _mediaSoupServerApi.ApiAsync(MethodName.GetRouterRtpCapabilities));
                await _mediaSoupDevice.LoadAsync(routerRtpCapabilities);


                // Create mediasoup Transport for sending (unless we don't want to produce).
                if (_produce)
                {
                    var transportInfo = (TransportInfo)ParseResponse(MethodName.CreateWebRtcTransport,
                        await _mediaSoupServerApi.ApiAsync(MethodName.CreateWebRtcTransport,
                            new WebRtcTransportCreateRequest
                            {
                                ForceTcp = forceTcp,
                                AppData = new WebRtcTransportAppData { Direction = "producer" }
                            }));

                    _sendTransport = _mediaSoupDevice.CreateSendTransport(new TransportOptions
                    {
                        Id = transportInfo.TransportId,
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
                        await _mediaSoupServerApi.ApiAsync(MethodName.CreateWebRtcTransport,
                            new WebRtcTransportCreateRequest
                            {
                                ForceTcp = forceTcp,
                                AppData = new WebRtcTransportAppData { Direction = "consumer" }
                            }));

                    _recvTransport = _mediaSoupDevice.CreateRecvTransport(new TransportOptions
                    {
                        Id = transportInfo.TransportId,
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
                    await _mediaSoupServerApi.ApiAsync(MethodName.Join,
                        new JoinRequest
                        {
                            DisplayName = _displayName,
                            Device = _device,
                            RtpCapabilities = _consume ? _mediaSoupDevice.RtpCapabilities : null
                        }));

                foreach (var peer in peers)
                    OnNewPeer(peer);



                if (_produce)
                {
                    // Produce the stream the caller already opened. Opening the camera a second
                    // time here used to fail on Android with "Failed to start capture request /
                    // onCameraError from another session": the local preview kept the first
                    // capture session, and the tracks produced from this second one carried no
                    // frames, so remote peers saw a black tile.
                    var mediaStream = userContext.LocalStream;
                    if (mediaStream is null)
                        throw new Exception(
                            "UserContext.LocalStream is required when producing is enabled");

                        // Enable mic. An audio-only or video-only local stream is legitimate,
                        // so each track is produced only if the caller actually supplied one.
                        var micTrack = mediaStream.GetAudioTracks().FirstOrDefault();
                        if (micTrack is not null)
                            _micProducer = await _sendTransport.ProduceAsync(new ProducerOptions
                            {
                                Track = micTrack,
                                Encodings = new RtpEncodingParameters[] { },
                                CodecOptions = new ProducerCodecOptions
                                {
                                    OpusStereo = true,
                                    OpusDtx = true
                                }
                            });

                        // Enable webcam.
////                        var mediaDevices = _webRtc.Window(_jsRuntime).Navigator().MediaDevices;
                        //var videoInputDevices = (await mediaDevices.EnumerateDevices())
                        //    .Where(d => d.Kind == MediaDeviceInfoKind.VideoInput)
                        //    .ToArray();

                        //var webcamStream =  await mediaDevices.GetUserMedia(new MediaStreamConstraints
                        //{
                          //  Video = new MediaStreamContraintsUnion { Value = true }
                            //Video = new MediaStreamContraintsUnion
                            //{
                            //    Object = new MediaTrackConstraints
                            //    {
                            //        DeviceId = new ConstrainDOMString
                            //        {
                            //            Ideal = new ConstrainDOMStringUnion
                            //            {
                            //                 Value = videoInputDevices[0].DeviceId
                            //            }
                            //        },
                            //        Width = new ConstrainULong 
                            //        { 
                            //            Object = new ConstrainULongRange 
                            //            { 
                            //                Ideal = 1280
                            //            }
                            //        },
                            //        Height = new ConstrainULong
                            //        {
                            //            Object = new ConstrainULongRange
                            //            {
                            //                Ideal = 720
                            //            }
                            //        }

                            //    }
                            //}
                        //});

////                        var tracks = webcamStream.GetVideoTracks();
                        var webcamTrack = mediaStream.GetVideoTracks().FirstOrDefault();

                        //var caps = webcamTrack.GetCapabilities();
                        //var constraints = webcamTrack.GetConstraints();
                        //var settings = webcamTrack.GetSettings();




                        RtpEncodingParameters[] encodings = null;
                        RtpCodecCapability codec = null;
                        ProducerCodecOptions codecOptions = new()
                        {
                            VideoGoogleStartBitrate = 1000
                        };

                        if (forceH264)
                        {
                            codec = _mediaSoupDevice.RtpCapabilities.Codecs
                                .FirstOrDefault(c => c.MimeType.ToLower() == "video/h264");
                            if (codec is null)
                                throw new Exception("Desired H264 codec+configuration is not supported");
                        }
                        else if (forceVP9)
                        {
                            codec = _mediaSoupDevice.RtpCapabilities.Codecs
                                .FirstOrDefault(c => c.MimeType.ToLower() == "video/vp9");
                            if (codec is null)
                                throw new Exception("Desired VP9 codec+configuration is not supported");
                        }

                        if (useSimulcast)
                        {
                            // If VP9 is the only available video codec then use SVC.
                            var firstVideoCodec = _mediaSoupDevice.RtpCapabilities.Codecs
                                .FirstOrDefault(c => c.Kind == MediaKind.Video);
                            if ((forceVP9 && codec is not null) ||
                                firstVideoCodec?.MimeType.ToLower() == "video/vp9")
                            {
                                encodings = new RtpEncodingParameters[]
                                {
                                    new()
                                    {
                                        ScalabilityMode = "S3T3_KEY"
                                    }
                                };
                            }
                            else
                            {
                                encodings = new RtpEncodingParameters[]
                                {
                                    new()
                                    {
                                        ScaleResolutionDownBy = 4,
                                        MaxBitrate = 500000
                                    },
                                    new()
                                    {
                                        ScaleResolutionDownBy = 2,
                                        MaxBitrate = 1000000
                                    },
                                    new()
                                    {
                                        ScaleResolutionDownBy = 1,
                                        MaxBitrate = 5000000
                                    }
                                };
                            }
                        }

                        if (webcamTrack is not null)
                            _webcamProducer = await _sendTransport.ProduceAsync(new ProducerOptions
                            {
                                Track = webcamTrack,
                                Encodings = encodings ?? new RtpEncodingParameters[] { },
                                CodecOptions = codecOptions,
                                Codec = codec
                            });

                  _logger.LogInformation("Connection completed");


                        ///// DIDN't halp to get producer stream to go 
                        //_ = ParseResponse(MethodName.PauseProducer,
                        //    await _mediaSoupServerApi.ApiAsync(MethodName.PauseProducer,
                        //        new PauseProducerRequest
                        //        {
                        //            ProducerId = _webcamProducer.Id
                        //        })); ;


                        //_ = ParseResponse(MethodName.ResumeProducer,
                        //    await _mediaSoupServerApi.ApiAsync(MethodName.ResumeProducer,
                        //        new ResumeProducerRequest
                        //        {
                        //            ProducerId = _webcamProducer.Id
                        //        })); ;


                    }

                    //_connectionContext = new ConnectionContext
                    //{
                    //    UserContext = userContext,
                    //    Observer = observer
                    //};

#if false
                    ///////////////////TESTING
                    _ = Task.Run(async () =>
                    {
                        while (true)
                        {
                            try
                            {
                                await Task.Delay(2000);

                                var txStats = await _sendTransport.GetStatsAsync();
                                foreach (var entry in txStats)
                                    Console.WriteLine($"{entry.Value.Type} {entry.Key}: " +
                                        string.Join(", ", entry.Value.Members
                                            .Select(member => $"{member.Key}={member.Value}")));

                                //var rxStats = await _recvTransport.GetStatsAsync();


                                //var sendTransportStats = (object)ParseResponse(MethodName.GetTransportStats,
                                //await _mediaSoupServerApi.ApiAsync(MethodName.GetTransportStats, 
                                //new GetTransportStatsRequest { TransportId = _sendTransport.Id }));

                                //var recvTransportStats = (object)ParseResponse(MethodName.GetTransportStats,
                                //await _mediaSoupServerApi.ApiAsync(MethodName.GetTransportStats,
                                //new GetTransportStatsRequest { TransportId = _recvTransport.Id }));

                                //var micProducerStats = (GetProducerStatsResponse[])ParseResponse(MethodName.GetProducerStats,
                                //    await _mediaSoupServerApi.ApiAsync(MethodName.GetProducerStats,
                                //    new GetProducerStatsRequest { ProducerId = _micProducer.Id }));


                                //var webcamProducerStats = (GetProducerStatsResponse[])ParseResponse(MethodName.GetProducerStats,
                                //await _mediaSoupServerApi.ApiAsync(MethodName.GetProducerStats,
                                //    new GetProducerStatsRequest { ProducerId = _webcamProducer.Id }));

                                //_consumers.Values.ToList().ForEach(async consumer =>
                                //{
                                //    var consumerStats = (GetConsumerStatsResponse[])ParseResponse(MethodName.GetConsumerStats,
                                //        await _mediaSoupServerApi.ApiAsync(MethodName.GetConsumerStats,
                                //        new GetConsumerStatsRequest { ConsumerId = consumer.Id }));
                                //});


                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"@@@@@@@@@@@@@@@@@@@@@ EXCEPTION: {ex.Message}");
                                var m = ex.Message;
                            }
                        }

                    });
#endif








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
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Disconnecting from the mediasoup server failed: {ex.Message}");
                    }

                    CloseConnection();
                };

                async Task SendTransport_OnConnectAsync(object sender, DtlsParameters dtlsParameters)
                {
                    _logger.LogInformation($"-------> SendTransport_OnConnectAsync");
                    _ = ParseResponse(MethodName.ConnectWebRtcTransport,
                        await _mediaSoupServerApi.ApiAsync(MethodName.ConnectWebRtcTransport,
                            new WebRtcTransportConnectRequest
                            {
                                TransportId = _sendTransport.Id,
                                DtlsParameters = dtlsParameters
                            }));
                }


                async Task SendTransport_OnConnectionStateChangeAsync(object sender, ConnectionState connectionState)
                {
                    _logger.LogInformation($"-------> SendTransport_OnConnectionStateChangeAsync");
                    if (connectionState == ConnectionState.Connected)
                    {
                        if (_useDataChannel)
                        {
                            // Add chat data producer.
                            _chatDataProducer = await _sendTransport.ProduceDataAsync(new DataProducerOptions
                            {
                                Ordered = false,
                                MaxRetransmits = 1,
                                Label = "chat",
                                Protocol = "medium",
                                AppData = new Dictionary<string, object>() 
                                {
                                    { "info", "my-chat-DataProducer" }
                                }
                            });

                            // Add bot data producer.
                            _botDataProducer = await _sendTransport.ProduceDataAsync(new DataProducerOptions
                            {
                                Ordered = false,
                                MaxPacketLifeTime = 2000,
                                Label = "bot",
                                Protocol = "medium",
                                AppData = new Dictionary<string, object>()
                                {
                                    { "info", "my-bot-DataProducer" }
                                }
                            });

                            _connectionContext.Observer.OnNext(new PeerResponse
                            {
                                Type = PeerResponseType.ProducerDataChannel,
                                Id = Guid.NewGuid(),// TODO: HOW TO GET GUID FOR PEER ID??? requestData.PeerId,
                                Name = "DataProducer",// dataConsumerRequestData.PeerId,//peer.Peer.DisplayName,
                                MediaStream = null,
                                DataChannel = null,
                                ProducerDataChannel = _chatDataProducer.DataChannel,
                                ConsumerDataChannel = null
                            });
                        }
                    }
                }

                async Task<string> SendTransport_OnProduceAsync(object sender, ProduceEventParameters params_)
                {
                    _logger.LogInformation($"-------> SendTransport_OnProduceAsync");

                    var id = (string)ParseResponse(MethodName.Produce,
                        await _mediaSoupServerApi.ApiAsync(MethodName.Produce,
                            new ProduceRequest
                            {
                                TransportId = _sendTransport.Id,
                                Kind = params_.Kind,
                                RtpParameters = params_.RtpParameters,
                                AppData = params_.AppData ?? new Dictionary<string, object>()
                            }));
                    return id;
                }
                
                async Task<string> SendTransport_OnProduceDataAsync(object sender, ProduceDataEventParameters params_)
                {
                    _logger.LogInformation($"-------> SendTransport_OnProduceDataAsync");

                    var id = (string)ParseResponse(MethodName.ProduceData,
                        await _mediaSoupServerApi.ApiAsync(MethodName.ProduceData,
                            new ProduceDataRequest
                            {
                                TransportId = _sendTransport.Id,
                                SctpStreamParameters = params_.SctpStreamParameters,
                                Label = params_.Label,
                                Protocol = params_.Protocol,
                                AppData = params_.AppData ?? new Dictionary<string, object>()
                            }));
                    return id;

                }

                async Task RecvTransport_OnConnectAsync(object sender, DtlsParameters dtlsParameters)
                {
                    _logger.LogInformation($"-------> RecvTransport_OnConnectAsync");
                    _ = ParseResponse(MethodName.ConnectWebRtcTransport,
                        await _mediaSoupServerApi.ApiAsync(MethodName.ConnectWebRtcTransport,
                            new WebRtcTransportConnectRequest
                            {
                                TransportId = _recvTransport.Id,
                                DtlsParameters = dtlsParameters
                            }));
                }

                Task RecvTransport_OnConnectionStateChangeAsync(object sender, ConnectionState connectionState)
                {
                    _logger.LogInformation($"-------> RecvTransport_OnConnectionStateChangeAsync");
                    //if (connectionState == ConnectionState.Connected)
                    //{

                    //}

                    return Task.CompletedTask;
                }


            });


        }


        public Task OnNotifyAsync(string method, object data)
        {
            _logger.LogInformation($"=======> OnNotifyAsync: {method}");
            Console.WriteLine($"=======> OnNotifyAsync: {method}");

            var element = (JsonElement)data;

            switch (method)
            {
                case MethodName.NewPeer:
                    OnNewPeer(JsonSerializer.Deserialize<Peer>(
                        element.GetRawText(), JsonHelper.WebRtcJsonSerializerOptions));
                    break;

                case MethodName.PeerClosed:
                    OnPeerClosed(GetString(element, "peerId"));
                    break;

                case MethodName.PeerDisplayNameChanged:
                    {
                        var peerId = GetString(element, "peerId");
                        if (peerId is not null && _peers.TryGetValue(peerId, out var peer) &&
                            peer.Peer is not null)
                        {
                            peer.Peer.DisplayName = GetString(element, "displayName");
                        }
                    }
                    break;

                case MethodName.ConsumerClosed:
                    {
                        // Closing raises the consumer's own close handler, which is what takes
                        // it out of the peer's list and stops receiving it.
                        var consumerId = GetString(element, "consumerId");
                        if (consumerId is not null && _consumers.TryGetValue(consumerId, out var consumer))
                            consumer.Close();
                    }
                    break;

                case MethodName.ConsumerPaused:
                    {
                        var consumerId = GetString(element, "consumerId");
                        if (consumerId is not null && _consumers.TryGetValue(consumerId, out var consumer))
                            consumer.Pause();
                    }
                    break;

                case MethodName.ConsumerResumed:
                    {
                        var consumerId = GetString(element, "consumerId");
                        if (consumerId is not null && _consumers.TryGetValue(consumerId, out var consumer))
                            consumer.Resume();
                    }
                    break;

                case MethodName.DataConsumerClosed:
                    {
                        var dataConsumerId = GetString(element, "dataConsumerId");
                        if (dataConsumerId is not null &&
                            _dataConsumers.TryGetValue(dataConsumerId, out var dataConsumer))
                        {
                            dataConsumer.Close();
                        }
                    }
                    break;

                // Reported continuously by the server and nothing consumes them yet. Recognised
                // rather than handled, so that a genuinely unknown method still stands out.
                case MethodName.ConsumerScore:
                case MethodName.ConsumerLayersChanged:
                case MethodName.ProducerScore:
                case MethodName.ActiveSpeaker:
                case MethodName.DownlinkBwe:
                case MethodName.SpeakingPeers:
                case MethodName.MediasoupVersion:
                    break;

                default:
                    _logger.LogError($"-------> UNKNOWN Notification: {method}");
                    break;
            }

            return Task.CompletedTask;

            static string GetString(JsonElement element, string name) =>
                element.TryGetProperty(name, out var value) &&
                value.ValueKind == JsonValueKind.String
                    ? value.GetString()
                    : null;
        }

        /// <summary>
        /// Drops a peer that has left and tells the caller it is gone.
        /// </summary>
        /// <remarks>
        /// The server closes the peer's consumers separately, but a peer whose transport died
        /// may never produce those notifications, so anything still attributed to it is closed
        /// here too. Closing twice is a no-op.
        /// </remarks>
        void OnPeerClosed(string peerId)
        {
            if (peerId is null || !_peers.TryRemove(peerId, out var peer))
                return;

            foreach (var consumerId in peer.ConsumerIds.ToArray())
            {
                if (_consumers.TryRemove(consumerId, out var consumer))
                    consumer.Close();
            }

            foreach (var dataConsumerId in peer.DataConsumerIds.ToArray())
            {
                if (_dataConsumers.TryRemove(dataConsumerId, out var dataConsumer))
                    dataConsumer.Close();
            }

            _connectionContext?.Observer.OnNext(new PeerResponse
            {
                Type = PeerResponseType.PeerLeft,
                Id = peer.Id,
                Name = peerId
            });
        }

        public async Task OnRequestAsync(string method, object data,
            IMediaSoupServerNotify.Accept accept, IMediaSoupServerNotify.Reject reject)
        {
            _logger.LogInformation($"=======> OnRequestAsync: {method}");
            Console.WriteLine($"=======> OnRequestAsync: {method}");

            switch (method)
            {
                case MethodName.NewConsumer:
                    Consumer consumer = null;
                    if (!_consume)
                    {
                        reject(403, "I do not want to data consume");
                        return;
                    }

                    var consumerJson = ((JsonElement)data).GetRawText();
                    var consumerRequestData = JsonSerializer.Deserialize<ConsumerRequestParameters>(
                        consumerJson, JsonHelper.WebRtcJsonSerializerOptions);
                    
                    // Convert elements with Dictionary<string, object> to string or number or bool.
                    consumerRequestData.AppData.ToStringOrNumberOrBool();
                    foreach (var codec in consumerRequestData.RtpParameters.Codecs)
                        codec.Parameters.ToStringOrNumberOrBool();
                    foreach (var headerExtension in consumerRequestData.RtpParameters.HeaderExtensions)
                        headerExtension.Parameters.ToStringOrNumberOrBool();


                    var consumerAppData = consumerRequestData.AppData;
                    consumerAppData.Add(KeyName.PeerId, consumerRequestData.PeerId);  // trick

                    ////accept();

                    ////await Task.Delay(1000);

                    Console.WriteLine($"~~~~~~~~~~~~~~~~~~~~~~~~~~~ NEW CONSUMER: {consumerRequestData.Kind}");

                    consumer = await _recvTransport.ConsumeAsync(new ConsumerOptions
                    {
                        Id = consumerRequestData.ConsumerId,
                        ProducerId = consumerRequestData.ProducerId,
                        Kind = consumerRequestData.Kind,
                        RtpParameters = consumerRequestData.RtpParameters,
                        AppData = consumerAppData
                    });
                    Console.WriteLine($"~~~~~~~~~~~~~~~~~~~~~~~~~~~ NEW CONSUMER: after ConsumeAsync {consumerRequestData.Kind} {consumer.Kind}");


                    _consumers[consumer.Id] = consumer;
                    ////if (requestData.PeerId is not null)
                    {
                        var peer = GetOrAddPeer(consumerRequestData.PeerId);
                        peer.ConsumerIds.Add(consumer.Id);
                    }

                    consumer.OnClose += Consumer_OnClose;
                    consumer.OnTransportClosed += Consumer_OnTransportClosed;
                    consumer.OnTrackEnded += Consumer_OnTrackEnded;

                    Console.WriteLine($"~~~~~~~~~~~~~~~~~~~~~~~~~~~ NEW CONSUMER: before accept {consumerRequestData.Kind} {consumer.Kind}");
                    accept();
                    Console.WriteLine($"~~~~~~~~~~~~~~~~~~~~~~~~~~~ NEW CONSUMER: after accept {consumerRequestData.Kind} {consumer.Kind}");

                    // If audio-only mode is enabled, pause it.
                    ////if (consumer.Kind == MediaKind.Video && get 'audioOnly' from config)
                    ////consumer.Pause();


                    // Consumer is ready. Check if stream is ready (both audio and video).
                    // TODO: WE can have audio only calls!!!
                    ////if (requestData.PeerId is not null)
                    {
                        var consumerPeer = GetOrAddPeer(consumerRequestData.PeerId);
                        var consumers = consumerPeer.ConsumerIds
                            .Select(key => _consumers[key])
                            .ToList();
                        foreach (var c in consumers)
                        {
                            Console.WriteLine($"--------------------------- CONSUMER: {c.Kind}");
                        }

                        var audioConsumer =
                            consumers.FirstOrDefault(consumer => consumer.Kind == MediaKind.Audio);
                        var videoConsumer =
                            consumers.FirstOrDefault(consumer => consumer.Kind == MediaKind.Video);

                        Console.WriteLine($"~~~~~~~~~~~~~~~~~~~~~~~~~~~ NEW CONSUMER: {consumerRequestData.Kind} {consumer.Kind}");
                        if (consumer.Kind == MediaKind.Video)
                        {

                            //// TODO: THERE IS A TIMING ISSUE. WITHOUT THE ABOVE DELAY, _webcamProducer is nul!!! CHECK THIS
                            Console.WriteLine($"--------------------------- NEW VIDEO TRACK");
                            _logger.LogInformation($"--------------------------- NEW VIDEO TRACK");

                            ////await Task.Delay(2000);
                            ////_logger.LogInformation($"--------------------------- WEBCAM - muted: {_webcamProducer.Track.Muted} ");

                        }
                        else if (consumer.Kind == MediaKind.Audio)
                        {
                            Console.WriteLine($"--------------------------- NEW AUDIO TRACK");
                            _logger.LogInformation($"--------------------------- NEW AUDIO TRACK");
                        }

                        // TODO: ASSUMED ONLY 1 video and 1 audio trak per peer.
                        if (audioConsumer is not null && videoConsumer is not null)
                        {


#if false
                            //// TESTING
         _ = Task.Run(async () => 
         {
             while (true)
             {
                 try
                 {
                     await Task.Delay(2000);

                     //var txStats = await _sendTransport.GetStatsAsync();
                     //var rxStats = await _recvTransport.GetStatsAsync();


                     //var sendTransportStats = (object)ParseResponse(MethodName.GetTransportStats,
                     //await _mediaSoupServerApi.ApiAsync(MethodName.GetTransportStats, 
                     //new GetTransportStatsRequest { TransportId = _sendTransport.Id }));

                     //var recvTransportStats = (object)ParseResponse(MethodName.GetTransportStats,
                     //await _mediaSoupServerApi.ApiAsync(MethodName.GetTransportStats,
                     //new GetTransportStatsRequest { TransportId = _recvTransport.Id }));

                     var micProducerStats = (GetProducerStatsResponse[])ParseResponse(MethodName.GetProducerStats,
                         await _mediaSoupServerApi.ApiAsync(MethodName.GetProducerStats,
                         new GetProducerStatsRequest { ProducerId = _micProducer.Id }));


                     ////var webcamProducerStats = (GetProducerStatsResponse[])ParseResponse(MethodName.GetProducerStats,
                     ////await _mediaSoupServerApi.ApiAsync(MethodName.GetProducerStats,
                     ////new GetProducerStatsRequest { ProducerId = _webcamProducer.Id }));

                     _consumers.Values.ToList().ForEach(async consumer => 
                     {
                         var consumerStats = (GetConsumerStatsResponse[])ParseResponse(MethodName.GetConsumerStats,
                             await _mediaSoupServerApi.ApiAsync(MethodName.GetConsumerStats,
                             new GetConsumerStatsRequest { ConsumerId = consumer.Id }));
                     });


                 }
                 catch (Exception ex)
                 {
                     Console.WriteLine($"@@@@@@@@@@@@@@@@@@@@@ EXCEPTION: {ex.Message}");
                     var m = ex.Message;
                 }
             }

         });
#endif






                            //_ = ParseResponse(MethodName.PauseConsumer,
                            //    await _mediaSoupServerApi.ApiAsync(MethodName.PauseConsumer,
                            //        new PauseConsumerRequest
                            //        {
                            //            ConsumerId = videoConsumer.Id
                            //        })); ;


                            //_ = ParseResponse(MethodName.ResumeConsumer,
                            //    await _mediaSoupServerApi.ApiAsync(MethodName.ResumeConsumer,
                            //        new ResumeConsumerRequest
                            //        {
                            //            ConsumerId = videoConsumer.Id
                            //        })); ;


                            var mediaStream = _webRtc.Window(_jsRuntime).MediaStream();
                            mediaStream.AddTrack(audioConsumer.Track);
                            mediaStream.AddTrack(videoConsumer.Track);
             ////mediaStream.AddTrack(_webcamProducer.Track);
                            _connectionContext.Observer.OnNext(new PeerResponse
                            {
                                Type = PeerResponseType.PeerJoined,
                                Id = consumerPeer.Id,
                                Name = consumerRequestData.PeerId,
                                MediaStream = mediaStream,
                                DataChannel = /*isInitiator ? dataChannel :*/ null
                            });
                        }
                    }
                    break;

                    void Consumer_OnClose(object sender, EventArgs e)
                    {
                        _logger.LogInformation($"-------> Consumer_OnClose");
                        if (TryGetPeer(consumer.AppData, out var peer))
                            peer.ConsumerIds.Remove(consumer.Id);
                        _consumers.TryRemove(consumer.Id, out _);
                    }

                    void Consumer_OnTransportClosed(object sender, EventArgs e)
                    {
                        _logger.LogInformation($"-------> Consumer_OnTransportClose");
                        if (TryGetPeer(consumer.AppData, out var peer))
                            peer.ConsumerIds.Remove(consumer.Id);
                        _consumers.TryRemove(consumer.Id, out _);
                    }

                    void Consumer_OnTrackEnded(object sender, EventArgs e)
                    {
                        _logger.LogInformation($"-------> Consumer_OnTrackEnded");
                    }


                case MethodName.NewDataConsumer:
                    DataConsumer dataConsumer = null;
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

                    var dataConsumerJson = ((JsonElement)data).GetRawText();
                    //_logger.LogInformation($"NewDataConsumer.JSON: {json}");
                    var dataConsumerRequestData = JsonSerializer.Deserialize<DataConsumerRequestParameters>(
                        dataConsumerJson, JsonHelper.WebRtcJsonSerializerOptions);

                    // Convert elements with Dictionary<string, object> to string or number or bool.
                    dataConsumerRequestData.AppData.ToStringOrNumberOrBool();

                    var appData = dataConsumerRequestData.AppData;
                    appData.Add(KeyName.PeerId, dataConsumerRequestData.PeerId);  // trick

                    // Invoke accept here, ConsumerDataAsync call assumes DataConsumer is already created.
                    ////accept();


                    dataConsumer = await _recvTransport.ConsumeDataAsync(new DataConsumerOptions
                    {
                        Id = dataConsumerRequestData.DataConsumerId,
                        DataProducerId = dataConsumerRequestData.DataProducerId,
                        SctpStreamParameters = dataConsumerRequestData.SctpStreamParameters,
                        Label = dataConsumerRequestData.Label,
                        Protocol = dataConsumerRequestData.Protocol,
                        AppData = appData
                    });

                    _dataConsumers[dataConsumer.Id] = dataConsumer;
                    if (dataConsumerRequestData.PeerId is not null)
                    {
                        var dataConsumerPeer = GetOrAddPeer(dataConsumerRequestData.PeerId);
                        dataConsumerPeer.DataConsumerIds.Add(dataConsumer.Id);
                    }

                    dataConsumer.OnOpen += DataConsumer_OnOpen;
                    dataConsumer.OnClose += DataConsumer_OnClose;
                    dataConsumer.OnTransportClosed += DataConsumer_OnTransportClosed;
                    dataConsumer.OnError += DataConsumer_OnError;
                    dataConsumer.OnMessage += DataConsumer_OnMessage;

                    accept();
                    break;

                    //// TODO: HOW TO DEREGISTER EVENTS???
                    void DataConsumer_OnOpen(object sender, EventArgs e)
                    {
                        _logger.LogInformation($"####=======> {dataConsumer.Label} DataConsumer_OnOpen");
                        if (dataConsumer.DataChannel.Label == "chat")
                        {
                            _connectionContext.Observer.OnNext(new PeerResponse
                            {
                                Type = PeerResponseType.ConsumerDataChannel,
                                Id = Guid.NewGuid(),// TODO: HOW TO GET GUID FOR PEER ID??? requestData.PeerId,
                                Name = dataConsumerRequestData.PeerId,//peer.Peer.DisplayName,
                                MediaStream = null,
                                DataChannel = null,
                                ProducerDataChannel = null,
                                ConsumerDataChannel = dataConsumer.DataChannel
                            });
                        }
                    }
                    void DataConsumer_OnClose(object sender, EventArgs e)
                    {
                        _logger.LogInformation($"####=======> {dataConsumer.Label} DataConsumer_OnClose");
                        if (TryGetPeer(dataConsumer.AppData, out var peer))
                            peer.DataConsumerIds.Remove(dataConsumer.Id);
                        _dataConsumers.TryRemove(dataConsumer.Id, out _);
                    }

                    void DataConsumer_OnTransportClosed(object sender, EventArgs e)
                    {
                        _logger.LogInformation($"####=======> {dataConsumer.Label} DataConsumer_OnTransportClosed");
                        if (TryGetPeer(dataConsumer.AppData, out var peer))
                            peer.DataConsumerIds.Remove(dataConsumer.Id);
                        _dataConsumers.TryRemove(dataConsumer.Id, out _);

                    }

                    void DataConsumer_OnError(object sender, IRTCErrorEvent e)
                    {
                        _logger.LogInformation($"####=======> {dataConsumer.Label} DataConsumer_OnError {e.Error.Message}");
                    }

                    void DataConsumer_OnMessage(object sender, IMessageEvent e)
                    {
                        _logger.LogInformation($"####=======> {dataConsumer.Label} DataConsumer_OnMessage");
                    }

                default:
                    _logger.LogError($"-------> UNKNOWN Request: {method}");
                    break;
            }

            _logger.LogInformation($"=======> OnRequestAsync: {method} ~~~~~~~~~~~~~~~~~~~~~~ END");
            Console.WriteLine($"=======> OnRequestAsync: {method} ~~~~~~~~~~~~~~~~~~~~~~ END");


        }


        /// <summary>
        /// Statistics for what is being received from one peer.
        /// </summary>
        /// <remarks>
        /// A peer arrives over as many consumers as it produces tracks, so their reports are
        /// merged into one. This covers the receive side only: with an SFU there is no
        /// per-peer send side to report, because one set of producers serves every peer.
        /// </remarks>
        public async Task<IRTCStatsReport> GetStats(Guid id)
        {
            var peer = _peers.Values.FirstOrDefault(peer => peer.Id == id);
            if (peer is null)
                throw new ArgumentException($"No peer with id {id} is in this connection.", nameof(id));

            Dictionary<string, RTCStats> stats = new();

            foreach (var consumerId in peer.ConsumerIds.ToArray())
            {
                if (!_consumers.TryGetValue(consumerId, out var consumer))
                    continue;

                var report = await consumer.GetStats();
                if (report is null)
                    continue;

                foreach (var entry in report)
                    stats[entry.Key] = entry.Value;
            }

            return new AggregateStatsReport(stats);
        }

        /// <summary>
        /// Swaps a track this connection is producing, keeping the producer and its negotiation.
        /// </summary>
        public async Task ReplaceOutgoingTrackAsync(IMediaStreamTrack track, IMediaStreamTrack newTrack)
        {
            if (track is null)
                throw new ArgumentNullException(nameof(track));

            var producer = track.Kind switch
            {
                MediaStreamTrackKind.Audio => _micProducer,
                MediaStreamTrackKind.Video => _webcamProducer,
                _ => null
            };

            if (producer is null)
                throw new InvalidOperationException(
                    $"This connection is not producing a {track.Kind} track to replace.");

            await producer.ReplaceTrackAsync(newTrack);
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
                    // Wrapped in its own member rather than being the response body.
                    var routerRtpCapabilities = JsonSerializer.Deserialize<RouterRtpCapabilitiesResponse>(
                        json, JsonHelper.WebRtcJsonSerializerOptions).RouterRtpCapabilities;

                    // Need to convert object (Parameters.Value) to either string or int.
                    foreach (var codec in routerRtpCapabilities.Codecs)
                    {
                        codec.Parameters.ToStringOrNumber();
                    }

                    return routerRtpCapabilities;

                case MethodName.CreateWebRtcTransport:
                    var transportInfo = JsonSerializer.Deserialize<TransportInfo>(
                        json, JsonHelper.WebRtcJsonSerializerOptions);
                    return transportInfo;

                case MethodName.Join:
                    var joinResponse = JsonSerializer.Deserialize<JoinResponse>(
                        json, JsonHelper.WebRtcJsonSerializerOptions);
                    var peers = joinResponse.Peers;
                    return peers;

                case MethodName.ConnectWebRtcTransport:
                    return null;

                case MethodName.Produce:
                    var produceResponse = JsonSerializer.Deserialize<ProduceResponse>(
                        json, JsonHelper.WebRtcJsonSerializerOptions);
                    return produceResponse.ProducerId;

                case MethodName.PauseProducer:
                    var pauseProducerResponse = JsonSerializer.Deserialize<PauseProducerResponse>(
                        json, JsonHelper.WebRtcJsonSerializerOptions);
                    return pauseProducerResponse;

                case MethodName.ResumeProducer:
                    var resumeProducerResponse = JsonSerializer.Deserialize<ResumeProducerResponse>(
                        json, JsonHelper.WebRtcJsonSerializerOptions);
                    return resumeProducerResponse;

                case MethodName.ProduceData:
                    var produceDataResponse = JsonSerializer.Deserialize<ProduceDataResponse>(
                        json, JsonHelper.WebRtcJsonSerializerOptions);
                    return produceDataResponse.DataProducerId;

                case MethodName.PauseConsumer:
                    var pauseConsumerResponse = JsonSerializer.Deserialize<PauseConsumerResponse>(
                        json, JsonHelper.WebRtcJsonSerializerOptions);
                    return pauseConsumerResponse;

                case MethodName.ResumeConsumer:
                    var resumeConsumerResponse = JsonSerializer.Deserialize<ResumeConsumerResponse>(
                        json, JsonHelper.WebRtcJsonSerializerOptions);
                    return resumeConsumerResponse;

                case MethodName.GetTransportStats:
                    // Stats arrive wrapped in a 'stats' member rather than as a bare array.
                    var getTransportStatsResponse = JsonSerializer.Deserialize<StatsResponse<GetTransportStatsResponse>>(
                        json, JsonHelper.WebRtcJsonSerializerOptions).Stats;
                    return getTransportStatsResponse;

                case MethodName.GetProducerStats:
                    // Stats arrive wrapped in a 'stats' member rather than as a bare array.
                    var getProducerStatsResponse = JsonSerializer.Deserialize<StatsResponse<GetProducerStatsResponse>>(
                        json, JsonHelper.WebRtcJsonSerializerOptions).Stats;
                    return getProducerStatsResponse;

                case MethodName.GetConsumerStats:
                    // Stats arrive wrapped in a 'stats' member rather than as a bare array.
                    var getConsumerStatsResponse = JsonSerializer.Deserialize<StatsResponse<GetConsumerStatsResponse>>(
                        json, JsonHelper.WebRtcJsonSerializerOptions).Stats;
                    return getConsumerStatsResponse;

            }

            return null;

        }


        /// <summary>
        /// Returns this connection to its pre-call state.
        /// </summary>
        /// <remarks>
        /// Closing the transports is what releases the peer connections, and with them the
        /// producers, consumers and the tracks they hold. Without it a hung-up call left its
        /// camera, microphone and ICE agents running, and the next call started on top of the
        /// previous one's state.
        /// </remarks>
        void CloseConnection()
        {
            var sendTransport = Interlocked.Exchange(ref _sendTransport, null);
            var recvTransport = Interlocked.Exchange(ref _recvTransport, null);

            foreach (var transport in new[] { sendTransport, recvTransport })
            {
                if (transport is null)
                    continue;

                try
                {
                    transport.Close();
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Closing a mediasoup transport failed: {ex.Message}");
                }
            }

            _consumers.Clear();
            _dataConsumers.Clear();
            _peers.Clear();

            _micProducer = null;
            _webcamProducer = null;
            _chatDataProducer = null;
            _botDataProducer = null;
            _mediaSoupDevice = null;
            _connectionContext = null;
        }

        void OnNewPeer(Peer peer)
        {
            if (peer?.PeerId is null)
            {
                // Better a complaint than a peer recorded under a null key, which takes the
                // whole connection down and says nothing about why.
                _logger.LogError("-------> Ignoring a peer announced without an id");
                return;
            }

            GetOrAddPeer(peer.PeerId).Peer = peer;
        }

        /// <summary>
        /// The record for a peer, created on demand.
        /// </summary>
        /// <remarks>
        /// The server starts sending consumers for a peer the moment we join, and those are
        /// handled on the dispatcher while the join response is still being processed here, so
        /// a consumer can legitimately arrive before its peer has been recorded.
        /// </remarks>
        PeerParameters GetOrAddPeer(string peerId) =>
            _peers.GetOrAdd(peerId, _ => new PeerParameters
            {
                ConsumerIds = new(),
                DataConsumerIds = new(),
            });

        static bool TryGetPeerId(Dictionary<string, object> appData, out string peerId)
        {
            peerId = appData is not null && appData.TryGetValue(KeyName.PeerId, out var value)
                ? value as string
                : null;
            return peerId is not null;
        }

        bool TryGetPeer(Dictionary<string, object> appData, out PeerParameters peer)
        {
            peer = null;
            // The server's bot data producer has no peer id at all.
            return TryGetPeerId(appData, out var peerId) && _peers.TryGetValue(peerId, out peer);
        }

        //        MediaSoup.Device GetDevice()
        //        {
        //#if ANDROID
        //            return new MediaSoup.Device
        //                {
        //                    Flag = "Android",
        //                    Name = DeviceInfoExt.Name,
        //                    Version = DeviceInfoExt.Version.ToString()
        //                };
        //#elif IOS
        //            return new MediaSoup.Device
        //                {
        //                    Flag = "iOS",
        //                    Name = DeviceInfoExt.Name,
        //                    Version = DeviceInfoExt.Version.ToString()
        //                };
        //#else
        //            return new MediaSoup.Device
        //            {
        //                Flag = "Blazor",
        //                Name = "Browser",
        //                Version = "1.0"
        //            };
        //#endif
        //        }

        MediaSoup.Device GetDevice()
        {
            if (DeviceInfo.Platform == DevicePlatform.Android)
                return new MediaSoup.Device
                {
                    Flag = "Android",
                    Name = DeviceInfo.Name,
                    Version = DeviceInfo.Version.ToString()
                };
            else if (DeviceInfo.Platform == DevicePlatform.iOS)
                return new MediaSoup.Device
                {
                    Flag = "iOS",
                    Name = DeviceInfo.Name,
                    Version = DeviceInfo.Version.ToString()
                };
            else
                return new MediaSoup.Device
                {
                    Flag = "Blazor",
                    Name = "Browser",
                    Version = "1.0"
                };
        }
    }
}
