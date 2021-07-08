using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Utilme;
using WebRTCme.ConnectionServer;
using WebRTCme.Middleware.MediaStreamProxies;
using WebRTCme.Middleware.Models;

//// CURRENTLY USING HARD CODED MediaSoup

namespace WebRTCme.Middleware.Services
{
    class MediaServerConnection : IMediaServerConnection
    {
        //        readonly MediaServerProxyFactory _mediaServerProxyFactory;
        readonly IConfiguration _configuration;
        readonly IMediaServerApi _mediaServerApi;
        readonly ILogger<MediaServerConnection> _logger;
        readonly IWebRtcMiddleware _webRtcMiddleware;
        readonly IJSRuntime _jsRuntime;

        static Dictionary<MediaServer, IMediaServerProxy> MediaServerProxies = new();

        public MediaServerConnection(IConfiguration configuration, /*MediaServerProxyFactory mediaServerProxyFactory,*/ 
            IMediaServerApi mediaServerApi, 
            ILogger<MediaServerConnection> logger, IWebRtcMiddleware webRtcMiddleware, IJSRuntime jsRuntime = null)
        {
            //_mediaServerProxyFactory = mediaServerProxyFactory;
            _configuration = configuration;
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


                    _mediaServerApi.NotifyEventAsync += MediaServer_OnNotifyAsync;


                    //var mediaServerName = GetMediaServerFromName(request.ConnectionParameters.MediaServerName);
                    //mediaServerProxy = GetMediaServerClient(mediaServerName);
                    //await mediaServerProxy.StartAsync(request);
                    //await mediaServerProxy.JoinAsync();

                    //await _mediaServerApi.JoinAsync(Guid.NewGuid(), request.ConnectionParameters.UserName,
                    //    request.ConnectionParameters.RoomName);
                    await _mediaServerApi.ConnectAsync(Guid.NewGuid(), request.ConnectionParameters.UserName,
                        request.ConnectionParameters.RoomName);


                    /////////// TODO: CREATE DEVICE AND HANDLER

                    var routerRtpCapabilities = ParseResponse(MethodName.GetRouterRtpCapabilities, 
                        await _mediaServerApi.CallAsync(MethodName.GetRouterRtpCapabilities));
                    ////////////////// TODO: CALL DEVICE Load(routerRtpCapabilities)

                    var transportInfo = ParseResponse(MethodName.CreateWebRtcTransport, 
                        await _mediaServerApi.CallAsync(MethodName.CreateWebRtcTransport,
                            new WebRtcTransportCreateParameters
                            {
                                ForceTcp = _configuration.GetValue<bool>("MediaSoupServer:ForceTcp"),
                                Producing = true,
                                Consuming = false,
                                SctpCapabilities = new SctpCapabilities
                                {
                                    NumStream = new NumSctpStreams
                                    {
                                        Os = 1024,
                                        Mis = 1024,
                                    }
                                }
                            }));





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
                        _mediaServerApi.NotifyEventAsync -= MediaServer_OnNotifyAsync;
                        await mediaServerProxy.StopAsync();
                    }
                    catch { };
                };
            });

            Task MediaServer_OnNotifyAsync(string method, object data)
            {
                throw new NotImplementedException();
            }

        }


        public Task ReplaceOutgoingVideoTracksAsync(string turnServerName, string roomName, IMediaStreamTrack newVideoTrack)
        {
            throw new NotImplementedException();
        }

        public ValueTask DisposeAsync()
        {
            throw new NotImplementedException();
        }

        object ParseResponse(string method, Result<object> result)
        {
            if (!result.IsOk)
                throw new Exception(result.ErrorMessage);

            var data = result.Value;            
            var json = ((JsonElement)data).GetRawText();

            switch (method)
            {
                case MethodName.GetRouterRtpCapabilities:
                    var routerRtpCapabilities = JsonSerializer.Deserialize<RouterRtpCapabilities>(
                        json, JsonHelper.CamelCaseAndIgnoreNullJsonSerializerOptions);
                    foreach (var codec in routerRtpCapabilities.Codecs)
                    {
                        var parametersJson = ((JsonElement)codec.Parameters).GetRawText();
                        if (codec.MimeType.Equals("audio/opus"))
                        {
                            var opus = JsonSerializer.Deserialize<OpusParameters>(parametersJson,
                                JsonHelper.CamelCaseAndIgnoreNullJsonSerializerOptions);
                            codec.Parameters = opus;
                        }
                        if (codec.MimeType.Equals("video/H264"))
                        {
                            var h264 = JsonSerializer.Deserialize<H264Parameters>(parametersJson,
                                JsonHelper.CamelCaseAndIgnoreNullJsonSerializerOptions);
                            codec.Parameters = h264;
                        }
                        else if (codec.MimeType.Equals("video/VP8"))
                        {
                            var vp8 = JsonSerializer.Deserialize<VP8Parameters>(parametersJson,
                                JsonHelper.CamelCaseAndIgnoreNullJsonSerializerOptions);
                            codec.Parameters = vp8;
                        }
                        else if (codec.MimeType.Equals("video/VP9"))
                        {
                            var vp9 = JsonSerializer.Deserialize<VP9Parameters>(parametersJson,
                                JsonHelper.CamelCaseAndIgnoreNullJsonSerializerOptions);
                            codec.Parameters = vp9;
                        }
                        else if (codec.MimeType.Equals("video/rtx"))
                        {
                            var rtx = JsonSerializer.Deserialize<RtxParameters>(parametersJson,
                                JsonHelper.CamelCaseAndIgnoreNullJsonSerializerOptions);
                            codec.Parameters = rtx;
                        }
                        else
                            codec.Parameters = null;
                    }
                    return routerRtpCapabilities;

                case MethodName.CreateWebRtcTransport:
                    var transportInfo = JsonSerializer.Deserialize<TransportInfo>(
                        json, JsonHelper.CamelCaseAndIgnoreNullJsonSerializerOptions);
                    return transportInfo;

            }

            return null;

        }

    }
}
