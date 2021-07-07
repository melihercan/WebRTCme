using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using WebRTCme.ConnectionServer;
using WebRTCme.Middleware.MediaStreamProxies;
using WebRTCme.Middleware.Models;

//// CURRENTLY USING HARD CODED MediaSoup

namespace WebRTCme.Middleware.Services
{
    class MediaServerConnection : IMediaServerConnection
    {
        //        readonly MediaServerProxyFactory _mediaServerProxyFactory;
        readonly IMediaServerApi _mediaServerApi;
        readonly ILogger<MediaServerConnection> _logger;
        readonly IWebRtcMiddleware _webRtcMiddleware;
        readonly IJSRuntime _jsRuntime;

        static Dictionary<MediaServer, IMediaServerProxy> MediaServerProxies = new();

        public MediaServerConnection(/*MediaServerProxyFactory mediaServerProxyFactory,*/ IMediaServerApi mediaServerApi, 
            ILogger<MediaServerConnection> logger, IWebRtcMiddleware webRtcMiddleware, IJSRuntime jsRuntime = null)
        {
            //_mediaServerProxyFactory = mediaServerProxyFactory;
            _mediaServerApi = mediaServerApi;
            _logger = logger;
            _webRtcMiddleware = webRtcMiddleware;
            _jsRuntime = jsRuntime;

            MediaSoupClient.Registry.WebRtc = _webRtcMiddleware.WebRtc;
            MediaSoupClient.Registry.JsRuntime = _jsRuntime;



        }

        //IMediaServerProxy GetMediaServerClient(MediaServer mediaServer)
        //{
        //    if (!MediaServerProxies.ContainsKey(mediaServer))
        //        MediaServerProxies.Add(mediaServer, _mediaServerProxyFactory.Create(mediaServer));
        //    return MediaServerProxies[mediaServer];
        //}

        //MediaServer GetMediaServerFromName(string mediaServerName) =>
        //    (MediaServer)Enum.Parse(typeof(MediaServer), mediaServerName, true);


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

                    /////////// TODO: RoomClient.js functionality here.




                    //var mediaServerName = GetMediaServerFromName(request.ConnectionParameters.MediaServerName);
                    //mediaServerProxy = GetMediaServerClient(mediaServerName);
                    //await mediaServerProxy.StartAsync(request);
                    //await mediaServerProxy.JoinAsync();

                    await _mediaServerApi.JoinAsync(Guid.NewGuid(), request.ConnectionParameters.UserName,
                        request.ConnectionParameters.RoomName);

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
