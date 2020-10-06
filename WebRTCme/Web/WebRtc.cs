using System;
using System.Collections.Generic;
using System.Text;
using WebRtc.Web;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace WebRTCme
{
    internal class WebRtc : IWebRtc
    {
        public INavigator Navigator => throw new NotImplementedException();

        //        internal IServiceProvider ServiceProvider { get; private set; }

        public IRTCPeerConnection CreateRTCPeerConnection()
        {

            Task.Run(async () =>
            {
                try
                {
                    //                    var mediaDevices = await jsRuntime.InvokeAsync<List<MediaDeviceInfo>>(
                    //                      "navigator.mediaDevices.enumerateDevices");

                    //                    var mediaDevicesJson = await jsRuntime.InvokeAsync<object>(
                    //                      "navigator.mediaDevices.enumerateDevices");

                    //                var mediaDevicesJson2 = JsonSerializer.Deserialize<List<MediaDeviceInfo>>(mediaDevicesJson.ToString(),
                    //                  new JsonSerializerOptions
                    //                {
                    //AllowTrailingCommas = true,
                    //                  PropertyNameCaseInsensitive = true
                    //            });


                    //                    var tcs = new TaskCompletionSource<object>();
                    //                  var promiseHandler = DotNetObjectReference.Create<PromiseHandler>(new PromiseHandler());
                    //                var x = jsRuntime.InvokeAsync<object>(
                    //                  "navigator.mediaDevices.getUserMedia",
                    //                promiseHandler,
                    //              new object[]
                    //            {
                    //              new Constraints
                    //            {
                    //              Video = true,
                    //            Audio = true
                    //      }
                    //});
                    //var x5 = await tcs.Task;


                    //                    Console.WriteLine(x);
                }
                catch (Exception ex)
                {
                    var y = ex.Message;
                }
            });

            return new RTCPeerConnection();
        }

        //    public Task<object> GetUserMedia(IJSRuntime jsRuntime)
        //    {
        //        try
        //        {
        //            var tcs = new TaskCompletionSource<object>();
        //            var promiseHandler = DotNetObjectReference.Create<PromiseHandler>(new PromiseHandler { tcs = tcs });
        //            var x = jsRuntime.InvokeAsync<object>(
        //                              "navigator.mediaDevices.getUserMedia",
        //                            promiseHandler,
        //                          new object[]
        //                        {
        //                          new Constraints
        //                        {
        //                          Video = true,
        //                        Audio = true
        //                  }
        //            });
        //            return tcs.Task;
        //        }
        //        catch (Exception ex)
        //        {
        //            var x = ex.Message;
        //            return null;
        //        }
        //    }

        //    //        public void Initialize(IServiceProvider serviceProvider)
        //    //      {
        //    //        ServiceProvider = serviceProvider;
        //    //  }
        //}

        //class Constraints
        //{
        //    public bool Video { get; set; }
        //    public bool Audio { get; set; }
        //}
    }
}
