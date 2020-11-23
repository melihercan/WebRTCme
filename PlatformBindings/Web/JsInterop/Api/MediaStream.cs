using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebRtcJsInterop.Extensions;
using WebRtcJsInterop.Interops;
using WebRTCme;

namespace WebRtcJsInterop.Api
{
    internal class MediaStream : ApiBase, IMediaStream
    {
        public Task<bool> Active => throw new NotImplementedException();

        ////public Task<bool> Ended => throw new NotImplementedException();

        public Task<string> Id => throw new NotImplementedException();

        private MediaStream(IJSRuntime jsRuntime, JsObjectRef jsObjectRef) : base(jsRuntime, jsObjectRef) { }


        public async Task<List<IMediaStreamTrack>> GetTracks()
        {
            var jsObjectRef = await JsRuntime.CallJsMethod<JsObjectRef>(SelfNativeObject, "getTracks");
            var jsObjectRefMediaStreamTrackArray = await JsRuntime.GetJsPropertyArray(jsObjectRef);
            var mediaStreamTracks = new List<IMediaStreamTrack>();
            foreach(var jsObjectRefMediaStreamTrack in jsObjectRefMediaStreamTrackArray)
            {
                mediaStreamTracks.Add(MediaStreamTrack.CreateAsync(JsRuntime, jsObjectRefMediaStreamTrack));
            }
            return mediaStreamTracks;
        }

        public async Task SetElementReferenceSrcObjectAsync(object/*ElementReference*/ elementReference)
        {
            await JsRuntime.SetJsProperty(elementReference, "srcObject", SelfNativeObject);

            //await JsRuntime.InvokeVoidAsync(
            //    "objectRef.set",
            //    new object[]
            //    {
            //        elementReference,
            //        "srcObject",
            //        JsObjectRef
            //    });

        }

        internal static async Task<IMediaStream> CreateAsync(IJSRuntime jsRuntime)
        {
            var jsObjectRef = await jsRuntime.CreateJsObject("window", "MediaStream");
            var mediaStream = new MediaStream(jsRuntime, jsObjectRef);
            return mediaStream;
        }
        internal static IMediaStream Create(IJSRuntime jsRuntime, JsObjectRef jsObjectRefMediaStream)
        {
            var mediaStream = new MediaStream(jsRuntime, jsObjectRefMediaStream);
            return mediaStream;
        }

        public Task<IMediaStream> Clone()
        {
            throw new NotImplementedException();
        }

        public Task<IMediaStreamTrack> GetTrackById(string id)
        {
            throw new NotImplementedException();
        }

        public Task<List<IMediaStreamTrack>> GetVideoTracks()
        {
            throw new NotImplementedException();
        }

        public Task<List<IMediaStreamTrack>> GetAudioTracks()
        {
            throw new NotImplementedException();
        }

        public Task AddTrack(IMediaStreamTrack track)
        {
            throw new NotImplementedException();
        }

        public Task RemoveTrack(IMediaStreamTrack track)
        {
            throw new NotImplementedException();
        }
    }
}
