using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebRTCme;
using UtilmeSdpTransform;
using WebRTCme.Connection.MediaSoup;
using Utilme.SdpTransform;
using WebRTCme.Connection.MediaSoup.Proxy.Client.Sdp;

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



    }


}
