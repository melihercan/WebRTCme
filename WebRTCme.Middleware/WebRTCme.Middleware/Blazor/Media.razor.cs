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

namespace WebRTCme.Middleware
{
    public partial class Media : IDisposable
    {
        [Parameter]
        public IMediaStream Stream { get; set; }

        [Parameter]
        public string Label { get; set; } = string.Empty;

        [Parameter]
        public bool Hangup { get; set; } = false;

        [Parameter]
        public bool VideoMuted { get; set; } = false;

        [Parameter]
        public bool AudioMuted { get; set; } = false;

        [Parameter]
        public CameraType CameraType { get; set; } = CameraType.Default;

        [Parameter]
        public bool ShowContols { get; set; } = false;

        [Inject]
        private IJSRuntime JsRuntime { get; set; }

        [Inject]
        private IConfiguration Configuration { get; set; }

        private ElementReference VideoElementReference { get; set; }

        protected override void OnAfterRender(bool firstRender)
        {
            base.OnAfterRender(firstRender);

            if (Stream is not null)// && VideoElementReference.Id is not null)
                BlazorSupport.SetVideoSource(JsRuntime, VideoElementReference, Stream, VideoMuted);
        }

        public void Dispose()
        {
        }
    }
}
