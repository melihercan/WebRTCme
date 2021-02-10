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
using WebRTCme.SignallingServerClient;

namespace WebRTCme.Middleware.Blazor
{
    public partial class Media : IDisposable
    {
        [Parameter]
        public IMediaStream Stream { get; set; }

        [Parameter]
        public string Label { get; set; }

        [Parameter]
        public bool VideoMuted { get; set; }

        [Parameter]
        public bool AudioMuted { get; set; }

        [Inject]
        private IJSRuntime JsRuntime { get; set; }

        [Inject]
        private IConfiguration Configuration { get; set; }

        private ElementReference VideoElementReference { get; set; }

        protected override Task OnParametersSetAsync()
        {
            if (Stream is not null)
                BlazorSupport.SetVideoSource(JsRuntime, VideoElementReference, Stream);

            return base.OnParametersSetAsync();
        }

        public void Dispose()
        {
        }
    }
}
