using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebRTCme.Bindings.Blazor.Extensions;
using WebRTCme.Bindings.Blazor.Interops;
using WebRTCme;
using WebRTCme.Platforms.Blazor.Custom;

namespace WebRTCme.Blazor
{
    internal class MediaStream : NativeBase, IMediaStream
    {
        public MediaStream(IJSRuntime jsRuntime) : this(jsRuntime, jsRuntime.CreateJsObject("window", "MediaStream"))
        {
        }

        public MediaStream(IJSRuntime jsRuntime, JsObjectRef jsObjectRef) : base(jsRuntime, jsObjectRef) 
        {
            AddNativeEventListenerForObjectRef("addtrack", (s, e) => OnAddTrack?.Invoke(s, e), 
                MediaStreamTrackEvent.Create);
            AddNativeEventListenerForObjectRef("removetrack", (s, e) => OnRemoveTrack?.Invoke(s, e), 
                MediaStreamTrackEvent.Create);
        }

        public bool Active => GetNativeProperty<bool>("active");

        public string Id => GetNativeProperty<string>("id");

        public event EventHandler<IMediaStreamTrackEvent> OnAddTrack;
        public event EventHandler<IMediaStreamTrackEvent> OnRemoveTrack;

        public void AddTrack(IMediaStreamTrack track) =>
            JsRuntime.CallJsMethodVoid(NativeObject, "addTrack", ((MediaStreamTrack)track).NativeObject);

        public IMediaStream Clone() =>
            new MediaStream(JsRuntime, JsRuntime.CallJsMethod<JsObjectRef>(NativeObject, "clone"));

        public IMediaStreamTrack[] GetAudioTracks()
        {
            var jsObjectRefGetAudioTracks = JsRuntime.CallJsMethod<JsObjectRef>(NativeObject, "getAudioTracks");
            var jsObjectRefMediaStreamTrackArray = JsRuntime.GetJsPropertyArray(jsObjectRefGetAudioTracks);
            return jsObjectRefMediaStreamTrackArray
                .Select(jsObjectRef => new MediaStreamTrack(JsRuntime, jsObjectRef))
                .ToArray();
        }

        public IMediaStreamTrack GetTrackById(string id) =>
            new MediaStreamTrack(JsRuntime, JsRuntime.CallJsMethod<JsObjectRef>(NativeObject, "getTranckById", id));

        public IMediaStreamTrack[] GetTracks()
        {
            var jsObjectRefGetTracks = JsRuntime.CallJsMethod<JsObjectRef>(NativeObject, "getTracks");
            var jsObjectRefMediaStreamTrackArray = JsRuntime.GetJsPropertyArray(jsObjectRefGetTracks);
            return jsObjectRefMediaStreamTrackArray
                .Select(jsObjectRef => new MediaStreamTrack(JsRuntime, jsObjectRef))
                .ToArray();
        }

        public IMediaStreamTrack[] GetVideoTracks()
        {
            var jsObjectRefGetVideoTracks = JsRuntime.CallJsMethod<JsObjectRef>(NativeObject, "getVideoTracks");
            var jsObjectRefMediaStreamTrackArray = JsRuntime.GetJsPropertyArray(jsObjectRefGetVideoTracks);
            return jsObjectRefMediaStreamTrackArray
                .Select(jsObjectRef => new MediaStreamTrack(JsRuntime, jsObjectRef))
                .ToArray();
        }

        public void RemoveTrack(IMediaStreamTrack track) =>
            JsRuntime.CallJsMethodVoid(NativeObject, "removeTrack", ((MediaStreamTrack)track).NativeObject);
    }
}
