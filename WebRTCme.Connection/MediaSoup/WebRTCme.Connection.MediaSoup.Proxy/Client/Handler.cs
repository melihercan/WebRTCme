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
                { MediaKind.Audio, new RtpParameters()},
                { MediaKind.Video, new RtpParameters()}
            };

        }
    }
}
