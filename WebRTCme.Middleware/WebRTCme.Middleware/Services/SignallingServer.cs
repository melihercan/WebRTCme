using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using WebRTCme;
using WebRTCme.Middleware;
using WebRTCme.SignallingServerProxy;
using WebRTCme.Middleware.Models;
using Xamarin.Essentials;

namespace WebRTCme.Middleware.Services
{
    internal class SignallingServer : ISignallingServer, ISignallingServerCallbacks
    {
        private readonly IWebRtcConnection _webRtcConnection;
        private readonly IJSRuntime _jsRuntime;
        private readonly ILogger<SignallingServer> _logger;
        private readonly string _signallingServerBaseUrl;

        //// TODO: FIND ANOTHER SOLUTION TO PASS THIS TO WebRtcConnection.
        private readonly ISignallingServerProxy SignallingServerProxy;
//        private static List<ConnectionContext> _connectionContexts = new();

        public SignallingServer(IWebRtcConnection webRtcConnection, IConfiguration configuration, 
            ILogger<SignallingServer> logger,
            IJSRuntime jsRuntime = null)
        {
            _webRtcConnection = webRtcConnection;
            _signallingServerBaseUrl = configuration["SignallingServer:BaseUrl"];
            _logger = logger;
            _jsRuntime = jsRuntime;
            SignallingServerProxy = new SignallingServerProxy.SignallingServerProxy(configuration/*_signallingServerBaseUrl*/, this);
            _webRtcConnection.SignallingServerProxy = SignallingServerProxy;
        }

        public async Task<string[]> GetTurnServerNamesAsync()
        {
            var result = await SignallingServerProxy.GetTurnServerNamesAsync();
            if (result.Status != Ardalis.Result.ResultStatus.Ok)
                throw new Exception(string.Join("-", result.Errors.ToArray()));
            return result.Value;
        }



        public async ValueTask DisposeAsync()
        {
            await SignallingServerProxy.DisposeAsync();
        }


        #region SignallingServerCallbacks

        public async Task OnPeerJoinedAsync(string turnServerName, string roomName, string peerUserName) 
        {
            Subject<PeerResponseParameters> subject = null;
            try
            {
                var connectionContext =  _webRtcConnection.GetConnectionContext(turnServerName, roomName);
                _logger.LogInformation(
                    $">>>>>>>> OnPeerJoined - turn:{turnServerName} room:{roomName} " +
                    $"user:{connectionContext.ConnectionRequestParameters.ConnectionParameters.UserName} " +
                    $"peerUser:{peerUserName}");

                await _webRtcConnection.CreateOrDeletePeerConnectionAsync(turnServerName, roomName, peerUserName, isInitiator: true);
                var peerContext = connectionContext.PeerContexts
                    .Single(context => context.PeerParameters.PeerUserName
                    .Equals(peerUserName, StringComparison.OrdinalIgnoreCase));
                var peerConnection = peerContext.PeerConnection;
                subject = peerContext.PeerResponseSubject;

                var offerDescription = await peerConnection.CreateOffer();
                // Android DOES NOT expose 'Type'!!! I set it manually here. 
                if (DeviceInfo.Platform == DevicePlatform.Android)
                    offerDescription.Type = RTCSdpType.Offer;

                // Send offer before setting local description to avoid race condition with ice candidates.
                // Setting local description triggers ice candidate packets.
                var sdp = JsonSerializer.Serialize(offerDescription, JsonHelper.WebRtcJsonSerializerOptions);
                _logger.LogInformation(
                    $"-------> Sending Offer - room:{roomName} " +
                    $"user:{connectionContext.ConnectionRequestParameters.ConnectionParameters.UserName} " +
                    $"peerUser:{peerUserName}");// sdp:{offerDescription.Sdp}");

                //_logger.LogInformation(
                //    $"**** SetLocalDescription - turn:{turnServerName} room:{roomName} " +
                //    $"user:{connectionContext.ConnectionRequestParameters.ConnectionParameters.UserName} " +
                //    $"peerUser:{peerUserName}");
                await peerConnection.SetLocalDescription(offerDescription);

                await SignallingServerProxy.SdpAsync(turnServerName, roomName, peerUserName, sdp);

            }
            catch (Exception ex)
            {
                subject?.OnNext(new PeerResponseParameters 
                { 
                    Code = PeerResponseCode.PeerError,
                    TurnServerName = turnServerName,
                    RoomName = roomName,
                    PeerUserName = peerUserName,
                    ErrorMessage = ex.Message
                });
            }
        }

        public async Task OnPeerLeftAsync(string turnServerName, string roomName, string peerUserName)
        {
            Subject<PeerResponseParameters> subject = null;
            try
            {
                var connectionContext = _webRtcConnection.GetConnectionContext(turnServerName, roomName);
                //_logger.LogInformation(
                //    $">>>>>>>> OnPeerLeft - turn:{turnServerName} room:{roomName} " +
                //    $"user:{connectionContext.ConnectionRequestParameters.ConnectionParameters.UserName} " +
                //    $"peerUser:{peerUserName}");
                var peerContext = connectionContext.PeerContexts
                    .Single(context => context.PeerParameters.PeerUserName
                    .Equals(peerUserName, StringComparison.OrdinalIgnoreCase));
                subject = peerContext.PeerResponseSubject;

                await _webRtcConnection.CreateOrDeletePeerConnectionAsync(turnServerName, roomName, peerUserName,
                    isInitiator: peerContext.IsInitiator, isDelete: true);

                subject.OnNext(new PeerResponseParameters 
                {
                    Code = PeerResponseCode.PeerLeft,
                    TurnServerName = turnServerName,
                    RoomName = roomName,
                    PeerUserName = peerUserName,
                });
            }
            catch (Exception ex)
            {
                subject?.OnNext(new PeerResponseParameters
                {
                    Code = PeerResponseCode.PeerError,
                    TurnServerName = turnServerName,
                    RoomName = roomName,
                    PeerUserName = peerUserName,
                    ErrorMessage = ex.Message
                });
            }
        }

        public async Task OnPeerSdpAsync(string turnServerName, string roomName, string peerUserName, string peerSdp)
        {
            Subject<PeerResponseParameters> subject = null;
            try
            {
                var connectionContext = _webRtcConnection.GetConnectionContext(turnServerName, roomName);
                _logger.LogInformation(
                    $"<-------- OnPeerSdp - turn:{turnServerName} room:{roomName} " +
                    $"user:{connectionContext.ConnectionRequestParameters.ConnectionParameters.UserName} " +
                    $"peerUser:{peerUserName}"); //peedSdp:{peerSdp}");

                var description = JsonSerializer.Deserialize<RTCSessionDescriptionInit>(peerSdp,
                    JsonHelper.WebRtcJsonSerializerOptions);

                var peerContext = connectionContext.PeerContexts
                    .FirstOrDefault(context => context.PeerParameters.PeerUserName
                    .Equals(peerUserName, StringComparison.OrdinalIgnoreCase));
                if (description.Type == RTCSdpType.Offer &&  peerContext is null)
                {
                    await _webRtcConnection.CreateOrDeletePeerConnectionAsync(turnServerName, roomName, peerUserName, isInitiator: false);
                    peerContext = connectionContext.PeerContexts
                        .Single(context => context.PeerParameters.PeerUserName
                        .Equals(peerUserName, StringComparison.OrdinalIgnoreCase));
                }
                var peerConnection = peerContext.PeerConnection;
                subject = peerContext.PeerResponseSubject;

                //_logger.LogInformation(
                //    $"**** SetRemoteDescription - turn:{turnServerName} room:{roomName} " +
                //    $"user:{connectionContext.ConnectionRequestParameters.ConnectionParameters.UserName} " +
                //    $"peerUser:{peerUserName}");
                await peerConnection.SetRemoteDescription(description);

                if (description.Type == RTCSdpType.Offer)
                {
                    var answerDescription = await peerConnection.CreateAnswer();
                    // Android DOES NOT expose 'Type'!!! I set it manually here. 
                    if (DeviceInfo.Platform == DevicePlatform.Android)
                        answerDescription.Type = RTCSdpType.Answer;

                    // Send offer before setting local description to avoid race condition with ice candidates.
                    // Setting local description triggers ice candidate packets.
                    var sdp = JsonSerializer.Serialize(answerDescription, JsonHelper.WebRtcJsonSerializerOptions);
                    _logger.LogInformation(
                        $"-------> Sending Answer - room:{roomName} " +
                        $"user:{connectionContext.ConnectionRequestParameters.ConnectionParameters.UserName}  " +
                        $"peerUser:{peerUserName}");// sdp:{answerDescription.Sdp}");

                    //_logger.LogInformation(
                    //    $"**** SetLocalDescription - turn:{turnServerName} room:{roomName} " +
                    //    $"user:{connectionContext.ConnectionRequestParameters.ConnectionParameters.UserName} " +
                    //    $"peerUser:{peerUserName}");
                    await peerConnection.SetLocalDescription(answerDescription);
                    await SignallingServerProxy.SdpAsync(turnServerName, roomName, peerUserName, sdp);
                }
            }
            catch (Exception ex)
            {
                subject?.OnNext(new PeerResponseParameters
                {
                    Code = PeerResponseCode.PeerError,
                    TurnServerName = turnServerName,
                    RoomName = roomName,
                    PeerUserName = peerUserName,
                    ErrorMessage = ex.Message
                });
            }
        }


        public async Task OnPeerIceCandidateAsync(string turnServerName, string roomName, string peerUserName, 
            string peerIce)
        {
            Subject<PeerResponseParameters> subject = null;
            try
            {
                var connectionContext = _webRtcConnection.GetConnectionContext(turnServerName, roomName);
                _logger.LogInformation(
                    $"<-------- OnPeerIceCandidate - turn:{turnServerName} room:{roomName} " +
                    $"user:{connectionContext.ConnectionRequestParameters.ConnectionParameters.UserName} " +
                    $"peerUser:{peerUserName} " +
                    $"peerIce:{peerIce}");
                var peerContext = connectionContext.PeerContexts
                    .Single(context => context.PeerParameters.PeerUserName
                    .Equals(peerUserName, StringComparison.OrdinalIgnoreCase));
                var peerConnection = peerContext.PeerConnection;
                subject = peerContext.PeerResponseSubject;

                var iceCandidate = JsonSerializer.Deserialize<RTCIceCandidateInit>(peerIce,
                    JsonHelper.WebRtcJsonSerializerOptions);
                //_logger.LogInformation(
                //    $"**** AddIceCandidate - turn:{turnServerName} room:{roomName} " +
                //    $"user:{connectionContext.ConnectionRequestParameters.ConnectionParameters.UserName} " +
                //    $"peerUser:{peerUserName}");
                await peerConnection.AddIceCandidate(iceCandidate);
            }
            catch (Exception ex)
            {
                subject?.OnNext(new PeerResponseParameters
                {
                    Code = PeerResponseCode.PeerError,
                    TurnServerName = turnServerName,
                    RoomName = roomName,
                    PeerUserName = peerUserName,
                    ErrorMessage = ex.Message
                });
            }
        }


        #endregion


    }
}
