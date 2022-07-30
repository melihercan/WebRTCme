using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Utilme.SdpTransform;
using WebRTCme.Connection.MediaSoup.Proxy.Client.Sdp;
using WebRTCme.Connection.MediaSoup.Proxy.Models;

namespace WebRTCme.Connection.MediaSoup.Proxy.Client
{
    public class Handler : IDisposable
    {
        readonly Ortc _ortc;
        IWindow _window;
        IRTCPeerConnection _pc;
        InternalDirection _direction;
        RemoteSdp _remoteSdp;
        Dictionary<MediaKind, RtpParameters> _sendingRtpParametersByKind;
        Dictionary<MediaKind, RtpParameters> _sendingRemoteRtpParametersByKind;
        Dictionary<string, IRTCRtpTransceiver> _mapMidTransceiver = new();
        //IMediaStream _sendStream;
        bool _hasDataChannelMediaSection;
        int _nextSendSctpStreamId;
        bool _transportReady;

        // SendAsync and SendDataChannelAsync have a race condition as they both add MediaSection to the list
        // kept in RemoteSdp as well as both setting offers in WebRTC. There is a call to GetNextMediaSectionIdx
        // to get index from list and accessing the offer from WebRTC using that index. When index is taken and
        // before the list is updated, if there comes another write to the list, things will be corrupted.
        // Use this semaphore to avoid race condition.
        // Also any send or receive operation should be atomic.
        static SemaphoreSlim _sem = new(1);

        static int _instanceNo; 

        NumSctpStreams NumSctpStreams = new() 
        {
            Os = 1024,
            Mis = 1024,
        };

        public event EventHandlerAsync<ConnectionState> OnConnectionStateChangeAsync;
        public event EventHandlerAsync<DtlsParameters> OnConnectAsync;

        public Handler(Ortc ortc)
        {
            int instanceNo = Interlocked.Increment(ref _instanceNo);

            _ortc = ortc;
            Name = $"Generic{instanceNo}";
            _window = Registry.WebRtc.Window(Registry.JsRuntime);
        }


        public string Name { get; }

        public void Close()
        {
            try
            {
                _pc?.Close();
            }
            catch { }
        }

        public void Dispose()
        {
            Close();
        }

        public async Task<RtpCapabilities> GetNativeRtpCapabilitiesAsync()
        {
            IRTCPeerConnection pc = _window.RTCPeerConnection(new RTCConfiguration 
            { 
                IceTransportPolicy = RTCIceTransportPolicy.All,
                BundlePolicy = RTCBundlePolicy.MaxBundle,
                RtcpMuxPolicy = RTCRtcpMuxPolicy.Require,
                SdpSemantics = SdpSemantics.UnifiedPlan
            });
            pc.AddTransceiver(MediaStreamTrackKind.Audio);
            pc.AddTransceiver(MediaStreamTrackKind.Video);
            var offer = await pc.CreateOffer();
            pc.Close();

////Console.WriteLine($"SDP string:{offer.Sdp}");
            var sdpObject = offer.Sdp.ToSdp();
//SdpSerializer.DumpSdp(sdpObject);

            ////var x = Encoding.UTF8.GetString(SdpSerializer.WriteSdp(sdpObject));

            var nativeRtpCapabilities = CommonUtils.ExtractRtpCapabilities(sdpObject);

            return nativeRtpCapabilities;
        }


        public Task<SctpCapabilities> GetNativeSctpCapabilitiesAsync()
        {
            return Task.FromResult(new SctpCapabilities 
            {
                NumStreams = NumSctpStreams
            });
        }


        public void Run(HandlerRunOptions options)
        {
            _direction = options.Direction;
            _remoteSdp = new(options.IceParameters, options.IceCandidates, options.DtlsParameters, 
                options.SctpParameters, null, null);

            _sendingRtpParametersByKind = new() 
            {
                { MediaKind.Audio, _ortc.GetSendingRtpParameters(MediaKind.Audio, options.ExtendedRtpCapabilities)},
                { MediaKind.Video, _ortc.GetSendingRtpParameters(MediaKind.Video, options.ExtendedRtpCapabilities) }
            };

            _sendingRemoteRtpParametersByKind = new()
            {
                { 
                    MediaKind.Audio, 
                    _ortc.GetSendingRemoteRtpParameters(MediaKind.Audio, options.ExtendedRtpCapabilities) 
                },
                { 
                    MediaKind.Video, 
                    _ortc.GetSendingRemoteRtpParameters(MediaKind.Video, options.ExtendedRtpCapabilities) 
                }
            };

            _pc = _window.RTCPeerConnection(new RTCConfiguration 
            { 
                IceServers = options.IceServers,
                IceTransportPolicy = options.IceTransportPolicy.HasValue ? options.IceTransportPolicy : RTCIceTransportPolicy.All,
                BundlePolicy = RTCBundlePolicy.MaxBundle,
                RtcpMuxPolicy = RTCRtcpMuxPolicy.Require,
                SdpSemantics = SdpSemantics.UnifiedPlan
            });

            _pc.OnIceConnectionStateChange += async (s, e) => 
            {
                ConnectionState connectionState = _pc.IceConnectionState switch
                {
                    RTCIceConnectionState.Checking => ConnectionState.Connecting,
                    RTCIceConnectionState.Connected => ConnectionState.Connected,
                    RTCIceConnectionState.Completed => ConnectionState.Connected,
                    RTCIceConnectionState.Failed => ConnectionState.Failed,
                    RTCIceConnectionState.Disconnected => ConnectionState.Disconnected,
                    RTCIceConnectionState.Closed => ConnectionState.Closed,
                    _ => throw new NotImplementedException(),
                };

                await OnConnectionStateChangeAsync?.Invoke(this, connectionState);
            };

        }

        public Task UpdateIceServersAsync(RTCIceServer[] iceServers)
        {
            var configuration = _pc.GetConfiguration();
            configuration.IceServers = iceServers;
            _pc.SetConfiguration(configuration);
            return Task.CompletedTask;
        }

        public async Task RestartIceAsync(IceParameters iceParameters)
        {
            // Provide the remote SDP handler with new remote ICE parameters.
            _remoteSdp.UpdateIceParameters(iceParameters);

            if (!_transportReady)
                return;

            if (_direction == InternalDirection.Send)
            {
                var offer = await _pc.CreateOffer(new RTCOfferOptions { IceRestart = true });
                await _pc.SetLocalDescription(offer);
                RTCSessionDescriptionInit answer = new() 
                {
                    Type = RTCSdpType.Answer,
                    Sdp = _remoteSdp.GetSdp()
                };
                await _pc.SetRemoteDescription(answer);
            }
            else
            {
                RTCSessionDescriptionInit offer = new()
                {
                    Type = RTCSdpType.Offer,
                    Sdp = _remoteSdp.GetSdp()
                };
                await _pc.SetRemoteDescription(offer);
                var answer = await _pc.CreateAnswer();
                await _pc.SetLocalDescription(answer);
            }

        }

        public Task</*****IRTCStatsReport****/string> GetTransportStatsAsync()
        {
            ////return _pc.GetStats();
            return _pc.GetStatsHack();
        }

        public async Task<HandlerSendResult> SendAsync(HandlerSendOptions options)
        {
            try
            {
                await _sem.WaitAsync();

                if (options.Encodings is not null && options.Encodings.Length > 1)
                {
                    options.Encodings = options.Encodings
                         .Select((encoding, idx) =>
                         {
                             encoding.Rid = $"r{idx}";
                             return encoding;
                         }).ToArray();
                }


                var sendingRtpParameters =
                    Utils.Clone<RtpParameters>(_sendingRtpParametersByKind[options.Track.Kind.ToMediaSoup()], default);
                foreach (var codec in sendingRtpParameters.Codecs)
                    codec.Parameters.ToStringOrNumber();

                // This may throw.
                sendingRtpParameters.Codecs = _ortc.ReduceCodecs(sendingRtpParameters.Codecs, options.Codec);

                var sendingRemoteRtpParameters =
                    Utils.Clone<RtpParameters>(
                        _sendingRemoteRtpParametersByKind[options.Track.Kind.ToMediaSoup()], default);
                foreach (var codec in sendingRemoteRtpParameters.Codecs)
                    codec.Parameters.ToStringOrNumber();

                // This may throw.
                sendingRemoteRtpParameters.Codecs = _ortc.ReduceCodecs(sendingRemoteRtpParameters.Codecs, options.Codec);

                var mediaSectionIdx = _remoteSdp.GetNextMediaSectionIdx();

                var transceiver = _pc.AddTransceiver(options.Track, new RTCRtpTransceiverInit
                {
                    Direction = RTCRtpTransceiverDirection.SendOnly,
                    Streams = new IMediaStream[] { _window.MediaStream() },
                    SendEncodings = options.Encodings?.Select(e => e.ToWebRtc()).ToArray()
                });

                var offer = await _pc.CreateOffer();
                var localSdpObject = offer.Sdp.ToSdp();
                MediaObject offerMediaObject;

                if (!_transportReady)
                    await SetupTransportAsync(DtlsRole.Server, localSdpObject);

                // Special case for VP9 with SVC.
                var hackVp9Svc = false;

                var layers = ScalabilityModes.Parse(options.Encodings?[0].ScalabilityMode);

                if (options.Encodings is not null &&
                    options.Encodings.Length == 1 &&
                    layers.SpatialLayers > 1 &&
                    sendingRtpParameters.Codecs[0].MimeType.ToLower() == "video/vp9")
                {
                    Console.WriteLine("send() | enabling legacy simulcast for VP9 SVC");

                    hackVp9Svc = true;
                    localSdpObject = offer.Sdp.ToSdp();
                    offerMediaObject = new MediaObject
                    {
                        MediaDescription = localSdpObject.MediaDescriptions[mediaSectionIdx.Idx]
                    };

                    UnifiedPlanUtils.AddLegacySimulcast(offerMediaObject, layers.SpatialLayers);

                    offer = new RTCSessionDescriptionInit
                    {
                        Type = RTCSdpType.Offer,
                        Sdp = localSdpObject.ToText()
                    };
                }

////      Console.WriteLine($"send() - {options.Track.Kind} | calling pc.setLocalDescription() offer: {offer.Sdp}");

                await _pc.SetLocalDescription(offer);

                // We can now get the transceiver.mid.
                var localId = transceiver.Mid;

                // Set MID.
                sendingRtpParameters.Mid = localId;//// new Mid { Id = localId };

                ////Console.WriteLine("===================");
                ////Console.WriteLine($"{_pc.LocalDescription.Sdp}");
                ////Console.WriteLine("===================");

////    if (options.Track.Kind == MediaStreamTrackKind.Video)
    ////       Console.WriteLine("PUT A BP HERE...");


                localSdpObject = _pc.LocalDescription.Sdp.ToSdp();
                offerMediaObject = new MediaObject
                {
                    MediaDescription = localSdpObject.MediaDescriptions[mediaSectionIdx.Idx]
                };

                // Set RTCP CNAME.
                sendingRtpParameters.Rtcp.Cname = CommonUtils.GetCname(offerMediaObject);

                // Set RTP encodings by parsing the SDP offer if no encodings are given.
                if (options.Encodings is null)
                {
                    sendingRtpParameters.Encodings = UnifiedPlanUtils.GetRtpEncodings(offerMediaObject);
                }
                // Set RTP encodings by parsing the SDP offer and complete them with given
                // one if just a single encoding has been given.
                else if (options.Encodings.Length == 1)
                {
                    var newEncodings = UnifiedPlanUtils.GetRtpEncodings(offerMediaObject);

                    newEncodings[0] = options.Encodings[0];

                    // Hack for VP9 SVC.
                    if (hackVp9Svc)
                        newEncodings = new RtpEncodingParameters[] { newEncodings[0] };

                    sendingRtpParameters.Encodings = newEncodings;
                }
                // Otherwise if more than 1 encoding are given use them verbatim.
                else
                {
                    sendingRtpParameters.Encodings = options.Encodings;
                }

                // If VP8 or H264 and there is effective simulcast, add scalabilityMode to
                // each encoding.
                if (sendingRtpParameters.Encodings.Length > 1 &&
                    (sendingRtpParameters.Codecs[0].MimeType.ToLower() == "video/vp8" ||
                     sendingRtpParameters.Codecs[0].MimeType.ToLower() == "video/h264"))
                {
                    foreach (var encoding in sendingRtpParameters.Encodings)
                    {
                        encoding.ScalabilityMode = "S1T3";
                    }
                }

                _remoteSdp.Send(offerMediaObject, mediaSectionIdx.ReuseMid, sendingRtpParameters,
                    sendingRemoteRtpParameters, options.CodecOptions, true);

                var answer = new RTCSessionDescriptionInit
                {
                    Type = RTCSdpType.Answer,
                    Sdp = _remoteSdp.GetSdp()
                };

////          Console.WriteLine($"send() {options.Track.Kind} | calling pc.setRemoteDescription() answer: {answer.Sdp}");

                await _pc.SetRemoteDescription(answer);

                // Store in the map.
                _mapMidTransceiver.Add(localId, transceiver);

                return new HandlerSendResult
                {
                    LocalId = localId,
                    RtpParameters = sendingRtpParameters,
                    RtpSender = transceiver.Sender
                };
            }
            finally
            {
                _sem.Release();
            }

        }

        public async Task<HandlerSendDataChannelResult> SendDataChannelAsync(HandlerSendDataChannelOptions options_)
        {
            try
            {
                await _sem.WaitAsync();

                RTCDataChannelInit options = new()
                {
                    Ordered = options_.Ordered,
                    MaxPacketLifeTime = (ushort?)options_.MaxPacketLifeTime,
                    MaxRetransmits = (ushort?)options_.MaxRetransmits,
                    Protocol = options_.Protocol,
                    Negotiated = true,
                    Id = (short?)_nextSendSctpStreamId
                };

                var dataChannel = _pc.CreateDataChannel(options_.Label, options);

                // Increase next id.
                _nextSendSctpStreamId = ++_nextSendSctpStreamId % NumSctpStreams.Mis;

                // If this is the first DataChannel we need to create the SDP answer with
                // m=application section.
                if (!_hasDataChannelMediaSection)
                {
                    var offer = await _pc.CreateOffer();
                    var localSdpObject = offer.Sdp.ToSdp();
                    MediaObject offerMediaObject = new()
                    {
                        MediaDescription = localSdpObject.MediaDescriptions
                            .Single(md => md.Media == MediaType.Application)
                    };

                    if (!_transportReady)
                        await SetupTransportAsync(DtlsRole.Server, localSdpObject);

                    await _pc.SetLocalDescription(offer);
                    _remoteSdp.SendSctpAssociation(offerMediaObject);

                    RTCSessionDescriptionInit answer = new()
                    {
                        Type = RTCSdpType.Answer,
                        Sdp = _remoteSdp.GetSdp()
                    };
                    await _pc.SetRemoteDescription(answer);
                    _hasDataChannelMediaSection = true;
                }

                SctpStreamParameters sctpStreamParameters = new()
                {
                    StreamId = options.Id,
                    Ordered = options.Ordered,
                    MaxPacketLifeTime = options.MaxPacketLifeTime,
                    MaxRetransmits = options.MaxRetransmits
                };

                return new HandlerSendDataChannelResult
                {
                    DataChannel = dataChannel,
                    SctpStreamParameters = sctpStreamParameters
                };
            }
            finally
            {
                _sem.Release();
            }
        }

        public async Task StopSendingAsync(string localId)
        {
            Console.WriteLine($"stopSending() localId:{localId}");

            if (!_mapMidTransceiver.ContainsKey(localId))
                throw new Exception("associated RTCRtpTransceiver not found");

            var transceiver = _mapMidTransceiver[localId];
            await transceiver.Sender.ReplaceTrack(null);
            _pc.RemoveTrack(transceiver.Sender);
            _remoteSdp.CloseMediaSection(transceiver.Mid);

            var offer = await _pc.CreateOffer();
            await _pc.SetLocalDescription(offer);
            RTCSessionDescriptionInit answer = new()
            {
                Type = RTCSdpType.Answer, 
                Sdp = _remoteSdp.GetSdp() 
            };
		    await _pc.SetRemoteDescription(answer);
        }




        public async Task ReplaceTrackAsync(string localId, IMediaStreamTrack track = null)
	    {
		    if (track is not null)
		    {
			    Console.WriteLine($"replaceTrack() localId:{localId}, track.id:{track.Id}");
		    }
		    else
		    {
                Console.WriteLine($"replaceTrack() localId:{localId}, no track");
		    }

            if (!_mapMidTransceiver.ContainsKey(localId))
                throw new Exception("associated RTCRtpTransceiver not found");

            var transceiver = _mapMidTransceiver[localId];
            await transceiver.Sender.ReplaceTrack(track);
	    }

        public async Task SetMaxSpatialLayerAsync(string localId, int spatialLayer)
        {
            Console.WriteLine($"setMaxSpatialLayer() localId:{localId}, spatialLayer:{spatialLayer}");

            if (!_mapMidTransceiver.ContainsKey(localId))
                throw new Exception("associated RTCRtpTransceiver not found");
            
            var transceiver = _mapMidTransceiver[localId];
            var parameters = transceiver.Sender.GetParameters();
            parameters.Encodings = parameters.Encodings
                .Select((encoding, idx) =>
                {
                    if (idx <= spatialLayer)
                        encoding.Active = true;
                    else
                        encoding.Active = false;
                    return encoding;
                })
                .ToArray();

            await transceiver.Sender.SetParameters(parameters);
        }

        public async Task SetRtpEncodingParametersAsync(string localId, object params_)
        {
            if (!_mapMidTransceiver.ContainsKey(localId))
                throw new Exception("associated RTCRtpTransceiver not found");

            var transceiver = _mapMidTransceiver[localId];
            var parameters = transceiver.Sender.GetParameters();
            parameters.Encodings = parameters.Encodings
                .Select((encoding, idx) =>
                {
                    //// TODO: CHECK WHAT TO DO WITH params_
                    return encoding;
                })
                .ToArray();

            await transceiver.Sender.SetParameters(parameters);
        }

        public async Task<IRTCStatsReport> GetSenderStatsAsync(string localId)
        {
            var transceiver = _mapMidTransceiver[localId];
            if (transceiver is null)
                throw new Exception("associated RTCRtpTransceiver not found");

            return await transceiver.Sender.GetStats();
        }


        public async Task<HandlerReceiveResult> ReceiveAsync(HandlerReceiveOptions options,     Transport transport = null)
        {
            try
            {
                await _sem.WaitAsync();

                var localId = options.RtpParameters.Mid/****?.Id****/ ?? _mapMidTransceiver.Count.ToString();

                _remoteSdp.Receive(
                    localId,
                    options.Kind,
                    options.RtpParameters,
                    options.RtpParameters.Rtcp.Cname,
                    options.TrackId);

                RTCSessionDescriptionInit offer = new()
                {
                    Type = RTCSdpType.Offer,
                    Sdp = _remoteSdp.GetSdp()
                };
        ////Console.WriteLine($"receive() - {options.Kind} | calling pc.setRemoteDescription() offer: {offer.Sdp}");

                await _pc.SetRemoteDescription(offer);

                var answer = await _pc.CreateAnswer();
                var localSdpObject = answer.Sdp.ToSdp();

                MediaObject answerMediaObject = new()
                {
                    MediaDescription = localSdpObject.MediaDescriptions
                        .Single(md => md.Attributes.Mid.Id == localId)
                };

                // May need to modify codec parameters in the answer based on codec
                // parameters in the offer.
                CommonUtils.ApplyCodecParameters(options.RtpParameters, answerMediaObject);
                answer = new RTCSessionDescriptionInit
                {
                    Type = RTCSdpType.Answer,
                    Sdp = localSdpObject.ToText()
                };

                if (!_transportReady)
                    await SetupTransportAsync(DtlsRole.Client, localSdpObject);

            ////Console.WriteLine($"receive() - {options.Kind} | calling pc.setLocalDescription() answer: {answer.Sdp}");
                await _pc.SetLocalDescription(answer);

                var transceivers = _pc.GetTransceivers();


                var transceiver = transceivers //_pc.GetTransceivers()
                    .FirstOrDefault(t => t.Mid == localId);

                if (transceiver is null)
                    throw new Exception("new RTCRtpTransceiver not found");

                // Store in the map.
                _mapMidTransceiver.Add(localId, transceiver);

                return new HandlerReceiveResult
                {
                    LocalId = localId,
                    Track = transceiver.Receiver.Track,
                    RtpReceiver = transceiver.Receiver
                };
            }
            finally
            {
                _sem.Release();
            }
        }

        public async Task StopReceivingAsync(string localId)
        {
            if (!_mapMidTransceiver.ContainsKey(localId))
                throw new Exception("associated RTCRtpTransceiver not found");

            var transceiver = _mapMidTransceiver[localId];
            _remoteSdp.CloseMediaSection(transceiver.Mid);

            RTCSessionDescriptionInit offer = new()
            {
                Type = RTCSdpType.Offer, 
                Sdp = _remoteSdp.GetSdp() 
            };

		    await _pc.SetRemoteDescription(offer);
            var answer = await _pc.CreateAnswer();
		    await _pc.SetLocalDescription(answer);
        }

        public async Task<IRTCStatsReport> GetReceiverStatsAsync(string localId)
        {
            var transceiver = _mapMidTransceiver[localId];
            if (transceiver is null)
                throw new Exception("associated RTCRtpTransceiver not found");

            return await transceiver.Receiver.GetStats();
        }

        public async Task<HandlerReceiveDataChannelResult> ReceiveDataChannelAsync(HandlerReceiveDataChannelOptions
            options_)
        {
            try
            {
                await _sem.WaitAsync();

                RTCDataChannelInit options = new()
                {
                    Ordered = options_.SctpStreamParameters.Ordered,
                    MaxPacketLifeTime = (ushort?)options_.SctpStreamParameters.MaxPacketLifeTime,
                    MaxRetransmits = (ushort?)options_.SctpStreamParameters.MaxRetransmits,
                    Protocol = options_.Protocol,
                    Negotiated = true,
                    Id = (short?)options_.SctpStreamParameters.StreamId
                };

                var dataChannel = _pc.CreateDataChannel(options_.Label, options);

                // If this is the first DataChannel we need to create the SDP offer with
                // m=application section.
                if (!_hasDataChannelMediaSection)
                {
                    _remoteSdp.ReceiveSctpAssociation();

                    RTCSessionDescriptionInit offer = new()
                    {
                        Type = RTCSdpType.Offer,
                        Sdp = _remoteSdp.GetSdp()
                    };

                    ////Console.WriteLine($"OFFER:{offer.Sdp}");

                    await _pc.SetRemoteDescription(offer);
                    var answer = await _pc.CreateAnswer();

                    if (!_transportReady)
                    {
                        var localSdpObject = answer.Sdp.ToSdp();
                        await SetupTransportAsync(DtlsRole.Client, localSdpObject);
                    }

                    await _pc.SetLocalDescription(answer);
                    _hasDataChannelMediaSection = true;
                }

                return new HandlerReceiveDataChannelResult
                {
                    DataChannel = dataChannel
                };
            }
            finally
            {
                _sem.Release();
            }
        }

        async Task SetupTransportAsync(DtlsRole localDtlsRole, Utilme.SdpTransform.Sdp localSdpObject)
        {
            if (localSdpObject is null)
                localSdpObject = _pc.LocalDescription.Sdp.ToSdp();

            // Get our local DTLS parameters.
            var dtlsParameters = CommonUtils.ExtractDtlsParameters(localSdpObject);

            // Set our DTLS role.
            dtlsParameters.Role = localDtlsRole;

            // Update the remote DTLS role in the SDP.
            _remoteSdp.UpdateDtlsRole(localDtlsRole == DtlsRole.Client ? DtlsRole.Server : DtlsRole.Client);

            // Need to tell the remote transport about our parameters.
            await OnConnectAsync?.Invoke(this, dtlsParameters);

            _transportReady = true;
        }
    }

}
