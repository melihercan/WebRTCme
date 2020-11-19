using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebRtcJsInterop.Interops;
using WebRTCme;

namespace WebRtcJsInterop.Api
{
    internal class MediaStreamTrack : ApiBase, IMediaStreamTrack
    {
        private MediaStreamTrack(IJSRuntime jsRuntime, JsObjectRef jsObjectRef) : base(jsRuntime, jsObjectRef) { }

        public Task<string> ContentHint { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public Task<bool> Enabled { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public Task<string> Id => throw new NotImplementedException();

        public Task<bool> Isolated => throw new NotImplementedException();

        public Task<MediaStreamTrackKind> Kind => throw new NotImplementedException();

        public Task<string> Label => throw new NotImplementedException();

        public Task<bool> Muted => throw new NotImplementedException();

        public Task<bool> Readonly => throw new NotImplementedException();

        public Task<MediaStreamTrackState> ReadyState => throw new NotImplementedException();

        public Task<bool> Remote => throw new NotImplementedException();

        public event EventHandler OnMute;
        public event EventHandler OnUnmute;
        public event EventHandler OnEnded;

        internal static IMediaStreamTrack CreateAsync(IJSRuntime jsRuntime, JsObjectRef jsObjectRefMediaStreamTrack)
        {
            var mediaStreamTrack = new MediaStreamTrack(jsRuntime, jsObjectRefMediaStreamTrack);
            return mediaStreamTrack;
        }

        public Task ApplyConstraints(MediaTrackConstraints contraints)
        {
            throw new NotImplementedException();
        }

        public Task ApplyConstraintsAsync(MediaTrackConstraints contraints)
        {
            throw new NotImplementedException();
        }

        public Task<IMediaStreamTrack> Clone()
        {
            throw new NotImplementedException();
        }

        public Task<IMediaStreamTrack> CloneAsync()
        {
            throw new NotImplementedException();
        }

        public Task<MediaTrackCapabilities> GetCapabilities()
        {
            throw new NotImplementedException();
        }

        public Task<MediaTrackCapabilities> GetCapabilitiesAsync()
        {
            throw new NotImplementedException();
        }

        public Task<MediaTrackConstraints> GetContraints()
        {
            throw new NotImplementedException();
        }

        public Task<MediaTrackConstraints> GetContraintsAsync()
        {
            throw new NotImplementedException();
        }

        public Task<MediaTrackSettings> GetSettings()
        {
            throw new NotImplementedException();
        }

        public Task<MediaTrackSettings> GetSettingsAsync()
        {
            throw new NotImplementedException();
        }

        public Task Stop()
        {
            throw new NotImplementedException();
        }
    }
}
