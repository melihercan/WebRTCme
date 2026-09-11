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

        // The screen, when one is being shared. Its own producer rather than a swapped track, so
        // peers receive it beside the camera instead of in place of it.
        Producer _shareProducer;
        DataProducer _chatDataProducer;
        DataProducer _botDataProducer;
        ConcurrentDictionary<string, PeerParameters> _peers = new();

        // Whether to ask the server for its own view of consumers and transports - see
        // LogServerStatsAsync. Off unless MediaSoupServer:LogServerStats says otherwise.
        bool _logServerStats;

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
            _logServerStats = _configuration.GetValue<bool>("MediaSoupServer:LogServerStats");

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
                                },
                                AppData = SourceAppData(MicSource)
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
                                encodings = SimulcastEncodingsFor(webcamTrack);
                            }
                        }

                        if (webcamTrack is not null)
                        {
                            _webcamProducer = await _sendTransport.ProduceAsync(new ProducerOptions
                            {
                                Track = webcamTrack,
                                Encodings = encodings ?? new RtpEncodingParameters[] { },
                                CodecOptions = codecOptions,
                                Codec = codec,
                                AppData = SourceAppData(WebcamSource)
                            });

                            LogNegotiatedEncodings(_webcamProducer, encodings);
                        }

                  _logger.LogInformation("Connection completed");

                        // Pausing and resuming these producers is SetOutgoingMediaEnabledAsync's
                        // job now; it used to sit here commented out, as a debugging attempt.
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
                    // The peer is nested under "peer", not the body itself. Deserialising the body
                    // as a Peer gave one with a null id, which OnNewPeer then discarded - so every
                    // peer that joined after this client did was never recorded, and its display
                    // name was never learned.
                    OnNewPeer(JsonSerializer.Deserialize<NewPeerNotification>(
                        element.GetRawText(), JsonHelper.WebRtcJsonSerializerOptions)?.Peer);
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
                        {
                            consumer.Pause();
                            ReportPeerMedia(consumerId);
                        }
                    }
                    break;

                case MethodName.ConsumerResumed:
                    {
                        var consumerId = GetString(element, "consumerId");
                        if (consumerId is not null && _consumers.TryGetValue(consumerId, out var consumer))
                        {
                            consumer.Resume();
                            ReportPeerMedia(consumerId);
                        }
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

                // The server's own estimate of how much it can send us, which is what decides
                // which simulcast layer each consumer gets. Logged rather than acted on: when a
                // remote picture is worse than the network should allow, this says whether the
                // server believes the path is narrow or whether something else is capping it.
                case MethodName.DownlinkBwe:
                    System.Diagnostics.Debug.WriteLine(
                        $"######## DownlinkBwe: {element.GetRawText()}");
                    break;

                // The layer actually in use, and the server's opinion of how well it is arriving.
                // Same reason: these are the two numbers that explain a blurry remote tile.
                case MethodName.ConsumerLayersChanged:
                case MethodName.ConsumerScore:
                    System.Diagnostics.Debug.WriteLine(
                        $"######## {method}: {element.GetRawText()}");
                    break;

                // Who the server's audio level observer currently hears, with each one's volume.
                // Appearing in the list at all is the signal: the observer only reports producers
                // above its own threshold, so membership already means "audible".
                case MethodName.SpeakingPeers:
                    {
                        var speaking = new HashSet<string>();
                        if (element.TryGetProperty("peerVolumes", out var volumes) &&
                            volumes.ValueKind == JsonValueKind.Array)
                        {
                            foreach (var entry in volumes.EnumerateArray())
                            {
                                var id = GetString(entry, "peerId");
                                if (id is not null)
                                    speaking.Add(id);
                            }
                        }
                        ReportSpeakingPeers(speaking);
                    }
                    break;

                // Not used, despite being the obvious candidate. In this server 'activeSpeaker'
                // carries a peer id only from the ActiveSpeakerObserver's 'dominantspeaker' event,
                // which never fired in testing - not for tones, not for continuous speech, not
                // even with a single audio producer in the room. What did arrive was the silence
                // case, which the server sends as { peerId: undefined } and which therefore looks
                // exactly like a dominant speaker whose appData is missing. 'speakingPeers' says
                // the same thing continuously, per peer, and with a volume attached.
                case MethodName.ActiveSpeaker:
                case MethodName.ProducerScore:
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
        /// Builds a simulcast ladder that suits the camera, rather than a fixed one.
        /// </summary>
        /// <remarks>
        /// The old ladder was three layers at 1/4, 1/2 and full size with a 5 Mbit/s top - the
        /// values mediasoup-demo uses, which assume a 720p or 1080p camera. Against a 640x480
        /// webcam they produce a 160x120 bottom rung nobody wants and a top rung that never runs:
        /// measured on Chrome, `r2` sat `active` with `framesEncoded: 0` for two minutes, with
        /// `qualityLimitationReason: "none"` and a megabit of spare estimated bandwidth. The
        /// allocator has to fund the lower layers' maxima before it reaches the top one, and
        /// 500k + 1M of those against a VGA source leaves the best layer permanently unfunded.
        ///
        /// Consumers then cannot do better than 320x240 however they ask, which is what made the
        /// remote picture blurry, and the server's shuffling between the two layers that do exist
        /// is what made the tile jump and freeze.
        ///
        /// So: three layers only when the source is big enough to have three worth sending, two
        /// otherwise, and a top bitrate matched to the resolution instead of a 1080p number.
        /// Unknown dimensions - a platform that does not fill in the track settings - fall back to
        /// the conservative pair rather than to the ladder that misbehaves.
        /// </remarks>
        static RtpEncodingParameters[] SimulcastEncodingsFor(IMediaStreamTrack track)
        {
            long height = 0;
            try
            {
                height = track?.GetSettings()?.Height ?? 0;
            }
            catch
            {
                // Not every platform reports settings, and none of this is worth failing a call
                // over: an unknown size just takes the two-layer ladder below.
            }

            if (height >= 720)
                return new RtpEncodingParameters[]
                {
                    new() { ScaleResolutionDownBy = 4, MaxBitrate = 500000 },
                    new() { ScaleResolutionDownBy = 2, MaxBitrate = 1200000 },
                    new() { ScaleResolutionDownBy = 1, MaxBitrate = 3000000 }
                };

            return new RtpEncodingParameters[]
            {
                new() { ScaleResolutionDownBy = 2, MaxBitrate = 400000 },
                new() { ScaleResolutionDownBy = 1, MaxBitrate = 1500000 }
            };
        }

        /// <summary>
        /// Prints the ladder that was asked for beside the one the encoder actually has.
        /// </summary>
        /// <remarks>
        /// Simulcast has produced two separate defects here already, and both were invisible until
        /// somebody compared the request against the result. The encoder is free to ignore, clamp
        /// or reorder what it is given, and it does so silently: the second bug showed up as a
        /// negotiated layer that reported no frames at all.
        ///
        /// Only for a real ladder, and only once per producer, so a single-encoding call stays
        /// quiet.
        /// </remarks>
        static void LogNegotiatedEncodings(Producer producer, RtpEncodingParameters[] requested)
        {
            if (producer is null || requested is null || requested.Length < 2)
                return;

            try
            {
                Echo("Simulcast asked for: " + string.Join(", ", requested.Select(encoding =>
                    $"scale={encoding.ScaleResolutionDownBy} max={encoding.MaxBitrate}")));

                var negotiated = producer.RtpSender?.GetParameters()?.Encodings;
                if (negotiated is null)
                {
                    Echo("Simulcast negotiated: the sender reported no encodings at all.");
                    return;
                }

                Echo("Simulcast negotiated: " + string.Join(", ", negotiated.Select(encoding =>
                    $"rid={encoding.Rid} active={encoding.Active} " +
                    $"scale={encoding.ScaleResolutionDownBy} max={encoding.MaxBitrate}")));
            }
            catch (Exception exception)
            {
                Echo($"Simulcast encodings could not be read: " +
                    $"{exception.GetType().Name}: {exception.Message}");
            }
        }

        // A ceiling rather than a fixed choice: the server clamps this to the top layer the
        // producer actually publishes, and still drops below it when it has to.
        const int PreferredSpatialLayer = 2;
        const int PreferredTemporalLayer = 2;

        /// <summary>
        /// Logs the server's own view of a consumer and of the receive transport, raw.
        /// </summary>
        /// <remarks>
        /// Off unless <c>MediaSoupServer:LogServerStats</c> is set, because it is two extra
        /// requests per sample and only useful while chasing something.
        ///
        /// It exists because every other measurement is taken on the client, which can only show
        /// what it sent and what it got. The numbers that actually explain the SFU's layer choice -
        /// the consumer's score, the layer being forwarded, and above all the transport's own
        /// outgoing bitrate estimate - live on the server. Reading them is what turned "the SFU
        /// chooses badly" into "the SFU's estimate has collapsed"; see the gaps document.
        ///
        /// Raw JSON deliberately. <c>GetConsumerStatsResponse</c> is an empty class, so there is
        /// nothing to deserialize into, and these shapes are worth reading rather than assumed -
        /// guessing at a response shape has already cost this project two separate bugs.
        /// </remarks>
        async Task LogServerStatsAsync(string consumerId)
        {
            if (!_logServerStats)
                return;

            try
            {
                var consumerStats = await _mediaSoupServerApi.ApiAsync(MethodName.GetConsumerStats,
                    new GetConsumerStatsRequest { ConsumerId = consumerId });
                if (consumerStats.IsOk)
                    Echo($"SERVER consumer {consumerId}: " +
                        $"{((JsonElement)consumerStats.Value).GetRawText()}");

                if (_recvTransport is not null)
                {
                    var transportStats = await _mediaSoupServerApi.ApiAsync(MethodName.GetTransportStats,
                        new GetTransportStatsRequest { TransportId = _recvTransport.Id });
                    if (transportStats.IsOk)
                        Echo($"SERVER recvTransport: " +
                            $"{((JsonElement)transportStats.Value).GetRawText()}");
                }
            }
            catch (Exception exception)
            {
                Echo($"SERVER stats failed: {exception.GetType().Name}: {exception.Message}");
            }
        }

        // Both, because neither reaches everywhere on its own: the MAUI apps register no logging
        // provider so ILogger goes nowhere, and a packaged WinUI app has no console.
        static void Echo(string line)
        {
            Console.WriteLine($"######## {line}");
            System.Diagnostics.Debug.WriteLine($"######## {line}");
        }

        /// <summary>
        /// Lifts a new consumer's ceiling to the top, so nothing here is what holds it down.
        /// </summary>
        /// <remarks>
        /// This was added believing it would fix the server parking consumers on spatial layer 0.
        /// It did not, and the reason is worth keeping: preferred layers are a <b>ceiling, not a
        /// floor</b>. Raising a ceiling the server was never touching changes nothing, and the
        /// measurements afterwards were identical - still parked on layer 0, still 64-159 kbit/s at
        /// 289x240 on a path carrying 1.8 Mbit/s the moment simulcast was switched off.
        ///
        /// It stays because it makes the default honest: every consumer starts free to use the
        /// best layer published, so whatever is capping them is demonstrably not this client.
        ///
        /// Audio has no spatial layers, so it is left alone.
        /// </remarks>
        Task RequestBestLayersAsync(Consumer consumer) =>
            SetPreferredLayersAsync(consumer, PreferredSpatialLayer, PreferredTemporalLayer);

        /// <summary>
        /// Tells the server which layers to forward for one consumer.
        /// </summary>
        /// <remarks>
        /// A notification, so there is no acknowledgement and a failure to send is the only failure
        /// reportable. Not fatal either way: the call carries on at whatever layer the server picks
        /// for itself, which is exactly what happened before any of this existed.
        /// </remarks>
        async Task SetPreferredLayersAsync(Consumer consumer, int spatialLayer, int temporalLayer)
        {
            if (consumer is null || consumer.Kind != MediaKind.Video)
                return;

            var result = await _mediaSoupServerApi.NotifyAsync(
                MethodName.SetConsumerPreferredLayers,
                new SetConsumerPreferredLayersRequest
                {
                    ConsumerId = consumer.Id,
                    SpatialLayer = spatialLayer,
                    TemporalLayer = temporalLayer
                });

            if (!result.IsOk)
                System.Diagnostics.Debug.WriteLine(
                    $"######## Preferred layers not set for consumer {consumer.Id}: {result.ErrorMessage}");
        }

        /// <summary>
        /// Reports what a peer is sending, after one of its consumers was paused or resumed.
        /// </summary>
        /// <remarks>
        /// This is the mediasoup equivalent of the peer-to-peer path's media message, and it
        /// arrives by a different route: there, a peer says it has muted; here, the server pauses
        /// the consumers carrying that producer and each client works it out from its own side.
        ///
        /// The report covers both kinds because the response does, and it is derived from every
        /// consumer of the peer rather than from the one that changed - the peer's other kind has
        /// not changed, but the caller is being handed a complete picture and it has to be right.
        /// A kind with no consumer at all counts as muted: nothing is arriving for it either way.
        /// </remarks>
        #region Sources

        // What a producer is a picture of, carried in its appData. The server copies 'source'
        // from the producer's appData onto every consumer it creates for it, so tagging here is
        // what lets the receiving end tell a camera apart from a shared screen - there is nothing
        // else in a consumer that distinguishes two video streams from the same peer.
        const string SourceKey = "source";
        const string MicSource = "mic";
        const string WebcamSource = "webcam";
        const string ScreenSource = "screen";

        // Microphone and camera are one tile; a shared screen is its own. Anything unrecognised,
        // including a peer running a client old enough not to tag its producers at all, falls in
        // with the camera - which is what this did before sources existed.
        const string CameraGroup = "camera";

        static Dictionary<string, object> SourceAppData(string source) =>
            new() { [SourceKey] = source };

        static string SourceGroupOf(Dictionary<string, object> appData) =>
            appData is not null &&
            appData.TryGetValue(SourceKey, out var value) &&
            value as string == ScreenSource
                ? ScreenSource
                : CameraGroup;

        /// <summary>
        /// The tile label for one of a peer's sources.
        /// </summary>
        /// <remarks>
        /// The label is the tile's identity all the way up: the view keys on it, and a peer with a
        /// camera and a screen needs two that do not collide. Suffixed rather than prefixed so the
        /// two sit together when anything sorts by name.
        /// </remarks>
        string LabelFor(string peerId, string sourceGroup) =>
            sourceGroup == ScreenSource
                ? $"{DisplayNameFor(peerId)} (screen)"
                : DisplayNameFor(peerId);

        Consumer[] ConsumersOf(PeerParameters peer, string sourceGroup) =>
            peer.ConsumerIds.ToArray()
                .Select(id => _consumers.TryGetValue(id, out var consumer) ? consumer : null)
                .Where(consumer => consumer is not null && SourceGroupOf(consumer.AppData) == sourceGroup)
                .ToArray();

        /// <summary>
        /// Reports one of a peer's sources as a stream of its own.
        /// </summary>
        /// <remarks>
        /// Called for every consumer as it arrives rather than once the set looks complete, and it
        /// re-reports the whole group each time. There is no message saying how many producers a
        /// peer has, so waiting for a particular shape means guessing: the old code waited for one
        /// audio and one video consumer and therefore never announced a peer that published only
        /// audio, and could never have announced a second video source at all.
        ///
        /// Re-announcing is safe because the label identifies the tile and the view replaces
        /// rather than appends. A peer with a camera and a microphone is announced twice, a
        /// fraction of a second apart, and the second announcement carries both tracks.
        /// </remarks>
        void AnnounceSourceGroup(string peerId, PeerParameters peer, string sourceGroup)
        {
            if (_connectionContext is null)
                return;

            var consumers = ConsumersOf(peer, sourceGroup);
            if (consumers.Length == 0)
                return;

            var mediaStream = _webRtc.Window(_jsRuntime).MediaStream();
            foreach (var consumer in consumers)
                mediaStream.AddTrack(consumer.Track);

            var label = LabelFor(peerId, sourceGroup);

            System.Diagnostics.Debug.WriteLine(
                $"<------- PeerJoined - tile:{label} tracks:{consumers.Length}");

            _connectionContext.Observer.OnNext(new PeerResponse
            {
                Type = PeerResponseType.PeerJoined,
                Id = peer.Id,
                Name = label,
                MediaStream = mediaStream
            });
        }

        /// <summary>
        /// Re-reports a source after one of its consumers went away, or withdraws it if that was
        /// the last one.
        /// </summary>
        /// <remarks>
        /// This is what makes a shared screen disappear when the sharer stops: the screen producer
        /// closes, its consumer closes with it, the group empties, and the tile goes. A peer that
        /// merely turns its camera off keeps its consumer - that is a pause, not a close - so it
        /// keeps its tile and reports the mute instead.
        /// </remarks>
        void SourceGroupChanged(Consumer consumer)
        {
            if (consumer is null || !TryGetPeerId(consumer.AppData, out var peerId))
                return;

            if (!_peers.TryGetValue(peerId, out var peer))
                return;

            var sourceGroup = SourceGroupOf(consumer.AppData);

            if (ConsumersOf(peer, sourceGroup).Length == 0)
                RetireSourceGroup(peer.Id, LabelFor(peerId, sourceGroup));
            else
                AnnounceSourceGroup(peerId, peer, sourceGroup);
        }

        /// <summary>
        /// Withdraws a source's tile, for a screen that stopped or a peer that left.
        /// </summary>
        void RetireSourceGroup(Guid peerRecordId, string label)
        {
            System.Diagnostics.Debug.WriteLine($"<------- PeerLeft - tile:{label}");

            _connectionContext?.Observer.OnNext(new PeerResponse
            {
                Type = PeerResponseType.PeerLeft,
                Id = peerRecordId,
                Name = label
            });
        }

        #endregion

        // The peers the server last reported as audible. Held rather than asked for: the
        // notification is a snapshot of who is speaking now, and knowing who *stopped* needs the
        // previous snapshot to compare against.
        HashSet<string> _speakingPeerIds = new();

        /// <summary>
        /// Reports the peers whose speaking state just changed.
        /// </summary>
        /// <remarks>
        /// Voice activity costs nothing to compute on this path: mediasoup runs an audio level
        /// observer and says who is audible, and this client had been discarding the notification
        /// outright. <see cref="MediaContext.Speaking"/> has been on the response all along,
        /// hardcoded false.
        ///
        /// Only the difference is reported. These arrive several times a second, and re-reporting
        /// every peer each time would push a stream of identical updates through to the UI for no
        /// change in what it shows.
        ///
        /// The local peer can be in the list too, and is ignored here: this reports on peers, and
        /// this client consumes no audio of its own.
        /// </remarks>
        void ReportSpeakingPeers(HashSet<string> speaking)
        {
            if (speaking.SetEquals(_speakingPeerIds))
                return;

            var changed = new HashSet<string>(_speakingPeerIds);
            changed.SymmetricExceptWith(speaking);
            _speakingPeerIds = speaking;

            foreach (var peerId in changed)
            {
                if (!_peers.TryGetValue(peerId, out var peer))
                    continue;

                // Any of the peer's consumers will do: ReportPeerMedia works back from a consumer
                // to its peer, and they all give the same answer.
                var consumerId = peer.ConsumerIds.ToArray().FirstOrDefault();
                if (consumerId is not null)
                    ReportPeerMedia(consumerId);
            }
        }

        void ReportPeerMedia(string consumerId)
        {
            if (_connectionContext is null)
                return;

            // Snapshotted before it is read twice: the list itself is not concurrent, and the
            // server can be adding a consumer to it while this runs. OnPeerClosed takes the same
            // precaution for the same reason.
            //
            // Taken as a pair rather than by value, because the key is needed: it is the peer id,
            // and it stands in for a display name that is routinely absent - see below.
            var entry = _peers.FirstOrDefault(
                p => p.Value.ConsumerIds.ToArray().Contains(consumerId));
            var peerId = entry.Key;
            var peer = entry.Value;
            if (peer is null)
                return;

            var consumers = peer.ConsumerIds.ToArray()
                .Select(id => _consumers.TryGetValue(id, out var consumer) ? consumer : null)
                .Where(consumer => consumer is not null)
                .ToArray();

            bool IsMuted(MediaKind kind)
            {
                var ofKind = consumers.Where(consumer => consumer.Kind == kind).ToArray();
                return ofKind.Length == 0 || ofKind.All(consumer => consumer.Paused);
            }

            var mediaContext = new MediaContext
            {
                VideoMuted = IsMuted(MediaKind.Video),
                AudioMuted = IsMuted(MediaKind.Audio),
                Speaking = _speakingPeerIds.Contains(peerId)
            };

            var name = DisplayNameFor(peerId);

            System.Diagnostics.Debug.WriteLine(
                $"<------- PeerMedia - peer:{name} " +
                $"videoMuted:{mediaContext.VideoMuted} audioMuted:{mediaContext.AudioMuted} " +
                $"speaking:{mediaContext.Speaking}");
            _logger.LogInformation(
                $"<------- PeerMedia - peer:{name} " +
                $"videoMuted:{mediaContext.VideoMuted} audioMuted:{mediaContext.AudioMuted} " +
                $"speaking:{mediaContext.Speaking}");

            _connectionContext.Observer.OnNext(new PeerResponse
            {
                Type = PeerResponseType.PeerMedia,
                Id = peer.Id,
                Name = name,
                MediaContext = mediaContext
            });
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

            // Which tiles this peer put on screen, read before its consumers are closed - closing
            // them is what makes the answer unavailable. A peer sharing a screen has two, and
            // withdrawing only one leaves the other behind for the rest of the call.
            //
            // The name comes from the record just removed rather than from DisplayNameFor, which
            // looks in _peers and would no longer find it.
            var name = peer.Peer?.DisplayName ?? peerId;
            var labels = peer.ConsumerIds.ToArray()
                .Select(id => _consumers.TryGetValue(id, out var consumer) ? consumer : null)
                .Where(consumer => consumer is not null)
                .Select(consumer => SourceGroupOf(consumer.AppData))
                .Distinct()
                .Select(sourceGroup => sourceGroup == ScreenSource ? $"{name} (screen)" : name)
                .ToArray();

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

            // A peer that never got as far as producing anything has no tiles and no labels, and
            // still has to be reported gone - it was announced by name when it joined.
            foreach (var label in labels.DefaultIfEmpty(name))
                RetireSourceGroup(peer.Id, label);
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
                    consumerRequestData.AppData = consumerRequestData.AppData.ToStringOrNumberOrBool();
                    foreach (var codec in consumerRequestData.RtpParameters.Codecs)
                        codec.Parameters = codec.Parameters.ToStringOrNumberOrBool();
                    foreach (var headerExtension in consumerRequestData.RtpParameters.HeaderExtensions)
                        headerExtension.Parameters = headerExtension.Parameters.ToStringOrNumberOrBool();


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

                    await RequestBestLayersAsync(consumer);

                    // For the layer investigation - see LogServerStatsAsync, which does nothing
                    // unless it is switched on. Video only, and fire-and-forget: this runs inside
                    // the newConsumer request handler, and waiting here would delay the accept().
                    if (_logServerStats && consumer.Kind == MediaKind.Video)
                    {
                        var consumerId = consumer.Id;
                        _ = Task.Run(async () =>
                        {
                            for (var sample = 0; sample < 6; sample++)
                            {
                                await Task.Delay(TimeSpan.FromSeconds(5));
                                await LogServerStatsAsync(consumerId);
                            }
                        });
                    }

                    Console.WriteLine($"~~~~~~~~~~~~~~~~~~~~~~~~~~~ NEW CONSUMER: before accept {consumerRequestData.Kind} {consumer.Kind}");
                    accept();
                    Console.WriteLine($"~~~~~~~~~~~~~~~~~~~~~~~~~~~ NEW CONSUMER: after accept {consumerRequestData.Kind} {consumer.Kind}");

                    // If audio-only mode is enabled, pause it.
                    ////if (consumer.Kind == MediaKind.Video && get 'audioOnly' from config)
                    ////consumer.Pause();


                    // Announce the source this consumer belongs to. Every consumer triggers this
                    // and the whole group is rebuilt each time, because nothing tells us how many
                    // producers a peer has - see AnnounceSourceGroup.
                    {
                        var consumerPeer = GetOrAddPeer(consumerRequestData.PeerId);
                        AnnounceSourceGroup(
                            consumerRequestData.PeerId,
                            consumerPeer,
                            SourceGroupOf(consumer.AppData));
                    }
                    break;

                    void Consumer_OnClose(object sender, EventArgs e)
                    {
                        _logger.LogInformation($"-------> Consumer_OnClose");
                        if (TryGetPeer(consumer.AppData, out var peer))
                            peer.ConsumerIds.Remove(consumer.Id);
                        _consumers.TryRemove(consumer.Id, out _);
                        SourceGroupChanged(consumer);
                    }

                    void Consumer_OnTransportClosed(object sender, EventArgs e)
                    {
                        _logger.LogInformation($"-------> Consumer_OnTransportClose");
                        if (TryGetPeer(consumer.AppData, out var peer))
                            peer.ConsumerIds.Remove(consumer.Id);
                        _consumers.TryRemove(consumer.Id, out _);
                        SourceGroupChanged(consumer);
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
                    dataConsumerRequestData.AppData = dataConsumerRequestData.AppData.ToStringOrNumberOrBool();

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
                                Name = DisplayNameFor(dataConsumerRequestData.PeerId),
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
        /// Statistics for what this client is sending, gathered from the producers themselves.
        /// </summary>
        /// <remarks>
        /// One report per producer, merged: the ids identify RTP streams within the send
        /// transport's peer connection, so they are distinct and nothing is overwritten. With
        /// simulcast each producer contributes one <c>outbound-rtp</c> entry per layer, which is
        /// what makes an unfunded layer visible - it reports <c>framesEncoded: 0</c> while its
        /// neighbours climb.
        ///
        /// A paused producer is still asked. Its counters standing still is the point: that is
        /// the evidence a mute reached the wire, which no receive-side report can show.
        /// </remarks>
        public async Task<IRTCStatsReport> GetOutgoingStatsAsync()
        {
            Dictionary<string, RTCStats> stats = new();

            foreach (var producer in new[] { _micProducer, _webcamProducer })
            {
                // A closed producer has no transceiver left to ask, and would throw.
                if (producer is null || producer.Closed)
                    continue;

                var report = await producer.GetStatsAsync();
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

            var producer = ProducerFor(track.Kind)
                ?? throw new InvalidOperationException(
                    $"This connection is not producing a {track.Kind} track to replace.");

            await producer.ReplaceTrackAsync(newTrack);
        }

        public bool IsOutgoingMediaEnabled(MediaStreamTrackKind kind)
        {
            // No producer means nothing is being sent, which is what a caller asking this wants to
            // know - it is not an error, because producing is optional and starts asynchronously.
            var producer = ProducerFor(kind);
            return producer is not null && !producer.Paused;
        }

        /// <summary>
        /// Pauses or resumes the producer, locally and on the server.
        /// </summary>
        /// <remarks>
        /// Both halves are needed and neither is enough. <see cref="Producer.Pause"/> only
        /// disables the local track, so the SFU keeps forwarding an RTP stream of silence to every
        /// consumer; the server request is what actually stops the forwarding and makes the other
        /// peers see the pause, through their own <c>consumerPaused</c> notification.
        ///
        /// The server is told first and the local producer changed afterwards. A notification is
        /// not acknowledged, so the only failure that can be reported is a failure to send, and
        /// pausing locally before that would leave this client believing it had muted while the
        /// SFU carried on forwarding.
        /// </remarks>
        public async Task SetOutgoingMediaEnabledAsync(MediaStreamTrackKind kind, bool enabled)
        {
            var producer = ProducerFor(kind)
                ?? throw new InvalidOperationException($"This connection is not producing {kind}.");

            // Already in the requested state, so there is nothing to say to the server.
            if (producer.Paused == !enabled)
                return;

            // Debug.WriteLine as well as the logger: the MAUI apps register no logging provider,
            // so ILogger output never reaches logcat or the Visual Studio output window.
            System.Diagnostics.Debug.WriteLine(
                $"######## Outgoing {kind} {(enabled ? "unmuted" : "muted")} - producer:{producer.Id}");
            _logger.LogInformation(
                $"######## Outgoing {kind} {(enabled ? "unmuted" : "muted")} - producer:{producer.Id}");

            // Notifications, not requests. The server dispatches these two through
            // handleProtooNotification, so sending them as requests is answered with "unknown
            // request method 'pauseProducer'" - which is what happened the first time this ran
            // against a real server, and reads like a version mismatch rather than a shape error.
            var result = enabled
                ? await _mediaSoupServerApi.NotifyAsync(MethodName.ResumeProducer,
                      new ResumeProducerRequest { ProducerId = producer.Id })
                : await _mediaSoupServerApi.NotifyAsync(MethodName.PauseProducer,
                      new PauseProducerRequest { ProducerId = producer.Id });

            if (!result.IsOk)
                throw new Exception(result.ErrorMessage);

            // Local state last: the server has no acknowledgement to give, so the only failure
            // that can be reported is a failure to send, and pausing locally before that would
            // leave this client muted while the SFU kept forwarding.
            if (enabled)
                producer.Resume();
            else
                producer.Pause();
        }

        /// <summary>
        /// Asks the server for the best layers to forward from one peer.
        /// </summary>
        /// <remarks>
        /// Applied to every video consumer that peer has, which today is one, but a peer producing
        /// both a camera and a screen has two and both are its own.
        /// </remarks>
        public async Task SetPreferredIncomingLayersAsync(Guid peerId, int spatialLayer, int temporalLayer)
        {
            if (spatialLayer < 0)
                throw new ArgumentOutOfRangeException(nameof(spatialLayer),
                    "Layers count from 0; there is nothing below the bottom one.");
            if (temporalLayer < 0)
                throw new ArgumentOutOfRangeException(nameof(temporalLayer),
                    "Layers count from 0; there is nothing below the bottom one.");

            var peer = _peers.Values.FirstOrDefault(peer => peer.Id == peerId)
                ?? throw new ArgumentException(
                    $"No peer with id {peerId} is in this connection.", nameof(peerId));

            foreach (var consumerId in peer.ConsumerIds.ToArray())
            {
                if (_consumers.TryGetValue(consumerId, out var consumer))
                    await SetPreferredLayersAsync(consumer, spatialLayer, temporalLayer);
            }
        }

        /// <summary>
        /// Caps which simulcast layers this client encodes, by switching the rest off.
        /// </summary>
        /// <remarks>
        /// Video only. Audio has no spatial layers, and a caller asking to cap them has
        /// misunderstood something rather than made a request worth honouring quietly.
        /// </remarks>
        public async Task SetMaxOutgoingSpatialLayerAsync(int spatialLayer)
        {
            if (spatialLayer < 0)
                throw new ArgumentOutOfRangeException(nameof(spatialLayer),
                    "Layers count from 0; capping below the bottom one would send nothing at all, " +
                    "which is what SetOutgoingMediaEnabledAsync is for.");

            var producer = _webcamProducer
                ?? throw new InvalidOperationException("This connection is not sending video.");

            await producer.SetMaxSpatialLayerAsync(spatialLayer);
        }

        /// <summary>
        /// Produces the screen as a source of its own, beside the camera.
        /// </summary>
        /// <remarks>
        /// A separate producer rather than a track swap, which is what lets a peer see the camera
        /// and the screen at once: the server copies this producer's <c>source</c> onto the
        /// consumers it creates, and the receiving end groups by it.
        ///
        /// No simulcast. A shared screen is mostly still, and the ladder here is built from a
        /// camera's frame height - neither the resolution nor the bitrates suit a desktop, and the
        /// bottom rung of a camera ladder makes text unreadable.
        /// </remarks>
        public async Task StartScreenShareAsync(IMediaStream displayStream)
        {
            var track = displayStream?.GetVideoTracks().FirstOrDefault()
                ?? throw new ArgumentException(
                    "The stream to share has no video track.", nameof(displayStream));

            var sendTransport = _sendTransport
                ?? throw new InvalidOperationException("There is no call to share into.");

            // Idempotent: a second start replaces what is being shared rather than producing
            // twice, which would put two screen tiles on every peer.
            await StopScreenShareAsync();

            _shareProducer = await sendTransport.ProduceAsync(new ProducerOptions
            {
                Track = track,
                Encodings = new RtpEncodingParameters[] { },
                CodecOptions = new ProducerCodecOptions { VideoGoogleStartBitrate = 1000 },
                AppData = SourceAppData(ScreenSource)
            });
        }

        public async Task StopScreenShareAsync()
        {
            var producer = _shareProducer;
            if (producer is null)
                return;

            _shareProducer = null;

            // Told to close, not just closed locally: the server is what stops forwarding it and
            // what makes the other peers' screen tiles go away.
            var result = await _mediaSoupServerApi.NotifyAsync(MethodName.CloseProducer,
                new CloseProducerRequest { ProducerId = producer.Id });

            if (!result.IsOk)
                System.Diagnostics.Debug.WriteLine(
                    $"######## Screen share not closed on the server: {result.ErrorMessage}");

            producer.Close();
        }

        /// <summary>
        /// Re-gathers ICE on both transports, recovering a connection whose path has died.
        /// </summary>
        /// <remarks>
        /// Each transport is restarted independently and both are attempted even if the first
        /// fails: they are separate ICE sessions, and a send path that has died does not imply the
        /// receive path has. Reporting only the first failure would also hide the more useful one.
        ///
        /// The server gathers new candidates and answers with its side's ICE parameters, which the
        /// handler needs before it can restart its own - hence a request rather than a
        /// notification.
        /// </remarks>
        public async Task RestartIceAsync()
        {
            var transports = new[] { _sendTransport, _recvTransport }.Where(t => t is not null).ToArray();
            if (transports.Length == 0)
                throw new InvalidOperationException("There is no call whose ICE could be restarted.");

            var failures = new List<string>();

            foreach (var transport in transports)
            {
                try
                {
                    var iceParameters = (IceParameters)ParseResponse(MethodName.RestartIce,
                        await _mediaSoupServerApi.ApiAsync(MethodName.RestartIce,
                            new RestartIceRequest { TransportId = transport.Id }));

                    await transport.RestartIceAsync(iceParameters);

                    System.Diagnostics.Debug.WriteLine(
                        $"######## ICE restarted on transport {transport.Id}");
                }
                catch (Exception exception)
                {
                    failures.Add($"{transport.Id}: {exception.Message}");
                }
            }

            if (failures.Count > 0)
                throw new Exception($"ICE restart failed on {string.Join("; ", failures)}");
        }

        Producer ProducerFor(MediaStreamTrackKind kind) => kind switch
        {
            MediaStreamTrackKind.Audio => _micProducer,
            MediaStreamTrackKind.Video => _webcamProducer,
            _ => null
        };

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
                        codec.Parameters = codec.Parameters.ToStringOrNumber();
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

                case MethodName.RestartIce:
                    // Nested under its own member, like the router capabilities above.
                    return JsonSerializer.Deserialize<RestartIceResponse>(
                        json, JsonHelper.WebRtcJsonSerializerOptions).IceParameters;

                case MethodName.Produce:
                    var produceResponse = JsonSerializer.Deserialize<ProduceResponse>(
                        json, JsonHelper.WebRtcJsonSerializerOptions);
                    return produceResponse.ProducerId;

                // PauseProducer, ResumeProducer, PauseConsumer and ResumeConsumer used to be
                // handled here. They are notifications, not requests, so they have no response to
                // parse and cannot reach this switch.

                case MethodName.ProduceData:
                    var produceDataResponse = JsonSerializer.Deserialize<ProduceDataResponse>(
                        json, JsonHelper.WebRtcJsonSerializerOptions);
                    return produceDataResponse.DataProducerId;

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

        /// <summary>
        /// The name to show for a peer, falling back to its id when the server has not named it.
        /// </summary>
        /// <remarks>
        /// The two used to be the same string, because this client joined with its display name as
        /// the peer id - so two people with one name collided on the server and the second
        /// displaced the first. They are separate now: the id is a GUID and the name travels in the
        /// join request, coming back on every peer the server describes.
        ///
        /// The fallback is therefore a real fallback rather than a synonym, and it is reachable: a
        /// peer record is created on demand when its consumers arrive, which can happen before the
        /// peer itself has been announced. Showing a GUID is poor, but it is better than showing
        /// nothing, and it is recognisably an id rather than a name someone might act on.
        /// </remarks>
        string DisplayNameFor(string peerId)
        {
            if (peerId is null)
                return null;

            return _peers.TryGetValue(peerId, out var peer) && peer.Peer?.DisplayName is { } name
                ? name
                : peerId;
        }

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
