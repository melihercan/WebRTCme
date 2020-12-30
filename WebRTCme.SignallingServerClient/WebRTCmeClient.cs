using Ardalis.Result;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WebRTCme.SignallingServerClient
{
    internal class WebRTCmeClient : ISignallingServerClient
    {
        private CancellationTokenSource _cts = new CancellationTokenSource();
        private HubConnection _hubConnection;
        private TaskCompletionSource<RTCIceServer[]> _roomTcs;


        public async Task InitializeAsync(string signallingServerBaseUrl)
        {
            _hubConnection = new HubConnectionBuilder()
                .WithUrl(signallingServerBaseUrl + "/roomhub")
                .AddMessagePackProtocol()
                .Build();
            _hubConnection.Closed += HubConnection_Closed;

            // Try connecting once.
            await ConnectWithRetryAsync(false);
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

        public async Task<RTCIceServer[]> CreateRoomAsync(string roomId, string clientId)
        {
            _roomTcs = new TaskCompletionSource<RTCIceServer[]>();
            await _hubConnection.SendAsync("CreateRoom", roomId, clientId);
            var iceServers = await _roomTcs.Task;
            return iceServers;
        }


        public Task<RTCIceServer[]> JoinRoomAsync(string roomId, string clientId)
        {
            throw new NotImplementedException();
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

        private async Task<bool> ConnectWithRetryAsync(bool keepTrying = true)
        {
            const int TimeoutMs = 1000;

            // Keep trying until we can start or the token is canceled.
            while (keepTrying)
            {
                try
                {
                    await _hubConnection.StartAsync(_cts.Token);
                    return true;
                }
                catch when (_cts.Token.IsCancellationRequested)
                {
                    return false;
                }
                catch 
                {
                    if (!keepTrying)
                        // Failed to connect, trying again in TimeoutMs if not overriden by keepTrying flag.
                        await Task.Delay(TimeoutMs);
                }
            }

            return false;
        }

        #region SignallingServerHubCallbacks
        public void SignallingServerHubCallback_RoomResponse(Result<RTCIceServer[]> result)
        {
            if (result.Status == ResultStatus.Ok)
                _roomTcs.SetResult(result.Value);
            else
                _roomTcs.SetException(new Exception(string.Join("-", result.Errors.ToArray())));
        }
        #endregion
    }
}
