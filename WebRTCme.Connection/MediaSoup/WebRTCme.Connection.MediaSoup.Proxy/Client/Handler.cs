using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebRTCme;
using Utilme.SdpTransform;
using WebRTCme.Connection.MediaSoup.Proxy.Client.Sdp;
using WebRTCme.Connection.MediaSoup.Proxy.Models;
using System.Linq;
using WebRTCme.Connection.MediaSoup.Proxy;

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
        IMediaStream _sendStream;
        bool _hasDataChannelMediaSection;
        int _nextSendSctpStreamId;
        bool _transportReady;


        NumSctpStreams NumSctpStreams = new() 
        {
            Os = 1024,
            Mis = 1024,
        };

        event EventHandler<ConnectionState> OnConnectionStateChange;
        event EventHandler<DtlsParameters> OnConnect;

        public Handler(Ortc ortc)
        {
            _ortc = ortc;
            Name = "Generic";
            _window = Registry.WebRtc.Window(Registry.JsRuntime);
        }


        public string Name { get; }

        public void Close()
        {
            _pc?.Close();
        }

        public void Dispose()
        {
        }

        public async Task<RtpCapabilities> GetNativeRtpCapabilitiesAsync()
        {
            IRTCPeerConnection pc = _window.RTCPeerConnection(new RTCConfiguration 
            { 
                IceTransportPolicy = RTCIceTransportPolicy.All,
                BundlePolicy = RTCBundlePolicy.Balanced,// .MaxBundle,
                RtcpMuxPolicy = RTCRtcpMuxPolicy.Require,
            });
            pc.AddTransceiver(MediaStreamTrackKind.Audio);
            pc.AddTransceiver(MediaStreamTrackKind.Video);
            var offer = await pc.CreateOffer();
            pc.Close();


      Console.WriteLine($"SDP string:{offer.Sdp}");

            var sdpObject = SdpSerializer.ReadSdp(Encoding.UTF8.GetBytes(offer.Sdp));
  SdpSerializer.DumpSdp(sdpObject);

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
            _remoteSdp = new(options.IceParameters, options.iceCandidates, options.DtlsParameters, 
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
                IceServers = options.RTCIceServers,
                IceTransportPolicy = options.IceTransportPolicy,
                BundlePolicy = RTCBundlePolicy.MaxBundle,
                RtcpMuxPolicy = RTCRtcpMuxPolicy.Require,
            });

            _pc.OnIceConnectionStateChange += (s, e) => 
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

                OnConnectionStateChange?.Invoke(this, connectionState);
            };

        }

        public void UpdateIceServers(RTCIceServer[] iceServers)
        {
            var configuration = _pc.GetConfiguration();
            configuration.IceServers = iceServers;
            _pc.SetConfiguration(configuration);
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

        public Task<IRTCStatsReport> GetTransportStatsAsync()
        {
            return _pc.GetStats();
        }

        public async Task<HandlerSendResult> SendAsync(HandlerSendOptions options)
        {
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

            // This may throw.
            sendingRtpParameters.Codecs = _ortc.ReduceCodecs(sendingRtpParameters.Codecs, options.Codec);

            var sendingRemoteRtpParameters = 
                Utils.Clone<RtpParameters>(
                    _sendingRemoteRtpParametersByKind[options.Track.Kind.ToMediaSoup()], default);

            // This may throw.
            sendingRemoteRtpParameters.Codecs = _ortc.ReduceCodecs(sendingRemoteRtpParameters.Codecs, options.Codec);

            var mediaSectionIdx = _remoteSdp.GetNextMediaSectionIdx();
            var transceiver = _pc.AddTransceiver(options.Track, new RTCRtpTransceiverInit
            {
                Direction = RTCRtpTransceiverDirection.Sendonly,
				Streams = new IMediaStream[] { _sendStream },
                SendEncodings = options.Encodings.Select(e => e.ToWebRtc()).ToArray()
            });

            var offer = await _pc.CreateOffer();
            var localSdpObject = SdpSerializer.ReadSdp(Encoding.UTF8.GetBytes(offer.Sdp));
            MediaObject offerMediaObject;

            if (!_transportReady)
                SetupTransport(DtlsRole.Server, localSdpObject);

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
                localSdpObject = SdpSerializer.ReadSdp(Encoding.UTF8.GetBytes(offer.Sdp));
                offerMediaObject = CommonUtils.SdpMediaDescriptionToMediaObject(
                    localSdpObject.MediaDescriptions[mediaSectionIdx.Idx]);

                UnifiedPlanUtils.AddLegacySimulcast(offerMediaObject, layers.SpatialLayers);

                offer = new RTCSessionDescriptionInit
                {
                    Type = RTCSdpType.Offer,
                    Sdp = Encoding.UTF8.GetString(SdpSerializer.WriteSdp(localSdpObject))
                };
            }

            Console.WriteLine($"send() | calling pc.setLocalDescription() {offer}");

            await _pc.SetLocalDescription(offer);

            // We can now get the transceiver.mid.
            var localId = transceiver.Mid;

            // Set MID.
            sendingRtpParameters.Mid = new Mid { Id = localId };

            localSdpObject = SdpSerializer.ReadSdp(Encoding.UTF8.GetBytes(_pc.LocalDescription.Sdp));
            offerMediaObject = CommonUtils.SdpMediaDescriptionToMediaObject(
                localSdpObject.MediaDescriptions[mediaSectionIdx.Idx]);

            // Set RTCP CNAME.
            sendingRtpParameters.Rtcp.Cname = CommonUtils.GetCname(offerMediaObject);

            // Set RTP encodings by parsing the SDP offer if no encodings are given.
            if (options.Encodings is not null)
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

            Console.WriteLine($"send() | calling pc.setRemoteDescription() {answer}");

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


        void  SetupTransport(DtlsRole localDtlsRole, Utilme.SdpTransform.Sdp localSdpObject)
        {
            if (localSdpObject is null)
                localSdpObject = SdpSerializer.ReadSdp(Encoding.UTF8.GetBytes(_pc.LocalDescription.Sdp));

            // Get our local DTLS parameters.
            var dtlsParameters = CommonUtils.ExtractDtlsParameters(localSdpObject);

            // Set our DTLS role.
            dtlsParameters.DtlsRole = localDtlsRole;

            // Update the remote DTLS role in the SDP.
            _remoteSdp.UpdateDtlsRole(localDtlsRole == DtlsRole.Client ? DtlsRole.Server : DtlsRole.Client);

            // Need to tell the remote transport about our parameters.
            OnConnect?.Invoke(this, dtlsParameters);

            _transportReady = true;
        }
    }

}
