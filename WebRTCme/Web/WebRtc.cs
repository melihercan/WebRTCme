using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Text;
using WebRtc.Web;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

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
                    var ret1 = await JsInterop.Prompt(jsRuntime, "hello there me");


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
