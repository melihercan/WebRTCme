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

        /// <summary>
        /// Whether this machine's own outgoing video is muted - the local preview's own state.
        /// </summary>
        /// <remarks>
        /// Covers this tile, the same way <see cref="PeerVideoMuted"/> covers a peer's. Until
        /// 2026-09-22 this drove the video element's <c>muted</c> property instead, which is an
        /// audio control and was never what the name said: it left the local preview playing its
        /// own microphone back (<see cref="AudioMuted"/> drives that now, as it always meant to),
        /// and it left the preview showing live video after the camera had been turned off.
        /// </remarks>
        [Parameter]
        public bool VideoMuted { get; set; } = false;

        /// <summary>
        /// Whether this tile's audio should be silenced by the element - true for the local
        /// preview, which would otherwise echo this machine's own microphone back at it.
        /// </summary>
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

        /// <summary>
        /// Whether this peer's transport is being restarted. The tile covers its last frame while
        /// it is, because that frame is stale and a still picture reads as a working call.
        /// </summary>
        [Parameter]
        public bool PeerReconnecting { get; set; } = false;

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

            if (ReferenceEquals(_attachedStream, Stream) && _attachedMuted == AudioMuted)
                return;

            BlazorSupport.SetVideoSource(JsRuntime, VideoElementReference, Stream, AudioMuted);
            _attachedStream = Stream;
            _attachedMuted = AudioMuted;
        }

        public void Dispose()
        {
        }
    }
}
