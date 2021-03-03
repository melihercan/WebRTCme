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

namespace WebRTCme.Middleware.Blazor
{

    public partial class Media : IDisposable
    {
        private IMediaStream _stream;
        [Parameter]
        public IMediaStream Stream
        {
            get => _stream;
            set
            {
                _stream = value;
                if (_stream is not null)
                    BlazorSupport.SetVideoSource(JsRuntime, VideoElementReference, _stream);
            }
        }

        [Parameter]
        public string Label { get; set; } = string.Empty;

        [Parameter]
        public bool VideoMuted { get; set; } = false;

        [Parameter]
        public bool AudioMuted { get; set; } = false;

        [Inject]
        private IJSRuntime JsRuntime { get; set; }

        [Inject]
        private IConfiguration Configuration { get; set; }

        private ElementReference VideoElementReference { get; set; }

        protected override Task OnParametersSetAsync()
        {
            //if (Stream is not null)
              //  BlazorSupport.SetVideoSource(JsRuntime, VideoElementReference, Stream);

            return base.OnParametersSetAsync();
        }

        public void Dispose()
        {
        }
    }
}
