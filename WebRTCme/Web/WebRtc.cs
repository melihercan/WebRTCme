using Microsoft.JSInterop;
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
        //        internal IServiceProvider ServiceProvider { get; private set; }

        public IRTCPeerConnection CreateRTCPeerConnection(IJSRuntime jsRuntime)
        {

            Task.Run(async () => 
            {
                try
                {
                    var mediaDevices = await jsRuntime.InvokeAsync<List<MediaDeviceInfo>>(
                        "navigator.mediaDevices.enumerateDevices");


                    var x = await jsRuntime.InvokeAsync<object>(
                        "navigator.mediaDevices.getUserMedia",
                        new object[]
                        {
                            new Constraints
                            {
                                Video = true,
                                Audio = true
                            }
                        });
                    Console.WriteLine(x);
                }
                catch (Exception ex)
                {
                    var y = ex.Message;
                }
            });

            return new RTCPeerConnection();
        }

//        public void Initialize(IServiceProvider serviceProvider)
  //      {
    //        ServiceProvider = serviceProvider;
      //  }
    }

    class Constraints
    {
        public bool Video { get; set; }
        public bool Audio { get; set; }
    }
}
