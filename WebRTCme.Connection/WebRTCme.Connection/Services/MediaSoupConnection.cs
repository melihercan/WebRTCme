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
using WebRTCme.Connection.MediaSoup;
using WebRTCme.Connection.MediaSoup.Proxy.Client;

namespace WebRTCme.Connection.Services
{
    class MediaSoupConnection : IConnection
    {
        readonly IConfiguration _configuration;
        readonly IMediaSoupServerApi _mediaSoupServerApi;
        readonly ILogger<MediaSoupConnection> _logger;
        readonly IWebRtc _webRtc;
        readonly IJSRuntime _jsRuntime;

        public MediaSoupConnection(IConfiguration configuration, 
            IMediaSoupServerApi mediaSoupServerApi,
            ILogger<MediaSoupConnection> logger, IWebRtc webRtc, IJSRuntime jsRuntime = null)
        {
            //_mediaServerProxyFactory = mediaServerProxyFactory;
            _configuration = configuration;
            _mediaSoupServerApi = mediaSoupServerApi;
            _logger = logger;
            _webRtc = webRtc;
            _jsRuntime = jsRuntime;

            //MediaSoupClient.Registry.WebRtc = _webRtcMiddleware.WebRtc;
            //MediaSoupClient.Registry.JsRuntime = _jsRuntime;
        }


        public IObservable<PeerResponse> ConnectionRequest(UserContext userContext)
        {
            return Observable.Create<PeerResponse>(async observer =>
            {
                //IMediaServerProxy mediaServerProxy = null;

                //ConnectionContext connectionContext = null;
                //bool isJoined = false;

                var guid = Guid.NewGuid();

                try
                {


                    _mediaSoupServerApi.NotifyEventAsync += MediaServer_OnNotifyAsync;


                    //var mediaServerName = GetMediaServerFromName(request.ConnectionParameters.MediaServerName);
                    //mediaServerProxy = GetMediaServerClient(mediaServerName);
                    //await mediaServerProxy.StartAsync(request);
                    //await mediaServerProxy.JoinAsync();

                    //await _mediaServerApi.JoinAsync(Guid.NewGuid(), request.ConnectionParameters.UserName,
                    //    request.ConnectionParameters.RoomName);
                    await _mediaSoupServerApi.ConnectAsync(guid, userContext.Name,
                        userContext.Room);


                    var mediaSoupDevice = new Device();


                    var routerRtpCapabilities = (RtpCapabilities)ParseResponse(MethodName.GetRouterRtpCapabilities,
                        await _mediaSoupServerApi.CallAsync(MethodName.GetRouterRtpCapabilities));
                    await mediaSoupDevice.LoadAsync(routerRtpCapabilities);

                    var transportInfo = (TransportInfo)ParseResponse(MethodName.CreateWebRtcTransport,
                        await _mediaSoupServerApi.CallAsync(MethodName.CreateWebRtcTransport,
                            new WebRtcTransportCreateParameters
                            {
                                ForceTcp = _configuration.GetValue<bool>("MediaSoupServer:ForceTcp"),
                                Producing = true,
                                Consuming = false,
                                SctpCapabilities = new SctpCapabilities
                                {
                                    NumStreams = new NumSctpStreams
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
                        _mediaSoupServerApi.NotifyEventAsync -= MediaServer_OnNotifyAsync;
                        await _mediaSoupServerApi.DisconnectAsync(guid);
                        //await mediaServerProxy.StopAsync();
                    }
                    catch { };
                };
            });

            Task MediaServer_OnNotifyAsync(string method, object data)
            {
                throw new NotImplementedException();
            }
        }

        public Task<IRTCStatsReport> GetStats(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task ReplaceOutgoingTrackAsync(IMediaStreamTrack track, IMediaStreamTrack newTrack)
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
                    var routerRtpCapabilities = JsonSerializer.Deserialize<RtpCapabilities>(
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
