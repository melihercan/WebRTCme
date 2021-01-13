﻿using Ardalis.Result;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace WebRTCme.SignallingServerClient
{
    internal class WebRTCmeClient : ISignallingServerClient
    {
        private CancellationTokenSource _cts = new CancellationTokenSource();
        private HubConnection _hubConnection;

        private readonly string _signallingServerBaseUrl;

        public WebRTCmeClient(string signallingServerBaseUrl)
        {
            _signallingServerBaseUrl = signallingServerBaseUrl;
        }

        public Task InitializeAsync()
        {
            bool bypassSslCertificateError = DeviceInfo.Platform == DevicePlatform.Android;

            _hubConnection = new HubConnectionBuilder()
                .WithUrl(_signallingServerBaseUrl + "/roomhub", (opts) =>
                {
                    if (bypassSslCertificateError)
                    {
                        opts.HttpMessageHandlerFactory = (message) =>
                        {
                            if (message is HttpClientHandler clientHandler)
                            // Bypass SSL certificate.
                            clientHandler.ServerCertificateCustomValidationCallback +=
                                    (sender, certificate, chain, sslPolicyErrors) => { return true; };
                            return message;
                        };
                    }
                })
                .ConfigureLogging(logging =>
                {
                    // Log to output window.
                    logging.AddDebug();

                    // This will set ALL logging to Debug level
                    logging.SetMinimumLevel(LogLevel.Debug);
                })
                .AddMessagePackProtocol()
                .Build();

            // Register handlers called by hub.
            _hubConnection.On<string>("EchoToCallerResponse", EchoToCallerResponse);

            _hubConnection.Closed += HubConnection_Closed;

            // Start connection without waiting.
            _ = ConnectWithRetryAsync();

            return Task.CompletedTask;
        }

        public async Task CleanupAsync()
        {
            _cts.Cancel();
            if (_hubConnection.State != HubConnectionState.Disconnected)
            {
                try
                {
                    await _hubConnection.StopAsync(_cts.Token);
                }
                catch { }
            }
        }

        public Task JoinRoomAsync(string roomName, string userName)
        {
            return _hubConnection.SendAsync("JoinRoom", roomName, userName);
        }

        public async Task StartRoomAsync(string roomName, string userName, TurnServer turnServer)
        {
            var result = await _hubConnection.InvokeAsync<Result<object>>("StartRoom", roomName, 
                userName, turnServer);
            if (result.Status != ResultStatus.Ok)
                throw new Exception(string.Join("-", result.Errors.ToArray()));
        }



        public Task StartRoomAsync(string roomName, string userName)
        {
            throw new NotImplementedException();
        }

        public Task LeaveRoomAsync(string roomName, string userName)
        {
            throw new NotImplementedException();
        }


        public Task SendSdpOfferAsync(string sdp)
        {
            return _hubConnection.SendAsync("SendSdpOffer", sdp);
        }


        private Task HubConnection_Closed(Exception arg)
        {
            if (arg != null)
            {
                // arg null means connection is closed either by client or server and NOT due to error or exception.
                // Start connection again without awaiting in error or exception case.
                _ = ConnectWithRetryAsync();
            }
            
            return Task.CompletedTask;
        }

        private async Task ConnectWithRetryAsync()
        {
            const int TimeoutMs = 1000;

            // Keep trying until we can start or the token is canceled.
            while (true)
            {
                try
                {
                    await _hubConnection.StartAsync(_cts.Token);
                    Debug.WriteLine("#### Connected to Signalling Server...");
                    break;
                }
                catch when (_cts.Token.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("====> EXCEPTION: " + ex.Message);
                    ////throw ex;
                    // Failed to connect, trying again in TimeoutMs if not overriden by keepTrying flag.
                    await Task.Delay(TimeoutMs);
                }
            }
        }

        #region SignallingServerResponseHandlers

        // For echo testing.
        private TaskCompletionSource<string> _echoTcs;

        public async Task<string> ExecuteEchoToCaller(string message)
        {
            _echoTcs = new TaskCompletionSource<string>();
            await _hubConnection.SendAsync("EchoToCaller", message);
            var result = await _echoTcs.Task;
            return result;

        }
        public void EchoToCallerResponse(string message)
        {
            _echoTcs.SetResult(message);
        }

        #endregion
    }
}
