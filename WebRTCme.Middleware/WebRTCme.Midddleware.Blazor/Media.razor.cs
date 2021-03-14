using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebRTCme;
using WebRTCme.SignallingServerProxy;

namespace WebRTCme.Middleware
{
    public partial class Media : IDisposable
    {
  private static int _id = 1;
  private int Id; 

        private IMediaStream _stream;


        public Media()
        {
  Id = _id++;
   Console.WriteLine($"$$$$$$ Media{Id} created");
        }

        [Parameter]
        public IMediaStream Stream { get; set; }
        //{
        //    get => _stream;
        //    set
        //    {
        //        _stream = value;
        //        if (_stream is not null)
        //            BlazorSupport.SetVideoSource(JsRuntime, VideoElementReference, _stream);
        //    }
        //}

        [Parameter]
        public string Label { get; set; } = string.Empty;

        [Parameter]
        public bool VideoMuted { get; set; } = false;

        [Parameter]
        public bool AudioMuted { get; set; } = false;

        [Parameter]
        public bool ShowContols { get; set; } = false;

        [Inject]
        private IJSRuntime JsRuntime { get; set; }

        [Inject]
        private IConfiguration Configuration { get; set; }

        private ElementReference VideoElementReference { get; set; }

        //protected override Task OnParametersSetAsync()
        //{
 //Console.WriteLine($"$$$$$$ Media{Id} OnParametersSetAync Stream:{Stream} VideoElementReference.Id{VideoElementReference.Id}");
            //if (Stream is not null)// && VideoElementReference.Id is not null)
               //BlazorSupport.SetVideoSource(JsRuntime, VideoElementReference, Stream);

            //return base.OnParametersSetAsync();
        //}

        protected override void OnAfterRender(bool firstRender)
        {
 Console.WriteLine($"$$$$$$ Media{Id} OnAfterRender firstRender{firstRender}  Stream:{Stream} VideoElementReference.Id{VideoElementReference.Id}");

            base.OnAfterRender(firstRender);

            if (Stream is not null)// && VideoElementReference.Id is not null)
                BlazorSupport.SetVideoSource(JsRuntime, VideoElementReference, Stream);
        }

        public void Dispose()
        {
        }
    }
}
