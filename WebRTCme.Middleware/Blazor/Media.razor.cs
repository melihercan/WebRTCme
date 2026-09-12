using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using Microsoft.JSInterop;
using System;
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

        /// <summary>
        /// Whether this tile is the local preview rather than a peer.
        /// </summary>
        [Parameter]
        public bool IsLocal { get; set; } = false;

        /// <summary>
        /// Whether the peer has muted its microphone.
        /// </summary>
        [Parameter]
        public bool PeerAudioMuted { get; set; } = false;

        /// <summary>
        /// Whether the peer has turned its camera off.
        /// </summary>
        [Parameter]
        public bool PeerVideoMuted { get; set; } = false;

        /// <summary>
        /// Whether the peer is talking right now.
        /// </summary>
        [Parameter]
        public bool PeerSpeaking { get; set; } = false;

        [Inject]
        private IJSRuntime JsRuntime { get; set; }

        [Inject]
        private IConfiguration Configuration { get; set; }

        private ElementReference VideoElementReference { get; set; }

        // What the video element was last pointed at, so it is only pointed again when that
        // actually changed.
        private IMediaStream _attachedStream;
        private bool _attachedMuted;

        /// <summary>
        /// Attaches the stream to the video element, once per stream.
        /// </summary>
        /// <remarks>
        /// This used to assign <c>srcObject</c> on every render, which was harmless while the
        /// page only re-rendered when a peer joined or left. It re-renders far more often now -
        /// the speaking indicator changes with every phrase - and each of those renders was a
        /// JS interop call to hand the element the stream it already had.
        /// </remarks>
        protected override void OnAfterRender(bool firstRender)
        {
            base.OnAfterRender(firstRender);

            if (Stream is null)
                return;

            if (ReferenceEquals(_attachedStream, Stream) && _attachedMuted == VideoMuted)
                return;

            BlazorSupport.SetVideoSource(JsRuntime, VideoElementReference, Stream, VideoMuted);
            _attachedStream = Stream;
            _attachedMuted = VideoMuted;
        }

        public void Dispose()
        {
        }
    }
}
