using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebRtcBindingsWeb.Extensions;
using WebRtcBindingsWeb.Interops;
using WebRTCme;

namespace WebRtcBindingsWeb.Api
{
    internal class MediaStream : ApiBase, IMediaStream
    {
        public static IMediaStream Create(IJSRuntime jsRuntime) =>
            new MediaStream(jsRuntime, jsRuntime.CreateJsObject("window", "MediaStream"));

        internal static IMediaStream Create(IJSRuntime jsRuntime, JsObjectRef jsObjectRefMediaStream) =>
            new MediaStream(jsRuntime, jsObjectRefMediaStream);

        private MediaStream(IJSRuntime jsRuntime, JsObjectRef jsObjectRef) : base(jsRuntime, jsObjectRef) { }

        public bool Active => GetNativeProperty<bool>("active");

        public string Id => GetNativeProperty<string>("id");

        public List<IMediaStreamTrack> GetTracks()
        {
            var jsObjectRefGetTracks = JsRuntime.CallJsMethod<JsObjectRef>(NativeObject, "getTracks");
            var jsObjectRefMediaStreamTrackArray = JsRuntime.GetJsPropertyArray(jsObjectRefGetTracks);
            return jsObjectRefMediaStreamTrackArray
                .Select(jsObjectRef => MediaStreamTrack.Create(JsRuntime, jsObjectRef))
                .ToList();
        }

        public IMediaStream Clone() =>
            Create(JsRuntime, JsRuntime.CallJsMethod<JsObjectRef>(NativeObject, "clone"));

        public IMediaStreamTrack GetTrackById(string id) => 
            MediaStreamTrack.Create(JsRuntime, JsRuntime.CallJsMethod<JsObjectRef>(NativeObject, "getTranckById", id));

        public List<IMediaStreamTrack> GetVideoTracks()
        {
            var jsObjectRefGetVideoTracks = JsRuntime.CallJsMethod<JsObjectRef>(NativeObject, "getVideoTracks");
            var jsObjectRefMediaStreamTrackArray = JsRuntime.GetJsPropertyArray(jsObjectRefGetVideoTracks);
            return jsObjectRefMediaStreamTrackArray
                .Select(jsObjectRef => MediaStreamTrack.Create(JsRuntime, jsObjectRef))
                .ToList();
        }

        public List<IMediaStreamTrack> GetAudioTracks()
        {
            var jsObjectRefGetAudioTracks = JsRuntime.CallJsMethod<JsObjectRef>(NativeObject, "getAudioTracks");
            var jsObjectRefMediaStreamTrackArray = JsRuntime.GetJsPropertyArray(jsObjectRefGetAudioTracks);
            return jsObjectRefMediaStreamTrackArray
                .Select(jsObjectRef => MediaStreamTrack.Create(JsRuntime, jsObjectRef))
                .ToList();
        }

        public void AddTrack(IMediaStreamTrack track) =>
            JsRuntime.CallJsMethodVoid(NativeObject, "addTrack", track.NativeObject);

        public void RemoveTrack(IMediaStreamTrack track) =>
            JsRuntime.CallJsMethodVoid(NativeObject, "removeTrack", track.NativeObject);

        public void SetElementReferenceSrcObject(object/*ElementReference*/ elementReference)
        {
            JsRuntime.SetJsProperty(elementReference, "srcObject", NativeObject);

            //await JsRuntime.InvokeVoidAsync(
            //    "objectRef.set",
            //    new object[]
            //    {
            //        elementReference,
            //        "srcObject",
            //        JsObjectRef
            //    });

        }

    }
}
