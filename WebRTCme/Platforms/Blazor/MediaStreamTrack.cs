using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebRTCme.Bindings.Blazor.Extensions;
using WebRTCme.Bindings.Blazor.Interops;
using WebRTCme;
using WebRTCme.Platforms.Blazor.Custom;

namespace WebRTCme.Blazor
{
    internal class MediaStreamTrack : NativeBase, IMediaStreamTrack
    {
        public MediaStreamTrack(IJSRuntime jsRuntime, JsObjectRef jsObjectRef) : base(jsRuntime, jsObjectRef) 
        { }

        public string ContentHint 
        {
            get => GetNativeProperty<string>("contentHint");
            set => SetNativeProperty("contentHint", value);
        }

        public bool Enabled 
        {
            get => GetNativeProperty<bool>("enabled");
            set => SetNativeProperty("enabled", value);
        }

        public string Id => GetNativeProperty<string>("id");

        public bool Isolated => GetNativeProperty<bool>("isolated");

        public MediaStreamTrackKind Kind => GetNativeProperty<MediaStreamTrackKind>("kind");

        public string Label => GetNativeProperty<string>("label");

        public bool Muted => GetNativeProperty<bool>("muted");

        public MediaStreamTrackState ReadyState => GetNativeProperty<MediaStreamTrackState>("readyState");

        // Registered when something subscribes, not when the wrapper is built.
        //
        // These three used to be attached in the constructor, and a wrapper is built for every
        // track on every GetTracks, GetVideoTracks and GetAudioTracks call - so each of those
        // calls attached three more JS listeners, each holding a DotNetObjectReference, to tracks
        // that already had them, and nothing ever removed them because the wrappers are transient
        // and never disposed.
        //
        // Measured on 2026-09-12 on a live call: nothing at all while the call sits idle, and
        // three listeners per track for every user action that touches tracks - two camera
        // toggles produced six. So it leaked slowly rather than quickly, which is why it went
        // unnoticed, and why it is worth fixing rather than urgent.
        //
        // Lazily instead of caching wrappers, because it fixes the leak where the leak is. A
        // wrapper nobody subscribes to now costs nothing, and one subscription attaches one
        // listener whoever built the wrapper. It does not make wrapper objects stable - that is
        // inherent to building one per call - so anything holding tracks in a collection still
        // has to key on Id. Two things in this repository learned that the hard way, and both
        // say so where they do it.
        EventHandler _onEnded;
        EventHandler _onMute;
        EventHandler _onUnmute;

        public event EventHandler OnEnded
        {
            add
            {
                if (_onEnded is null)
                    AddNativeEventListener("ended", (s, e) => _onEnded?.Invoke(s, e));
                _onEnded += value;
            }
            remove => _onEnded -= value;
        }

        public event EventHandler OnMute
        {
            add
            {
                if (_onMute is null)
                    AddNativeEventListener("mute", (s, e) => _onMute?.Invoke(s, e));
                _onMute += value;
            }
            remove => _onMute -= value;
        }

        public event EventHandler OnUnmute
        {
            add
            {
                if (_onUnmute is null)
                    AddNativeEventListener("unmute", (s, e) => _onUnmute?.Invoke(s, e));
                _onUnmute += value;
            }
            remove => _onUnmute -= value;
        }

        public Task ApplyConstraints(MediaTrackConstraints contraints) =>
            JsRuntime.InvokeVoidAsync("applyConstraints", contraints).AsTask();

        public IMediaStreamTrack Clone() =>
            new MediaStreamTrack(JsRuntime, JsRuntime.CallJsMethod<JsObjectRef>(NativeObject, "clone"));

        public MediaTrackCapabilities GetCapabilities() =>
            JsRuntime.CallJsMethodWithContent<MediaTrackCapabilities>(NativeObject, "getCapabilities");

        public MediaTrackConstraints GetConstraints() =>
            JsRuntime.CallJsMethodWithContent<MediaTrackConstraints>(NativeObject, "getConstraints");

        public MediaTrackSettings GetSettings() =>
            JsRuntime.CallJsMethodWithContent<MediaTrackSettings>(NativeObject, "getSettings");

        public void Stop() =>
            JsRuntime.CallJsMethodVoid(NativeObject, "stop");

        
        
        //public object GetView()
        //{
          //  throw new NotImplementedException();
        //}

    }
}
