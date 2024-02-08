using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using WebRTCme.Connection.Signaling;

namespace WebRTCme.Connection.Services
{
    class SignalingConnection : IConnection, ISignalingServerNotify
    {
        readonly ISignalingServerApi _signalingServerApi;
        readonly IWebRtc _webRtc;
        readonly ILogger<SignalingConnection> _logger;
        readonly IJSRuntime _jsRuntime;

        ConnectionContext _connectionContext;// = new();

        public SignalingConnection(ISignalingServerApi signalingServerApi, IWebRtc webRtc, 
            ILogger<SignalingConnection> logger, IJSRuntime jsRuntime = null)
        {
            _signalingServerApi = signalingServerApi;
            _webRtc = webRtc;
            _logger = logger;
            _jsRuntime = jsRuntime;

            _signalingServerApi.PeerJoinedEventAsync += OnPeerJoinedAsync;
            _signalingServerApi.PeerLeftEventAsync += OnPeerLeftAsync;
            _signalingServerApi.PeerSdpEventAsync += OnPeerSdpAsync;
            _signalingServerApi.PeerIceEventAsync += OnPeerIceAsync;
            _signalingServerApi.PeerMediaEventAsync += OnPeerMediaAsync;

        }

        public IObservable<PeerResponse> ConnectionRequest(UserContext userContext)
        {
            return Observable.Create<PeerResponse>(async observer =>
            {
                bool isJoined = false;

                try
                {
                    // Do checks before creating connection context.
                    var result = await _signalingServerApi.JoinAsync(
                        userContext.Id,
                        userContext.Name,
                        userContext.Room);
                    if (!result.IsOk)
                        throw new Exception($"{result.ErrorMessage}");

                    _connectionContext = new ConnectionContext
                    {
                        UserContext = userContext,
                        Observer = observer,
                    };
                    isJoined = true;
                }
                catch (Exception ex)
                {
                    observer.OnError(ex);
                }

                return async () =>
                {
                    try
                    {
                        if (isJoined)
                            // No error handling for leave.
                            _ = await _signalingServerApi.LeaveAsync(userContext.Id);

                        if (_connectionContext is not null)
                        {
                            foreach (var peerContext in _connectionContext.PeerContexts)
                            {
                                peerContext.PeerConnection.Close();
                            }
                            _connectionContext = null;
                        }
                    }
                    catch { };
                };
            });
        }

        public ValueTask DisposeAsync()
        {
            _signalingServerApi.PeerJoinedEventAsync -= OnPeerJoinedAsync;
            _signalingServerApi.PeerLeftEventAsync -= OnPeerLeftAsync;
            _signalingServerApi.PeerSdpEventAsync -= OnPeerSdpAsync;
            _signalingServerApi.PeerIceEventAsync -= OnPeerIceAsync;
            _signalingServerApi.PeerMediaEventAsync -= OnPeerMediaAsync;
            return new ValueTask();
        }

        public Task ReplaceOutgoingTrackAsync(IMediaStreamTrack track, IMediaStreamTrack newTrack)
        {
            foreach (var peerContext in _connectionContext.PeerContexts)
            {
                var peerConnection = peerContext.PeerConnection;
                peerConnection
                    .GetSenders()
                    //.Where(sender => sender.Track.Kind == MediaStreamTrackKind.Video)
                    //.Select(sender => sender.ReplaceTrack(newVideoTrack));
                    .First(sender => sender.Track.Kind == track.Kind)
                    .ReplaceTrack(newTrack);
            }

            return Task.CompletedTask;
        }

        public Task<IRTCStatsReport> GetStats(Guid id)
        {
            throw new NotImplementedException();
        }


        public async Task OnPeerJoinedAsync(Guid peerId, string peerName)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine(
////        _logger.LogInformation(
                    $">>>>>>>> OnPeerJoined - room:{_connectionContext.UserContext.Room} " +
                    $"user:{_connectionContext.UserContext.Name} " +
                    $"peerUser:{peerName}");

                await CreateOrDeletePeerConnectionAsync(peerId, peerName, isInitiator: true);
                var peerContext = _connectionContext.PeerContexts.Single(context => context.Id.Equals(peerId));
                var peerConnection = peerContext.PeerConnection;

                var offerDescription = await peerConnection.CreateOffer();

                var sdp = JsonSerializer.Serialize(offerDescription, JsonHelper.WebRtcJsonSerializerOptions);
                System.Diagnostics.Debug.WriteLine(
////        _logger.LogInformation(
                    $"-------> Sending Offer - room:{_connectionContext.UserContext.Room} " +
                    $"user:{_connectionContext.UserContext.Name} " +
                    $"peerUser:{peerName}");// sdp:{offerDescription.Sdp}");

                await peerConnection.SetLocalDescription(offerDescription);

                var result = await _signalingServerApi.SdpAsync(peerId, sdp);
                if (!result.IsOk)
                    throw new Exception($"{result.ErrorMessage}");
            }
            catch (Exception ex)
            {
                _connectionContext?.Observer.OnNext(new PeerResponse
                {
                    Type = PeerResponseType.PeerError,
                    Id = peerId,
                    Name = peerName,
                    ErrorMessage = ex.Message
                });
            }

        }

        public async Task OnPeerLeftAsync(Guid peerId)
        {
            string peerName = string.Empty;
            try
            {
                var peerContext = _connectionContext.PeerContexts.Single(context => context.Id.Equals(peerId));
                peerName = peerContext.Name;
                await CreateOrDeletePeerConnectionAsync(peerId, peerName, isInitiator: peerContext.IsInitiator, 
                    isDelete: true);

                _connectionContext.Observer.OnNext(new PeerResponse
                {
                    Type = PeerResponseType.PeerLeft,
                    Id = peerId,
                    Name = peerName,
                });
            }
            catch (Exception ex)
            {
                _connectionContext?.Observer.OnNext(new PeerResponse
                {
                    Type = PeerResponseType.PeerError,
                    Id = peerId,
                    Name = peerName,
                    ErrorMessage = ex.Message
                });
            }
        }


        public async Task OnPeerSdpAsync(Guid peerId, string peerName, string peerSdp)
        {
            try
            {
                var peerContext = _connectionContext.PeerContexts.SingleOrDefault(context => context.Id.Equals(peerId));
                var description = JsonSerializer.Deserialize<RTCSessionDescriptionInit>(peerSdp,
                    JsonHelper.WebRtcJsonSerializerOptions);

                if (description.Type == RTCSdpType.Offer && peerContext is null)
                {
                    await CreateOrDeletePeerConnectionAsync(peerId, peerName, isInitiator: false);
                    peerContext = _connectionContext.PeerContexts.Single(context => context.Id.Equals(peerId));
                }
                var peerConnection = peerContext.PeerConnection;

                System.Diagnostics.Debug.WriteLine(
////        _logger.LogInformation(
                    $"<-------- OnPeerSdp - room:{_connectionContext.UserContext.Room} " +
                    $"user:{_connectionContext.UserContext.Name} " +
                    $"peerUser:{peerName}"); //peedSdp:{peerSdp}");

                await peerConnection.SetRemoteDescription(description);

                if (description.Type == RTCSdpType.Offer)
                {
                    var answerDescription = await peerConnection.CreateAnswer();

                    // Setting local description triggers ice candidate packets.
                    var sdp = JsonSerializer.Serialize(answerDescription, JsonHelper.WebRtcJsonSerializerOptions);
                    System.Diagnostics.Debug.WriteLine(
////                _logger.LogInformation(
                        $"-------> Sending Answer - room:{_connectionContext.UserContext.Room} " +
                        $"user:{_connectionContext.UserContext.Name}  " +
                        $"peerUser:{peerName}");// sdp:{answerDescription.Sdp}");

                    //_logger.LogInformation(
                    //    $"**** SetLocalDescription - turn:{turnServerName} room:{roomName} " +
                    //    $"user:{connectionContext.ConnectionRequestParameters.ConnectionParameters.UserName} " +
                    //    $"peerUser:{peerUserName}");
                    await peerConnection.SetLocalDescription(answerDescription);

                    var result = await _signalingServerApi.SdpAsync(peerId, sdp);
                    if (!result.IsOk)
                        throw new Exception($"{result.ErrorMessage}");
                }
            }
            catch (Exception ex)
            {
                _connectionContext?.Observer.OnNext(new PeerResponse
                {
                    Type = PeerResponseType.PeerError,
                    Id = peerId,
                    Name = peerName,
                    ErrorMessage = ex.Message
                });
            }

        }

        public async Task OnPeerIceAsync(Guid peerId, string peerIce)
        {
            string peerName = string.Empty;
            try
            {
                var peerContext = _connectionContext.PeerContexts.Single(context => context.Id.Equals(peerId));
                peerName = peerContext.Name;
                System.Diagnostics.Debug.WriteLine(
////        _logger.LogInformation(
                    $"<-------- OnPeerIceCandidate - room:{_connectionContext.UserContext.Room} " +
                    $"user:{_connectionContext.UserContext.Name} " +
                    $"peerUser:{peerName} " +
                    $"peerIce:{peerIce}");
                var peerConnection = peerContext.PeerConnection;

                var iceCandidate = JsonSerializer.Deserialize<RTCIceCandidateInit>(peerIce,
                    JsonHelper.WebRtcJsonSerializerOptions);
                await peerConnection.AddIceCandidate(iceCandidate);
            }
            catch (Exception ex)
            {
                _connectionContext?.Observer.OnNext(new PeerResponse
                {
                    Type = PeerResponseType.PeerError,
                    Id = peerId,
                    Name = peerName,
                    ErrorMessage = ex.Message
                });
            }

        }

        public Task OnPeerMediaAsync(Guid peerId, bool videoMuted, bool audioMuted, bool speaking)
        {
            throw new NotImplementedException();
        }

        async Task CreateOrDeletePeerConnectionAsync(Guid peerId, string peerName, bool isInitiator,  bool isDelete = false)
        {
            try
            {
                PeerContext peerContext = null;
                IRTCPeerConnection peerConnection = null;
                IMediaStream mediaStream = null;
                IRTCDataChannel dataChannel = null;

                if (isDelete)
                {
                    peerContext = _connectionContext.PeerContexts.Single(context => context.Id.Equals(peerId));
                    peerConnection = peerContext.PeerConnection;

                    peerConnection.OnConnectionStateChanged -= OnConnectionStateChanged;
                    peerConnection.OnDataChannel -= OnDataChannel;
                    peerConnection.OnIceCandidate -= OnIceCandidate;
                    peerConnection.OnIceConnectionStateChange -= OnIceConnectionStateChange;
                    peerConnection.OnIceGatheringStateChange -= OnIceGatheringStateChange;
                    peerConnection.OnNegotiationNeeded -= OnNegotiationNeeded;
                    peerConnection.OnSignallingStateChange -= OnSignallingStateChange;
                    peerConnection.OnTrack -= OnTrack;

                    // Remove local tracks and close.
                    var senders = peerConnection.GetSenders();
                    foreach (var sender in senders)
                        peerConnection.RemoveTrack(sender);
                    peerConnection.Close();

                    _connectionContext.PeerContexts.Remove(peerContext);
                }
                else
                {
                    mediaStream = _webRtc.Window(_jsRuntime).MediaStream();
                    RTCIceServer[] iceServers = _connectionContext.IceServers;
                    if (iceServers is null)
                    {
                        var result = await _signalingServerApi.GetIceServersAsync();
                        if (!result.IsOk)
                            throw new Exception($"{result.ErrorMessage}");
                        iceServers = result.Value;
                        _connectionContext.IceServers = iceServers;
                    }
                    var configuration = new RTCConfiguration
                    {
                        IceServers = iceServers,
                        SdpSemantics = SdpSemantics.UnifiedPlan
                        //PeerIdentity = peerName
                    };

                    _logger.LogInformation($"################ LIST OF ICE SERVERS ################");
                    foreach (var iceServer in configuration.IceServers)
                        foreach (var url in iceServer.Urls)
                            _logger.LogInformation($"\t - {url}");
                    _logger.LogInformation($"#####################################################");

                    peerConnection = _webRtc.Window(_jsRuntime).RTCPeerConnection(configuration);
                    peerContext = new PeerContext
                    {
                        Id = peerId,
                        Name = peerName,
                        PeerConnection = peerConnection,
                        IsInitiator = isInitiator,
                    };
                    _connectionContext.PeerContexts.Add(peerContext);

                    peerConnection.OnConnectionStateChanged += OnConnectionStateChanged;
                    peerConnection.OnDataChannel += OnDataChannel;
                    peerConnection.OnIceCandidate += OnIceCandidate;
                    peerConnection.OnIceConnectionStateChange += OnIceConnectionStateChange;
                    peerConnection.OnIceGatheringStateChange += OnIceGatheringStateChange;
                    peerConnection.OnNegotiationNeeded += OnNegotiationNeeded;
                    peerConnection.OnSignallingStateChange += OnSignallingStateChange;
                    peerConnection.OnTrack += OnTrack;


                    if (_connectionContext.UserContext.DataChannelName is not null && isInitiator)
                    {
                        dataChannel = peerConnection.CreateDataChannel(
                            _connectionContext.UserContext.DataChannelName,
                            new RTCDataChannelInit
                            {
                                Negotiated = false,
                            });
                    }

                    if (_connectionContext.UserContext.LocalStream is not null)
                    {
                        var videoTrack = _connectionContext.UserContext.LocalStream.GetVideoTracks().FirstOrDefault();
                        var audioTrack = _connectionContext.UserContext.LocalStream.GetAudioTracks().FirstOrDefault();
                        if (videoTrack is not null)
                            peerConnection.AddTrack(videoTrack, _connectionContext.UserContext.LocalStream);
                        if (audioTrack is not null)
                            peerConnection.AddTrack(audioTrack, _connectionContext.UserContext.LocalStream);
                    }
                }

                void OnConnectionStateChanged(object s, EventArgs e)
                {
                    System.Diagnostics.Debug.WriteLine(
                    ////_logger.LogInformation(
                        $"######## OnConnectionStateChanged - room:{_connectionContext.UserContext.Room} " +
                        $"user:{_connectionContext.UserContext.Name} " +
                        $"peerUser:{peerName} " +
                        $"connectionState:{peerConnection.ConnectionState}");
                    if (peerConnection.ConnectionState == RTCPeerConnectionState.Connected)
                        _connectionContext.Observer.OnNext(new PeerResponse
                        {
                            Type = PeerResponseType.PeerJoined,
                            Id = peerId,
                            Name = peerName,
                            MediaStream = mediaStream,
                            DataChannel = isInitiator ? dataChannel : null
                        });
                    //// WILL BE HANDLED BY PEER LEFT
                    //else if (peerConnection.ConnectionState == RTCPeerConnectionState.Disconnected)
                    //ConnectionResponseSubject.OnCompleted();
                }
                void OnDataChannel(object s, IRTCDataChannelEvent e)
                {
                    System.Diagnostics.Debug.WriteLine(
                    ////_logger.LogInformation(
                        $"######## OnDataChannel - room:{_connectionContext.UserContext.Room} " +
                        $"user:{_connectionContext.UserContext.Name} " +
                        $"peerUser:{peerName} " +
                        $"state:{e.Channel.ReadyState}");

                    dataChannel?.Close();
                    dataChannel?.Dispose();

                    dataChannel = e.Channel;
                    _connectionContext.Observer.OnNext(new PeerResponse
                    {
                        Type = PeerResponseType.PeerJoined,
                        Name = peerName,
                        MediaStream = null,
                        DataChannel = dataChannel
                    });
                }
                async void OnIceCandidate(object s, IRTCPeerConnectionIceEvent e)
                {
                    //_logger.LogInformation(
                    //    $"######## OnIceCandidate - room:{roomName} " +
                    //    $"user:{connectionContext.ConnectionRequestParameters.ConnectionParameters.UserName} " +
                    //    $"peerUser:{peerName}");

                    // 'null' is valid and indicates end of ICE gathering process.
                    if (e.Candidate is not null)
                    {
                        var iceCandidate = new RTCIceCandidateInit
                        {
                            Candidate = e.Candidate.Candidate,
                            SdpMid = e.Candidate.SdpMid,
                            SdpMLineIndex = e.Candidate.SdpMLineIndex,
                            //UsernameFragment = ???
                        };
                        var ice = JsonSerializer.Serialize(iceCandidate, JsonHelper.WebRtcJsonSerializerOptions);
                        System.Diagnostics.Debug.WriteLine(
                    ////_logger.LogInformation(
                            $"--------> Sending ICE Candidate - room:{_connectionContext.UserContext.Room} " +
                            $"user:{_connectionContext.UserContext.Name} " +
                            $"peerUser:{peerName} " +
                            $"ice:{ice}");
                        var result = await _signalingServerApi.IceAsync(peerId, ice);
                        if (!result.IsOk)
                            throw new Exception($"{result.ErrorMessage}");
                    }
                }
                void OnIceConnectionStateChange(object s, EventArgs e)
                {
                    System.Diagnostics.Debug.WriteLine(
////                _logger.LogInformation(
                        $"######## OnIceConnectionStateChange - room:{_connectionContext.UserContext.Room} " +
                        $"user:{_connectionContext.UserContext.Name} " +
                        $"peerUser:{peerName} " +
                        $"iceConnectionState:{peerConnection.IceConnectionState}");
                }
                void OnIceGatheringStateChange(object s, EventArgs e)
                {
                    _logger.LogInformation(
                        $"######## OnIceGatheringStateChange - room:{_connectionContext.UserContext.Room} " +
                        $"user:{_connectionContext.UserContext.Name} " +
                        $"peerUser:{peerName} " +
                        $"iceGatheringState: {peerConnection.IceGatheringState}");
                }
                void OnNegotiationNeeded(object s, EventArgs e)
                {
                    _logger.LogInformation(
                        $"######## OnNegotiationNeeded - room:{_connectionContext.UserContext.Room} " +
                        $"user:{_connectionContext.UserContext.Name} " +
                        $"peerUser:{peerName}");
                    // TODO: WHAT IF Not initiator adds track (which trigggers this event)???
                }
                void OnSignallingStateChange(object s, EventArgs e)
                {
                    System.Diagnostics.Debug.WriteLine(
////                _logger.LogInformation(
                        $"######## OnSignallingStateChange - room:{_connectionContext.UserContext.Room} " +
                        $"user:{_connectionContext.UserContext.Name} " +
                        $"peerUser:{peerName}, " +
                        $"signallingState:{ peerConnection.SignalingState }");
                    //RoomEventSubject.OnNext(new RoomEvent
                    //{
                    //    Code = RoomEventCode.PeerJoined,
                    //    RoomName = roomName,
                    //    PeerUserName = peerName,
                    //    MediaStream = mediaStream
                    //});
                }
                void OnTrack(object s, IRTCTrackEvent e)
                {
                    System.Diagnostics.Debug.WriteLine(
////                _logger.LogInformation(
                        $"######## OnTrack - room:{_connectionContext.UserContext.Room} " +
                        $"user:{_connectionContext.UserContext.Name} " +
                        $"peerUser:{peerName} " +
                        $"trackType:{e.Track.Kind}");
                    mediaStream.AddTrack(e.Track);
                }
            }
            catch (Exception ex)
            {
                _connectionContext?.Observer.OnNext(new PeerResponse
                {
                    Type = PeerResponseType.PeerError,
                    Id = peerId,
                    Name = peerName,
                    ErrorMessage = ex.Message
                });
            }
        }


    }
}
