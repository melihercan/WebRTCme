using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using WebRTCme.Middleware.MediaStreamProxies;
using WebRTCme.Middleware.Models;

namespace WebRTCme.Middleware.Services
{
    class MediaServerConnection : IMediaServerConnection
    {
        readonly MediaServerProxyFactory _mediaServerProxyFactory;
        readonly ILogger<MediaServerConnection> _logger;

        static Dictionary<MediaServer, IMediaServerProxy> MediaServerProxies = new();

        public MediaServerConnection(MediaServerProxyFactory mediaServerProxyFactory, 
            ILogger<MediaServerConnection> logger)
        {
            _mediaServerProxyFactory = mediaServerProxyFactory;
            _logger = logger;
        }

        IMediaServerProxy GetMediaServerClient(MediaServer mediaServer)
        {
            if (!MediaServerProxies.ContainsKey(mediaServer))
                MediaServerProxies.Add(mediaServer, _mediaServerProxyFactory.Create(mediaServer));
            return MediaServerProxies[mediaServer];
        }

        MediaServer GetMediaServerFromName(string mediaServerName) =>
            (MediaServer)Enum.Parse(typeof(MediaServer), mediaServerName, true);


        public Task<string[]> GetMediaServerNamesAsync() =>
            Task.FromResult(Enum.GetNames(typeof(MediaServer)));

        public IObservable<PeerResponseParameters> ConnectionRequest(ConnectionRequestParameters request)
        {
            return Observable.Create<PeerResponseParameters>(async observer =>
            {
                IMediaServerProxy mediaServerProxy = null;

                //ConnectionContext connectionContext = null;
                //bool isJoined = false;

                try
                {
                    var mediaServerName = GetMediaServerFromName(request.ConnectionParameters.MediaServerName);
                    mediaServerProxy = GetMediaServerClient(mediaServerName);
                    await mediaServerProxy.StartAsync(request);
                    await mediaServerProxy.JoinAsync();

                    //connectionContext = new ConnectionContext
                    //{
                    //    ConnectionRequestParameters = request,
                    //    Observer = observer
                    //};

                }
                catch (Exception ex)
                {
                    observer.OnError(ex);
                }

                return async () =>
                {
                    try
                    {
                        await mediaServerProxy.StopAsync();
                    }
                    catch { };
                };
            });
        }


        public Task ReplaceOutgoingVideoTracksAsync(string turnServerName, string roomName, IMediaStreamTrack newVideoTrack)
        {
            throw new NotImplementedException();
        }

        public ValueTask DisposeAsync()
        {
            throw new NotImplementedException();
        }

    }
}
