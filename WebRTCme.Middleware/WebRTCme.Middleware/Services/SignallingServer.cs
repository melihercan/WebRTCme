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
    class SignallingServer : ISignallingServer, ISignallingServerCallbacks
    {
        readonly ISignallingServerProxy _signallingServerProxy;
        readonly IWebRtcConnection _webRtcConnection;
        readonly ILogger<SignallingServer> _logger;
        readonly IJSRuntime _jsRuntime;

        public SignallingServer(ISignallingServerProxy signallingServerProxy, IWebRtcConnection webRtcConnection,
            ILogger<SignallingServer> logger, IJSRuntime jsRuntime = null)
        {
            _signallingServerProxy = signallingServerProxy;
            _webRtcConnection = webRtcConnection;
            _logger = logger;
            _jsRuntime = jsRuntime;

            _signallingServerProxy.OnPeerJoinedAsyncEvent += OnPeerJoinedAsync;
            _signallingServerProxy.OnPeerLeftAsyncEvent += OnPeerLeftAsync;
            _signallingServerProxy.OnPeerSdpAsyncEvent += OnPeerSdpAsync;
            _signallingServerProxy.OnPeerIceAsyncEvent += OnPeerIceCandidateAsync;
        }

        public async Task<(SignallingServerResult, string[])> GetTurnServerNamesAsync()
        {
            return await _signallingServerProxy.GetTurnServerNamesAsync();
        }

        public ValueTask DisposeAsync()
        {
            _signallingServerProxy.OnPeerJoinedAsyncEvent -= OnPeerJoinedAsync;
            _signallingServerProxy.OnPeerLeftAsyncEvent -= OnPeerLeftAsync;
            _signallingServerProxy.OnPeerSdpAsyncEvent -= OnPeerSdpAsync;
            _signallingServerProxy.OnPeerIceAsyncEvent -= OnPeerIceCandidateAsync;
            return new ValueTask();
        }

        #region SignallingServerCallbacks

        public async Task OnPeerJoinedAsync(string turnServerName, string roomName, string peerUserName) 
        {
            ConnectionContext connectionContext = null;
            try
            {
                connectionContext =  _webRtcConnection.GetConnectionContext(turnServerName, roomName);
                _logger.LogInformation(
                    $">>>>>>>> OnPeerJoined - turn:{turnServerName} room:{roomName} " +
                    $"user:{connectionContext.ConnectionRequestParameters.ConnectionParameters.UserName} " +
                    $"peerUser:{peerUserName}");

                await _webRtcConnection.CreateOrDeletePeerConnectionAsync(turnServerName, roomName, peerUserName, isInitiator: true);
                var peerContext = connectionContext.PeerContexts
                    .Single(context => context.PeerParameters.PeerUserName
                    .Equals(peerUserName, StringComparison.OrdinalIgnoreCase));
                var peerConnection = peerContext.PeerConnection;

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

                var result = await _signallingServerProxy.SdpAsync(turnServerName, roomName, peerUserName, sdp);
                if (result != SignallingServerResult.Ok)
                    throw new Exception($"{result}");
            }
            catch (Exception ex)
            {
                connectionContext?.Observer.OnNext(new PeerResponseParameters
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
            ConnectionContext connectionContext = null;
            try
            {
                connectionContext = _webRtcConnection.GetConnectionContext(turnServerName, roomName);
                //_logger.LogInformation(
                //    $">>>>>>>> OnPeerLeft - turn:{turnServerName} room:{roomName} " +
                //    $"user:{connectionContext.ConnectionRequestParameters.ConnectionParameters.UserName} " +
                //    $"peerUser:{peerUserName}");
                var peerContext = connectionContext.PeerContexts
                    .Single(context => context.PeerParameters.PeerUserName
                    .Equals(peerUserName, StringComparison.OrdinalIgnoreCase));

                await _webRtcConnection.CreateOrDeletePeerConnectionAsync(turnServerName, roomName, peerUserName,
                    isInitiator: peerContext.IsInitiator, isDelete: true);

                connectionContext.Observer.OnNext(new PeerResponseParameters 
                {
                    Code = PeerResponseCode.PeerLeft,
                    TurnServerName = turnServerName,
                    RoomName = roomName,
                    PeerUserName = peerUserName,
                });
            }
            catch (Exception ex)
            {
                connectionContext?.Observer.OnNext(new PeerResponseParameters
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
            ConnectionContext connectionContext = null;
            try
            {
                connectionContext = _webRtcConnection.GetConnectionContext(turnServerName, roomName);
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
                    
                    var result = await _signallingServerProxy.SdpAsync(turnServerName, roomName, peerUserName, sdp);
                    if (result != SignallingServerResult.Ok)
                        throw new Exception($"{result}");
                }
            }
            catch (Exception ex)
            {
                connectionContext?.Observer.OnNext(new PeerResponseParameters
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
            ConnectionContext connectionContext = null;
            try
            {
                connectionContext = _webRtcConnection.GetConnectionContext(turnServerName, roomName);
                _logger.LogInformation(
                    $"<-------- OnPeerIceCandidate - turn:{turnServerName} room:{roomName} " +
                    $"user:{connectionContext.ConnectionRequestParameters.ConnectionParameters.UserName} " +
                    $"peerUser:{peerUserName} " +
                    $"peerIce:{peerIce}");
                var peerContext = connectionContext.PeerContexts
                    .Single(context => context.PeerParameters.PeerUserName
                    .Equals(peerUserName, StringComparison.OrdinalIgnoreCase));
                var peerConnection = peerContext.PeerConnection;

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
                connectionContext?.Observer.OnNext(new PeerResponseParameters
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
