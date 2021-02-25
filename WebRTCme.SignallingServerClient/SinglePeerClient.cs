using Ardalis.Result;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Reactive;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using WebSocketSharp;
using Xamarin.Essentials;

namespace WebRTCme.SignallingServerClient
{
    internal class SinglePeerClient : ISignallingServerClient
    {
        private readonly string _signallingServerBaseUrl;
        private readonly ISignallingServerCallbacks _signallingServerCallbacks;
        private WebSocket _ws;
        //private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private string _userName;
        private string _peerUserName;

        private JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
        };

        public static Task<ISignallingServerClient> CreateAsync(string signallingServerBaseUrl, 
            ISignallingServerCallbacks signallingServerCallbacks)
        {
            var self = new SinglePeerClient(signallingServerBaseUrl, signallingServerCallbacks);
            return Task.FromResult(self as ISignallingServerClient);
        }

        private SinglePeerClient(string signallingServerBaseUrl, ISignallingServerCallbacks signallingServerCallbacks)
        {
            _signallingServerCallbacks = signallingServerCallbacks;

            // For now hard coded.
            _ws = new WebSocket("ws://192.168.1.48:8080");
            _ws.OnMessage += WebSocket_OnMessage;
            _ws.Connect();
        }

        private async void WebSocket_OnMessage(object sender, MessageEventArgs e)
        {
            var jsonMessage = e.Data;
            var signallingMessage = JsonSerializer.Deserialize<SignallingMessage>(jsonMessage, _jsonSerializerOptions);

            if (signallingMessage.Type?.Equals(nameof(JoinRoom)) == true)
            {
                var data = JsonSerializer.Deserialize<Data>(signallingMessage.Candidate.Sdp, _jsonSerializerOptions);
                _peerUserName = data.Name;
                await _signallingServerCallbacks.OnPeerJoined(data.TurnServerName, data.RoomName,
                    data.Name);
            }
            else if (signallingMessage.Type?.Equals(nameof(LeaveRoom)) == true)
            {
                var data = JsonSerializer.Deserialize<Data>(signallingMessage.Candidate.Sdp, _jsonSerializerOptions);
                await _signallingServerCallbacks.OnPeerLeft(data.TurnServerName, data.RoomName,
                    data.Name);
            }
            else if (signallingMessage.Type?
                .Equals(RTCSdpType.Offer.ToString(), StringComparison.OrdinalIgnoreCase) == true)
            {
                var data = JsonSerializer.Deserialize<Data>(signallingMessage.Candidate.Sdp, _jsonSerializerOptions);
                _peerUserName = data.Name;
                await _signallingServerCallbacks.OnPeerSdpOffered(data.TurnServerName, data.RoomName,
                    _peerUserName, signallingMessage.Sdp);
            }
            else if (signallingMessage.Type?
                .Equals(RTCSdpType.Answer.ToString(), StringComparison.OrdinalIgnoreCase) == true)
            {
                var data = JsonSerializer.Deserialize<Data>(signallingMessage.Candidate.Sdp, _jsonSerializerOptions);
                await _signallingServerCallbacks.OnPeerSdpAnswered(data.TurnServerName, data.RoomName,
                    _peerUserName, signallingMessage.Sdp);
            }
            else if (signallingMessage.Candidate is not null)
            {
                var data = JsonSerializer.Deserialize<Data>(signallingMessage.Sdp, _jsonSerializerOptions);
                var iceCandidate = new RTCIceCandidateInit
                {
                    Candidate = signallingMessage.Candidate.Sdp,
                    SdpMLineIndex = (ushort)signallingMessage.Candidate.SdpMLineIndex,
                    SdpMid = signallingMessage.Candidate.SdpMid
                };
                var ice = JsonSerializer.Serialize(iceCandidate, _jsonSerializerOptions);
                await _signallingServerCallbacks.OnPeerIceCandidate(data.TurnServerName, data.RoomName,
                    _peerUserName, ice);
            }
        }

        public async ValueTask DisposeAsync()
        {
        }

        public Task<Result<string[]>> GetTurnServerNames() =>
            Task.FromResult(Result<string[]>.Success(new string[] 
            { 
                "StunOnly" 
            }));

        public Task<Result<RTCIceServer[]>> GetIceServers(string turnServerName) =>
            Task.FromResult(Result<RTCIceServer[]>.Success(new RTCIceServer[] 
            {
                new RTCIceServer
                {
                    Urls = new string[]
                    {
                        "stun:stun.l.google.com:19302"
                    },
                }
            }));

        public Task<Result<Unit>> ReserveRoom(string turnServerName, string roomName, string adminUserName,
            string[] participantUserNames) =>
                throw new NotImplementedException();
        public Task<Result<Unit>> FreeRoom(string turnServerName, string roomName, string adminUserName) =>
                throw new NotImplementedException();

        public Task<Result<Unit>> AddParticipant(string turnServerName, string roomName, 
            string participantUserName) =>
                throw new NotImplementedException();

        public Task<Result<Unit>> RemoveParticipant(string turnServerName, string roomName,
            string participantUserName) =>
                throw new NotImplementedException();

        public Task<Result<Unit>> JoinRoom(string turnServerName, string roomName, string userName)
        {
            _userName = userName;
            var data = new Data
            {
                TurnServerName = turnServerName,
                RoomName = roomName,
                Name = userName
            };
            var signallingMessage = new SignallingMessage
            {
                Type = nameof(JoinRoom),
                Candidate = new Candidate
                {
                    Sdp = JsonSerializer.Serialize(data, _jsonSerializerOptions)
                }
            };
            Send(signallingMessage);
            return Task.FromResult(Result<Unit>.Success(Unit.Default));
        }

        public Task<Result<Unit>> LeaveRoom(string turnServerName, string roomName, string userName)
        {
            var data = new Data
            {
                TurnServerName = turnServerName,
                RoomName = roomName,
                Name = userName
            };
            var signallingMessage = new SignallingMessage
            {
                Type = nameof(LeaveRoom),
                Candidate = new Candidate
                {
                    Sdp = JsonSerializer.Serialize(data, _jsonSerializerOptions)
                }
            };
            Send(signallingMessage);
            return Task.FromResult(Result<Unit>.Success(Unit.Default));
        }

        public Task<Result<Unit>> OfferSdp(string turnServerName, string roomName, string pairUserName, 
            string sdp)
        {
            var data = new Data
            {
                TurnServerName = turnServerName,
                RoomName = roomName,
                Name = _userName
            };

            var signallingMessage = new SignallingMessage
            {
                Type = RTCSdpType.Offer.ToString(),
                Sdp = sdp,
                Candidate = new Candidate
                {
                    Sdp = JsonSerializer.Serialize(data, _jsonSerializerOptions)
                }
            };
            Send(signallingMessage);
            return Task.FromResult(Result<Unit>.Success(Unit.Default));
        }

        public Task<Result<Unit>> AnswerSdp(string turnServerName, string roomName, string pairUserName,
            string sdp)
        {
            var data = new Data
            {
                TurnServerName = turnServerName,
                RoomName = roomName,
                Name = pairUserName
            };

            var signallingMessage = new SignallingMessage
            {
                Type = RTCSdpType.Answer.ToString(),
                Sdp = sdp,
                Candidate = new Candidate
                {
                    Sdp = JsonSerializer.Serialize(data, _jsonSerializerOptions)
                }
            };
            Send(signallingMessage);
            return Task.FromResult(Result<Unit>.Success(Unit.Default));
        }

        public Task<Result<Unit>> IceCandidate(string turnServerName, string roomName, string pairUserName,
            string ice)
        {
            var iceCandidate = JsonSerializer.Deserialize<RTCIceCandidateInit>(ice, _jsonSerializerOptions);

            var data = new Data
            {
                TurnServerName = turnServerName,
                RoomName = roomName,
                Name = pairUserName
            };

            var signallingMessage = new SignallingMessage
            {
                Sdp = JsonSerializer.Serialize(data, _jsonSerializerOptions),
                Candidate = new Candidate
                {
                    Sdp = iceCandidate.Candidate,
                    SdpMLineIndex = (int)iceCandidate.SdpMLineIndex,
                    SdpMid = iceCandidate.SdpMid
                }
            };
            Send(signallingMessage);
            return Task.FromResult(Result<Unit>.Success(Unit.Default));
        }


        private void Send(SignallingMessage signallingMessage)
        {
            var jsonMessage = JsonSerializer.Serialize(signallingMessage, _jsonSerializerOptions);
            _ws.Send(jsonMessage);
        }


            public Result<Unit> OfferSdpSync(string turnServerName, string roomName, string pairUserName, string sdp)
        {
            throw new NotImplementedException();

        }

        public Result<Unit> AnswerSdpSync(string turnServerName, string roomName, string pairUserName, string sdp)
        {
            throw new NotImplementedException();
        }

        public Result<Unit> IceCandidateSync(string turnServerName, string roomName, string pairUserName, string ice)
        {
            var iceCandidate = JsonSerializer.Deserialize<RTCIceCandidateInit>(ice, _jsonSerializerOptions);

            var data = new Data
            {
                TurnServerName = turnServerName,
                RoomName = roomName,
                Name = pairUserName
            };

            var signallingMessage = new SignallingMessage
            {
                Sdp = JsonSerializer.Serialize(data, _jsonSerializerOptions),
                Candidate = new Candidate
                {
                    Sdp = iceCandidate.Candidate,
                    SdpMLineIndex = (int)iceCandidate.SdpMLineIndex,
                    SdpMid = iceCandidate.SdpMid
                }
            };
            Send(signallingMessage);
            return Result<Unit>.Success(Unit.Default);
        }
    }
}
